using Demo.SmartWorkers.Core;
using MongoDB.Bson.Serialization.Attributes;

namespace Demo.SmartWorkers.Publisher
{
    [BsonIgnoreExtraElements]
    public class PatientChanged : IPatientChanged
    {
        public int FacilityId { get; set; }
        public int MedicalRecordNumber { get; set; }
        public int Version { get; set; }
    }
}