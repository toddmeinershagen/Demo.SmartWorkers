using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Demo.SmartWorkers.Core
{
    [BsonIgnoreExtraElements]
    public class PatientLock
    {
        public PatientLock()
        {
            UtcTimeStamp = DateTime.UtcNow;
        }

        public int FacilityId { get; set; }
        public int MedicalRecordNumber { get; set; }
        public DateTime UtcTimeStamp { get; set; }
    }
}