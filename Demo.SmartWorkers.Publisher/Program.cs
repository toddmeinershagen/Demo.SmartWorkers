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
            service.Publish(numberToPublish);

            Console.WriteLine("Published {0} messages", numberToPublish);
            Console.ReadLine();
        }
    }
}
