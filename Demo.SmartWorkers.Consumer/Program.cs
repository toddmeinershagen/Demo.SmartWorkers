using System;
using Demo.SmartWorkers.Messages;
using MassTransit;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

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

            var consumerUrl = string.Format("rabbitmq://localhost/Demo.SmartWorkers.Consumer.{0}", consumerId);

            Bus.Initialize(sbc =>
            {
                sbc.UseRabbitMq();
                sbc.ReceiveFrom(consumerUrl);
                sbc.Subscribe(subs => subs.Consumer<PatientChangedConsumer>());
            });

            Console.WriteLine("Listening from {0} for messages...", consumerUrl);
        }
    }

    public class PatientChangedConsumer : Consumes<IPatientChanged>.Selected
    {
        public void Consume(IPatientChanged message)
        {
            var database = GetDatabase();
            var patientChangedSnapshots = database.GetCollection<PatientChangedSnapshot>("patientChangedSnapshots");

            var messageToPersist = new PatientChangedSnapshot(message);
            patientChangedSnapshots.Insert(messageToPersist);
            Console.WriteLine("Persisted message for MRN::{0}", message.MedicalRecordNumber);

            var patientLocks = database.GetCollection<PatientLock>("patientLocks");

            var query = Query<PatientLock>.EQ(e => e.MedicalRecordNumber, message.MedicalRecordNumber);
            patientLocks.Remove(query);
        }

        public bool Accept(IPatientChanged message)
        {
            //check for lock existence.
            var database = GetDatabase();
            var patientLocks = database.GetCollection<PatientLock>("patientLocks");

            var query = Query<PatientLock>.EQ(e => e.MedicalRecordNumber, message.MedicalRecordNumber);
            var patientLock = patientLocks.FindOne(query);

            if (patientLock == null)
            {
                patientLocks.Insert(new PatientLock {MedicalRecordNumber = message.MedicalRecordNumber});
                return true;
            }

            return false;
        }

        private MongoDatabase GetDatabase()
        {
            const string connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            var database = server.GetDatabase("smartworkers");

            return database;
        }
    }

    public class PatientLock
    {
        public int MedicalRecordNumber { get; set; }
    }

    public class PatientChangedSnapshot : IPatientChanged
    {
        public PatientChangedSnapshot(IPatientChanged message)
        {
            MedicalRecordNumber = message.MedicalRecordNumber;
            SequenceNumber = message.SequenceNumber;
            TimeStamp = DateTime.Now;
        }

        public int MedicalRecordNumber { get; private set; }
        public int SequenceNumber { get; private set; }
        public DateTime TimeStamp { get; private set; }
    }
}
