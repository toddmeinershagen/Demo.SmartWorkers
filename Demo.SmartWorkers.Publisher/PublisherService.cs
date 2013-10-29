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

        public PublisherService(ILogger logger, IPatientVersionRepository patientVersionRepository, IServiceBus bus)
        {
            _logger = logger;
            _patientVersionRepository = patientVersionRepository;
            _bus = bus;
        }

        public void Execute()
        {
            var patientChangedMessages = new[]
                {
                    new PatientChanged {FacilityId = 1, MedicalRecordNumber = 12700},
                    new PatientChanged {FacilityId = 1, MedicalRecordNumber = 13567},
                    new PatientChanged {FacilityId = 2, MedicalRecordNumber = 14726}, 
                    new PatientChanged {FacilityId = 3, MedicalRecordNumber = 18750},
                    new PatientChanged {FacilityId = 2, MedicalRecordNumber = 12701}, 
                    new PatientChanged {FacilityId = 1, MedicalRecordNumber = 13568},
                    new PatientChanged {FacilityId = 3, MedicalRecordNumber = 14725}, 
                    new PatientChanged {FacilityId = 3, MedicalRecordNumber = 18751}
                };

            Console.WriteLine("Are you ready?  (Hit ENTER)");
            Console.ReadLine();

            var generator = new Random();

            const int totalCount = 1000;
            for (var counter = 0; counter < totalCount; counter++)
            {
                var index = generator.Next(0, patientChangedMessages.Length);
                var message = patientChangedMessages[index];

                message.Version = _patientVersionRepository.Increment(message.FacilityId, message.MedicalRecordNumber);
                _bus.Publish(message);

                var infoMessage = string.Format("Published message for MRN::{0}", message.MedicalRecordNumber);
                _logger.Info(infoMessage);

                var throttleInSeconds = Convert.ToDouble(ConfigurationManager.AppSettings["throttleInSeconds"]);
                Throttle(throttleInSeconds);
            }

            Console.WriteLine("Published {0} messages", totalCount);
            Console.ReadLine();
        }

        private void Throttle(double seconds)
        {
            var throttleSeconds = Convert.ToInt32(Math.Round(seconds*1000, 0));
            Thread.Sleep(throttleSeconds);
        }
    }
}