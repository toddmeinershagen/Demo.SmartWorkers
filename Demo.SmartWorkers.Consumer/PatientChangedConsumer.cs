using System;
using System.Threading;
using Demo.SmartWorkers.Data;
using Demo.SmartWorkers.Messages;
using MassTransit;
using MongoDB.Driver;

namespace Demo.SmartWorkers.Consumer
{
    public class PatientChangedConsumer : Consumes<IPatientChanged>.Context
    {
        private readonly IPatientLockRepository _patientLockRepository;
        private readonly IPatientVersionRepository _patientVersionRepository;
        private readonly IPatientChangedSnapshotRepository _patientChangedSnapshotRepository;

        public PatientChangedConsumer()
            : this(new PatientLockRepository(), new PatientVersionRepository("patientVersionForConsumer"), new PatientChangedSnapshotRepository())
        {}

        public PatientChangedConsumer(IPatientLockRepository patientLockRepository, IPatientVersionRepository patientVersionRepository, IPatientChangedSnapshotRepository patientChangedSnapshotRepository)
        {
            _patientLockRepository = patientLockRepository;
            _patientVersionRepository = patientVersionRepository;
            _patientChangedSnapshotRepository = patientChangedSnapshotRepository;
        }

        public void Consume(IConsumeContext<IPatientChanged> context)
        {
            //Throttle(.125);
            Throttle(0);

            var message = context.Message;

            if (_patientLockRepository.DoesNotExistFor(message.FacilityId, message.MedicalRecordNumber))
            {                
                try
                {
                    _patientLockRepository.Insert(new PatientLock { FacilityId = message.FacilityId, MedicalRecordNumber = message.MedicalRecordNumber });

                    var patientVersion = _patientVersionRepository.FindOne(message.FacilityId, message.MedicalRecordNumber);
                    if (DoesNotExist(patientVersion) || IsNextVersion(patientVersion, message))
                    {
                        var messageToPersist = new PatientChangedSnapshot(message);
                        var successful = _patientChangedSnapshotRepository.Insert(messageToPersist);

                        if (!successful)
                        {
                            _patientVersionRepository.Increment(message.FacilityId, message.MedicalRecordNumber);
                            Console.WriteLine("Persisted context for MRN::{0}", message.MedicalRecordNumber);
                        }
                    }
                    else
                    {
                        context.RetryLater();
                    }

                    _patientLockRepository.Remove(message.FacilityId, message.MedicalRecordNumber);
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

        private static bool IsNextVersion(PatientVersion patientVersion, IPatientChanged message)
        {
            return patientVersion.Version == message.Version - 1;
        }

        private static bool DoesNotExist(PatientVersion patientVersion)
        {
            return patientVersion == null;
        }

        private void Throttle(double seconds)
        {
            var throttleSeconds = Convert.ToInt32(Math.Round(seconds*1000, 0));
            Thread.Sleep(throttleSeconds);
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