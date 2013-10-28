using System;
using System.Configuration;
using Demo.SmartWorkers.Data;
using Demo.SmartWorkers.Messages;
using MassTransit;

namespace Demo.SmartWorkers.Consumer
{
    public class LockingPatientChangedConsumer : Consumes<IPatientChanged>.Context
    {
        private readonly Consumes<IPatientChanged>.Context _consumer;
        private readonly IPatientLockRepository _patientLockRepository;

        public LockingPatientChangedConsumer(Consumes<IPatientChanged>.Context consumer, IPatientLockRepository patientLockRepository)
        {
            _consumer = consumer;
            _patientLockRepository = patientLockRepository;
        }

        public void Consume(IConsumeContext<IPatientChanged> context)
        {
            var message = context.Message;

            var expirationInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["expirationInMinutes"]);
            _patientLockRepository.Expire(expirationInMinutes);

            if (_patientLockRepository.DoesNotExistFor(message.FacilityId, message.MedicalRecordNumber))
            {                
                try
                {
                    _patientLockRepository.Insert(new PatientLock { FacilityId = message.FacilityId, MedicalRecordNumber = message.MedicalRecordNumber });
                    _consumer.Consume(context);
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
    }
}
