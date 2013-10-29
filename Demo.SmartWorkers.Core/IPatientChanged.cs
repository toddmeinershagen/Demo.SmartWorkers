namespace Demo.SmartWorkers.Core
{
    public interface IPatientChanged
    {
        int FacilityId { get; }
        int MedicalRecordNumber { get; }
        int Version { get; }
    }
}
