using MongoDB.Bson.Serialization.Attributes;

namespace Demo.SmartWorkers.Messages
{
    [BsonIgnoreExtraElements]
    public class PatientLock
    {
        public int FacilityId { get; set; }
        public int MedicalRecordNumber { get; set; }
    }
}