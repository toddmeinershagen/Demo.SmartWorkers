using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Demo.SmartWorkers.Core
{
    [BsonIgnoreExtraElements]
    public class PatientVersion
    {
        public PatientVersion()
        {}

        public PatientVersion(IPatientChanged patientChanged)
        {
            FacilityId = patientChanged.FacilityId;
            MedicalRecordNumber = patientChanged.MedicalRecordNumber;
            Version = patientChanged.Version;
            UtcTimeStamp = DateTime.UtcNow;
        }

        public int FacilityId { get; set; }
        public int MedicalRecordNumber { get; set; }
        public int Version { get; set; }
        public DateTime UtcTimeStamp { get; set; }
    }
}
