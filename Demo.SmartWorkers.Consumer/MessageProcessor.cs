using Demo.SmartWorkers.Data;
using Demo.SmartWorkers.Messages;

namespace Demo.SmartWorkers.Consumer
{
    public class MessageProcessor : IMessageProcessor
    {
        private readonly IPatientChangedSnapshotRepository _patientChangedSnapshotRepository;

        public MessageProcessor(IPatientChangedSnapshotRepository patientChangedSnapshotRepository)
        {
            _patientChangedSnapshotRepository = patientChangedSnapshotRepository;
        }

        public bool Process(IPatientChanged message)
        {
            var messageToPersist = new PatientChangedSnapshot(message);
            return _patientChangedSnapshotRepository.Insert(messageToPersist);
        }
    }
}