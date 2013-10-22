using System;
using Demo.SmartWorkers.Messages;
using MassTransit;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Demo.SmartWorkers.Consumer
{
    public class PatientChangedConsumer : Consumes<IPatientChanged>.Selected
    {
        public void Consume(IPatientChanged message)
        {
            var database = GetDatabase();
            var patientChangedSnapshots = database.GetCollection<PatientChangedSnapshot>("patientChangedSnapshots");

            var messageToPersist = new PatientChangedSnapshot(message);
            patientChangedSnapshots.Insert(messageToPersist);
            Console.WriteLine("Persisted message for MRN::{0}", message.MedicalRecordNumber);

            var patientLocks = database.GetCollection<PatientLock>("patientLocks");

            var query = Query<PatientLock>.EQ(e => e.MedicalRecordNumber, message.MedicalRecordNumber);
            patientLocks.Remove(query);
        }

        public bool Accept(IPatientChanged message)
        {
            //check for lock existence.
            var database = GetDatabase();
            var patientLocks = database.GetCollection<PatientLock>("patientLocks");

            var query = Query<PatientLock>.EQ(e => e.MedicalRecordNumber, message.MedicalRecordNumber);
            var patientLock = patientLocks.FindOne(query);

            if (patientLock == null)
            {
                var options = new MongoInsertOptions();
                var concern = new WriteConcern();
                options.WriteConcern = concern;

                patientLocks.Insert(new PatientLock {MedicalRecordNumber = message.MedicalRecordNumber}, options);
                return true;
            }

            return false;
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