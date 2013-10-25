using Demo.SmartWorkers.Messages;

namespace Demo.SmartWorkers.Data
{
    public interface IPatientVersionRepository
    {
        PatientVersion FindOne(int facilityId, int medicalRecordNumber);
        int Increment(int facilityId, int medicalRecordNumber);
        bool DoesNotExistFor(int facilityId, int medicalRecordNumber);
    }
}