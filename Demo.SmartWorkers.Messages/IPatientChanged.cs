namespace Demo.SmartWorkers.Messages
{
    public interface IPatientChanged
    {
        int FacilityId { get; }
        int MedicalRecordNumber { get; }
        int Version { get; }
    }
}
