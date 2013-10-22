namespace Demo.SmartWorkers.Messages
{
    public interface IPatientChanged
    {
        int MedicalRecordNumber { get; }
        int SequenceNumber { get; }
    }
}
