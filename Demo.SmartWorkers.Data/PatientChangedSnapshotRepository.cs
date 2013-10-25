using Demo.SmartWorkers.Messages;

namespace Demo.SmartWorkers.Data
{
    public class PatientChangedSnapshotRepository : BaseRepository, IPatientChangedSnapshotRepository
    {
        public bool Insert(PatientChangedSnapshot snapshot)
        {
            var database = GetDatabase();
            var patientChangedSnapshots = database.GetCollection<PatientChangedSnapshot>("patientChangedSnapshots");

            var messageToPersist = new PatientChangedSnapshot(snapshot);
            var result = patientChangedSnapshots.Insert(messageToPersist);

            return result.HasLastErrorMessage;
        }
    }
}