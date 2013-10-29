namespace Demo.SmartWorkers.Core.Data
{
    public interface IPatientVersionRepository
    {
        PatientVersion FindOne(int facilityId, int medicalRecordNumber);
        int Increment(int facilityId, int medicalRecordNumber);
        bool DoesNotExistFor(int facilityId, int medicalRecordNumber);
        void Remove(int facilityId, int medicalRecordNumber);
    }
}