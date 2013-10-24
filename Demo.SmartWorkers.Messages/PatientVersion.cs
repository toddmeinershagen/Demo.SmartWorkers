using MongoDB.Bson.Serialization.Attributes;

namespace Demo.SmartWorkers.Messages
{
    [BsonIgnoreExtraElements]
    public class PatientVersion
    {
        public int FacilityId { get; set; }
        public int MedicalRecordNumber { get; set; }
        public int Version { get; set; }
    }
}
