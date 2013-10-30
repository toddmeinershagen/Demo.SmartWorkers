using System;
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
                .Set(e => e.UtcTimeStamp, DateTime.UtcNow)
                .Inc(e => e.Version, 1);

            var result = patientVersions.FindAndModify(query, SortBy.Null, update, true, true);
            return GetVersionFromResult(result);
        }

        public void Update(PatientVersion patientVersion)
        {
            var database = GetDatabase();
            var patientVersions = database.GetCollection<PatientVersion>(_collectionName);

            var query = Query.And(
                Query<PatientLock>.EQ(e => e.FacilityId, patientVersion.FacilityId),
                Query<PatientLock>.EQ(e => e.MedicalRecordNumber, patientVersion.MedicalRecordNumber));

            var options = new MongoInsertOptions();
            var concern = new WriteConcern();
            options.WriteConcern = concern;

            var update = Update<PatientVersion>
                .Set(e => e.FacilityId, patientVersion.FacilityId)
                .Set(e => e.MedicalRecordNumber, patientVersion.MedicalRecordNumber)
                .Set(e => e.UtcTimeStamp, DateTime.UtcNow)
                .Set(e => e.Version, patientVersion.Version);

            patientVersions.FindAndModify(query, SortBy.Null, update, true, true);
        }

        public void Remove(int facilityId, int medicalRecordNumber)
        {
            var query = Query.And(
                Query<PatientVersion>.EQ(e => e.FacilityId, facilityId),
                Query<PatientVersion>.EQ(e => e.MedicalRecordNumber, medicalRecordNumber));

            var database = GetDatabase();
            var patientVersions = database.GetCollection<PatientLock>(_collectionName);

            patientVersions.Remove(query);
        }

        public int RemoveIfExpired(int facilityId, int medicalRecordNumber, int expirationInMinutes)
        {
            var version = FindOne(facilityId, medicalRecordNumber);

            if (version == null)
                return 0;

            var minTimeStamp = DateTime.UtcNow.AddMinutes(-1 * expirationInMinutes);

            if (version.UtcTimeStamp <= minTimeStamp)
                Remove(facilityId, medicalRecordNumber);

            return version.Version;
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