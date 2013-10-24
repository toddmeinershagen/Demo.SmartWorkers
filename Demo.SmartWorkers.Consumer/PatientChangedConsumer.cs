using System;
using Demo.SmartWorkers.Messages;
using MassTransit;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Demo.SmartWorkers.Consumer
{
    public class PatientChangedConsumer : Consumes<IPatientChanged>.Context
    {
        public void Consume(IConsumeContext<IPatientChanged> context)
        {
            var message = context.Message;

            //check for lock existence.
            var database = GetDatabase();
            var patientLocks = database.GetCollection<PatientLock>("patientLocks");

            var query = Query.And(
                Query<PatientLock>.EQ(e => e.FacilityId, message.FacilityId),
                Query<PatientLock>.EQ(e => e.MedicalRecordNumber, message.MedicalRecordNumber));

            var patientLock = patientLocks.FindOne(query);

            if (patientLock == null)
            {                
                try
                {
                    var options = new MongoInsertOptions();
                    var concern = new WriteConcern();
                    options.WriteConcern = concern;

                    patientLocks.Insert(new PatientLock { FacilityId = message.FacilityId, MedicalRecordNumber = message.MedicalRecordNumber }, options);

                    var patientVersions = database.GetCollection<PatientVersion>("patientVersionForConsumer");
                    var patientVersion = patientVersions.FindOne(query);

                    if (patientVersion == null || patientVersion.Version == message.Version - 1)
                    {
                        var patientChangedSnapshots = database.GetCollection<PatientChangedSnapshot>("patientChangedSnapshots");

                        var messageToPersist = new PatientChangedSnapshot(message);
                        var result = patientChangedSnapshots.Insert(messageToPersist);

                        if (!result.HasLastErrorMessage)
                        {
                            var update = Update<PatientVersion>
                                .Set(e => e.FacilityId, message.FacilityId)
                                .Set(e => e.MedicalRecordNumber, message.MedicalRecordNumber)
                                .Inc(e => e.Version, 1);

                            patientVersions.Update(query, update, UpdateFlags.Upsert);

                            Console.WriteLine("Persisted context for MRN::{0}", message.MedicalRecordNumber);
                        }
                    }
                    else
                    {
                        context.RetryLater();
                    }

                    patientLocks.Remove(query);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    context.RetryLater();
                }

                return;
            }

            context.RetryLater();
        }

        private MongoDatabase GetDatabase()
        {
            const string connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            var database = server.GetDatabase("smartworkers");

            return database;
        }
    }
}