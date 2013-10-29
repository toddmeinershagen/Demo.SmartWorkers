using Demo.SmartWorkers.Core;
using Demo.SmartWorkers.Core.Data;

namespace Demo.SmartWorkers.Consumer.Processors
{
    public class VersionedMessageProcessor : IMessageProcessor
    {
        private readonly IMessageProcessor _messageProcessor;
        private readonly IPatientVersionRepository _patientVersionRepository;

        public VersionedMessageProcessor(IMessageProcessor messageProcessor, IPatientVersionRepository patientVersionRepository)
        {
            _messageProcessor = messageProcessor;
            _patientVersionRepository = patientVersionRepository;
        }

        public bool Process(IPatientChanged message)
        {
            var latestVersion = _patientVersionRepository.FindOne(message.FacilityId, message.MedicalRecordNumber);

            if (message.IsExpirationRequest(latestVersion))
            {
                _patientVersionRepository.Remove(message.FacilityId, message.MedicalRecordNumber);
                latestVersion = null;
            }

            if (message.IsNextToBeProcessed(latestVersion))
            {
                if (_messageProcessor.Process(message))
                {
                    _patientVersionRepository.Increment(message.FacilityId, message.MedicalRecordNumber);
                    return true;
                }
            }

            return false;
        }
    }
}