using System;
using System.Configuration;
using MassTransit;

namespace Demo.SmartWorkers.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            int consumerId = 1;

            if (args.Length > 0)
            {
                consumerId = Convert.ToInt32(args[0]);
            }

            var consumerUrlFormat = ConfigurationManager.AppSettings["consumerUrlFormat"];
            var consumerUrl = string.Format(consumerUrlFormat, consumerId);

            Bus.Initialize(sbc =>
            {
                sbc.UseRabbitMq();
                sbc.ReceiveFrom(consumerUrl);
                sbc.SetConcurrentConsumerLimit(4);
                sbc.Subscribe(subs => subs.Consumer<PatientChangedConsumer>());
            });

            Console.WriteLine("Listening from {0} for messages...", consumerUrl);
        }
    }
}
