using Demo.SmartWorkers.Messages;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Demo.SmartWorkers.Data
{
    public class PatientLockRepository : BaseRepository, IPatientLockRepository
    {
        public bool DoesNotExistFor(int facilityId, int medicalRecordNumber)
        {
            var database = GetDatabase();
            var patientLocks = database.GetCollection<PatientLock>("patientLocks");

            var query = Query.And(
                Query<PatientLock>.EQ(e => e.FacilityId, facilityId),
                Query<PatientLock>.EQ(e => e.MedicalRecordNumber, medicalRecordNumber));

            var patientLock = patientLocks.FindOne(query);

            return patientLock == null;   
        }

        public void Insert(PatientLock patientLock)
        {
            var options = new MongoInsertOptions();
            var concern = new WriteConcern();
            options.WriteConcern = concern;

            var database = GetDatabase();
            var patientLocks = database.GetCollection<PatientLock>("patientLocks");

            patientLocks.Insert(patientLock, options);
        }

        public void Remove(int facilityId, int medicalRecordNumber)
        {
            var query = Query.And(
                Query<PatientLock>.EQ(e => e.FacilityId, facilityId),
                Query<PatientLock>.EQ(e => e.MedicalRecordNumber, medicalRecordNumber));

            var database = GetDatabase();
            var patientLocks = database.GetCollection<PatientLock>("patientLocks");

            patientLocks.Remove(query);
        }
    }
}