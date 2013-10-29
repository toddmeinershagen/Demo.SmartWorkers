using Demo.SmartWorkers.Core;
using Demo.SmartWorkers.Core.Data;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Demo.SmartWorkers.Data
{
    public class PatientVersionRepository : BaseRepository, IPatientVersionRepository
    {
        private readonly string _collectionName;

        public PatientVersionRepository(string collectionName)
        {
            _collectionName = collectionName;
        }

        public PatientVersion FindOne(int facilityId, int medicalRecordNumber)
        {
            var database = GetDatabase();
            var patientVersions = database.GetCollection<PatientVersion>(_collectionName);

            var query = Query.And(
                Query<PatientLock>.EQ(e => e.FacilityId, facilityId),
                Query<PatientLock>.EQ(e => e.MedicalRecordNumber, medicalRecordNumber));

            return patientVersions.FindOne(query);
        }

        public int Increment(int facilityId, int medicalRecordNumber)
        {
            var database = GetDatabase();
            var patientVersions = database.GetCollection<PatientVersion>(_collectionName);

            var query = Query.And(
                Query<PatientLock>.EQ(e => e.FacilityId, facilityId),
                Query<PatientLock>.EQ(e => e.MedicalRecordNumber, medicalRecordNumber));

            var options = new MongoInsertOptions();
            var concern = new WriteConcern();
            options.WriteConcern = concern;

            var update = Update<PatientVersion>
                .Set(e => e.FacilityId, facilityId)
                .Set(e => e.MedicalRecordNumber, medicalRecordNumber)
                .Inc(e => e.Version, 1);

            var result = patientVersions.FindAndModify(query, SortBy.Null, update, true, true);
            return GetVersionFromResult(result);
        }

        private int GetVersionFromResult(FindAndModifyResult result)
        {
            var value = result.Response["value"];
            return value.IsBsonNull ? 1 : value["Version"].AsInt32;
        }

        public bool DoesNotExistFor(int facilityId, int medicalRecordNumber)
        {
            var patientVersion = FindOne(facilityId, medicalRecordNumber);
            return patientVersion == null;
        }
    }
}