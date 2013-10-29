namespace Demo.SmartWorkers.Core.Data
{
    public interface IPatientLockRepository
    {
        bool DoesNotExistFor(int facilityId, int medicalRecordNumber);
        void Insert(PatientLock patientLock);
        void Remove(int facilityId, int medicalRecordNumber);
        void Expire(int expirationInMinutes);
    }
}