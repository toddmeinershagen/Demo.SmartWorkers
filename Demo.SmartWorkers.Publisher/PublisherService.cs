using System;
using System.Configuration;
using System.Threading;
using Demo.SmartWorkers.Core;
using Demo.SmartWorkers.Core.Data;
using MassTransit;

namespace Demo.SmartWorkers.Publisher
{
    public class PublisherService
    {
        private readonly ILogger _logger;
        private readonly IPatientVersionRepository _patientVersionRepository;
        private readonly IServiceBus _bus;
        private readonly Random _generator = new Random();

        public PublisherService(ILogger logger, IPatientVersionRepository patientVersionRepository, IServiceBus bus)
        {
            _logger = logger;
            _patientVersionRepository = patientVersionRepository;
            _bus = bus;
            GetAppSetting = (name) => ConfigurationManager.AppSettings[name];
            GetNextRandomNumber = (min, max) => _generator.Next(min, max);
        }

        public void Publish(int numberToPublish, PatientChanged[] patientChangedMessages)
        {
            for (var counter = 0; counter < numberToPublish; counter++)
            {
                var index = GetNextRandomNumber(0, patientChangedMessages.Length);
                var message = patientChangedMessages[index];

                var previousVersion = _patientVersionRepository.RemoveIfExpired(message.FacilityId, message.MedicalRecordNumber, VersionExpirationInMinutes);
                message.Version = _patientVersionRepository.Increment(message.FacilityId, message.MedicalRecordNumber);
                message.PreviousVersion = previousVersion;

                _bus.Publish(message);

                var infoMessage = string.Format("Published message for MRN::{0}", message.MedicalRecordNumber);
                _logger.Info(infoMessage);

                var throttleInSeconds = Convert.ToDouble(GetAppSetting("throttleInSeconds"));
                Throttle(throttleInSeconds);
            }
        }

        private void Throttle(double seconds)
        {
            var throttleSeconds = Convert.ToInt32(Math.Round(seconds*1000, 0));
            Thread.Sleep(throttleSeconds);
        }

        public int VersionExpirationInMinutes
        {
            get { return Convert.ToInt32(GetAppSetting("versionExpirationInMinutes")); }
        }

        public Func<string, string> GetAppSetting { get; set; }

        public Func<int, int, int> GetNextRandomNumber { get; set; } 
    }
}