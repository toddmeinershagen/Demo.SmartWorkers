using System;
using System.Configuration;
using Demo.SmartWorkers.Core;
using Demo.SmartWorkers.Data;
using MassTransit;

namespace Demo.SmartWorkers.Publisher
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var publisherUrl = ConfigurationManager.AppSettings["publisherUrl"];
            Bus.Initialize(sbc =>
            {
                sbc.UseRabbitMq();
                sbc.ReceiveFrom(string.Format(publisherUrl));
            });

            Console.WriteLine("Are you ready?  (Hit ENTER)");
            Console.ReadLine();

            var service = new PublisherService(new ConsoleLogger(), new PatientVersionRepository("patientVersionForPublisher"), Bus.Instance);
            const int numberToPublish = 1000;
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

            service.Publish(numberToPublish, patientChangedMessages);

            Console.WriteLine("Published {0} messages", numberToPublish);
            Console.ReadLine();
        }
    }
}
