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
            var patientVersion = _patientVersionRepository.FindOne(message.FacilityId, message.MedicalRecordNumber);

            if (message.Version == 1 && (DoesNotExist(patientVersion) || message.PreviousVersion == patientVersion.Version) || (IsNextVersion(patientVersion, message)))
            {
                if (_messageProcessor.Process(message))
                {
                    _patientVersionRepository.Increment(message.FacilityId, message.MedicalRecordNumber);
                    return true;
                }
            }

            return false;
        }

        private bool IsNextVersion(PatientVersion patientVersion, IPatientChanged message)
        {
            return Exists(patientVersion) && patientVersion.Version == message.Version - 1;
        }

        private bool Exists(PatientVersion patientVersion)
        {
            return !DoesNotExist(patientVersion);
        }

        private bool DoesNotExist(PatientVersion patientVersion)
        {
            return patientVersion == null;
        }
    }
}