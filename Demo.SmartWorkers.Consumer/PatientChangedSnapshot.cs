using System;
using Demo.SmartWorkers.Messages;

namespace Demo.SmartWorkers.Consumer
{
    public class PatientChangedSnapshot : IPatientChanged
    {
        public PatientChangedSnapshot(IPatientChanged message)
        {
            MedicalRecordNumber = message.MedicalRecordNumber;
            SequenceNumber = message.SequenceNumber;
            TimeStamp = DateTime.Now;
        }

        public int MedicalRecordNumber { get; private set; }
        public int SequenceNumber { get; private set; }
        public DateTime TimeStamp { get; private set; }
    }
}