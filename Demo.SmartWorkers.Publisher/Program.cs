using System;
using System.Threading;
using Demo.SmartWorkers.Messages;
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

            Bus.Initialize(sbc =>
            {
                sbc.UseRabbitMq();
                sbc.ReceiveFrom("rabbitmq://localhost/Demo.SmartWorkers.Publisher");
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

                if (counter%2 != 0)
                {
                    Thread.Sleep(500);
                }
            }

            Console.WriteLine("Published {0} messages", totalCount);
            Console.ReadLine();
        }
    }

    public class PatientChanged : IPatientChanged
    {
        public int MedicalRecordNumber { get; set; }
        public int SequenceNumber { get; set; }
    }
}
