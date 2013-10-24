using System;
using Demo.SmartWorkers.Messages;

namespace Demo.SmartWorkers.Consumer
{
    public class PatientChangedSnapshot : IPatientChanged
    {
        public PatientChangedSnapshot(IPatientChanged message)
        {
            FacilityId = message.FacilityId;
            MedicalRecordNumber = message.MedicalRecordNumber;
            Version = message.Version;
            TimeStamp = DateTime.Now;
        }

        public int FacilityId { get; private set; }
        public int MedicalRecordNumber { get; private set; }
        public int Version { get; private set; }
        public DateTime TimeStamp { get; private set; }
    }
}