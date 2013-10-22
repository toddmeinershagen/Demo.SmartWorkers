using System;
using System.Configuration;
using System.Threading;
using MassTransit;

namespace Demo.SmartWorkers.Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            var patientChangedMessages = new[]
                {
                    new PatientChanged {MedicalRecordNumber = 12700}, 
                    new PatientChanged {MedicalRecordNumber = 13567},
                    new PatientChanged {MedicalRecordNumber = 14726}, 
                    new PatientChanged {MedicalRecordNumber = 18750}
                };

            var publisherUrl = ConfigurationManager.AppSettings["publisherUrl"];

            Bus.Initialize(sbc =>
            {
                sbc.UseRabbitMq();
                sbc.ReceiveFrom(string.Format(publisherUrl));
            });


            Console.WriteLine("Are you ready?  (Hit ENTER)");
            Console.ReadLine();

            var generator = new Random();

            const int totalCount = 100;
            for (var counter = 0; counter < totalCount; counter++)
            {
                var index = generator.Next(0, patientChangedMessages.Length - 1);
                var message = patientChangedMessages[index];
                message.SequenceNumber = counter;
                Bus.Instance.Publish(message);
            }

            Console.WriteLine("Published {0} messages", totalCount);
            Console.ReadLine();
        }
    }
}
