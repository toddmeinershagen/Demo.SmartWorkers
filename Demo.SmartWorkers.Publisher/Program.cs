using System.Configuration;
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

            var service = new PublisherService(new PatientVersionRepository("patientVersionForPublisher"), Bus.Instance);
            service.Execute();
        }
    }
}
