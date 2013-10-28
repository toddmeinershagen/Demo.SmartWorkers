using Demo.SmartWorkers.Messages;
using MongoDB.Driver;

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

            return WasSuccessful(result);
        }

        private static bool WasSuccessful(WriteConcernResult result)
        {
            return !result.HasLastErrorMessage;
        }
    }
}