using Demo.SmartWorkers.Core;

namespace Demo.SmartWorkers.Consumer
{
    public static class IPatientChangedExtensions
    {
        public static bool IsExpirationRequest(this IPatientChanged message, PatientVersion latestVersion)
        {
            return message.Version == 1 && Exists(latestVersion) && AllVersionsHaveBeenProcessedFor(message, latestVersion);
        }

        public static bool IsNextToBeProcessed(this IPatientChanged message, PatientVersion latestVersion)
        {
            return (DoesNotExist(latestVersion) && message.Version == 1) || (IsNextVersion(latestVersion, message));
        }

        private static bool AllVersionsHaveBeenProcessedFor(IPatientChanged message, PatientVersion latestVersion)
        {
            return latestVersion.Version == message.PreviousVersion;
        }

        private static bool IsNextVersion(PatientVersion latestVersion, IPatientChanged message)
        {
            return Exists(latestVersion) && latestVersion.Version == message.Version - 1;
        }

        private static bool Exists(PatientVersion latestVersion)
        {
            return !DoesNotExist(latestVersion);
        }

        private static bool DoesNotExist(PatientVersion latestVersion)
        {
            return latestVersion == null;
        }
    }
}
