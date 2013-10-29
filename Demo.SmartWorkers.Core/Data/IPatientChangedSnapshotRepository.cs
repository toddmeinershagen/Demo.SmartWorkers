namespace Demo.SmartWorkers.Core.Data
{
    public interface IPatientChangedSnapshotRepository
    {
        bool Insert(PatientChangedSnapshot snapshot);
    }
}