using Demo.SmartWorkers.Messages;

namespace Demo.SmartWorkers.Publisher
{
    public class PatientChanged : IPatientChanged
    {
        public int MedicalRecordNumber { get; set; }
        public int SequenceNumber { get; set; }
    }
}