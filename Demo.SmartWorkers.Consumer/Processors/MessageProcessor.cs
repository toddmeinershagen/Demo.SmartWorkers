using Demo.SmartWorkers.Core;
using Demo.SmartWorkers.Core.Data;
using Demo.SmartWorkers.Data;

namespace Demo.SmartWorkers.Consumer.Processors
{
    public class MessageProcessor : MessageProcessorBase, IMessageProcessor 
    {
        private readonly IPatientChangedSnapshotRepository _patientChangedSnapshotRepository;

        public MessageProcessor(ILogger logger, IPatientChangedSnapshotRepository patientChangedSnapshotRepository)
            : base(logger)
        {
            _patientChangedSnapshotRepository = patientChangedSnapshotRepository;
        }

        public bool Process(IPatientChanged message)
        {
            var messageToPersist = new PatientChangedSnapshot(message);
            if (_patientChangedSnapshotRepository.Insert(messageToPersist))
            {
                var infoMessage = string.Format("Persisted context for MRN::{0}", message.MedicalRecordNumber);
                Logger.Info(infoMessage);
                return true;
            }

            return false;
        }
    }
}