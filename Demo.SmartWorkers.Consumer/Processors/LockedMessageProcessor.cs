using System;
using System.Configuration;
using Demo.SmartWorkers.Core;
using Demo.SmartWorkers.Core.Data;
using Demo.SmartWorkers.Data;

namespace Demo.SmartWorkers.Consumer.Processors
{
    public class LockedMessageProcessor : IMessageProcessor
    {
        private readonly IMessageProcessor _messageProcessor;
        private readonly IPatientLockRepository _patientLockRepository;

        public LockedMessageProcessor(IMessageProcessor messageProcessor, IPatientLockRepository patientLockRepository)
        {
            _messageProcessor = messageProcessor;
            _patientLockRepository = patientLockRepository;
            GetAppSetting = (name) => ConfigurationManager.AppSettings[name];    
        }

        public bool Process(IPatientChanged message)
        {
            var expirationInMinutes = Convert.ToInt32(GetAppSetting("expirationInMinutes"));
            _patientLockRepository.Expire(expirationInMinutes);

            if (_patientLockRepository.DoesNotExistFor(message.FacilityId, message.MedicalRecordNumber))
            {
                _patientLockRepository.Insert(new PatientLock { FacilityId = message.FacilityId, MedicalRecordNumber = message.MedicalRecordNumber });
                var result = _messageProcessor.Process(message);
                _patientLockRepository.Remove(message.FacilityId, message.MedicalRecordNumber);
                    
                return result;
            }

            return false;
        }

        public Func<string, string> GetAppSetting { get; set; }
    }
}