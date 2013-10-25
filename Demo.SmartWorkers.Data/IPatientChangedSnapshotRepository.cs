using Demo.SmartWorkers.Messages;

namespace Demo.SmartWorkers.Data
{
    public interface IPatientChangedSnapshotRepository
    {
        bool Insert(PatientChangedSnapshot snapshot);
    }
}