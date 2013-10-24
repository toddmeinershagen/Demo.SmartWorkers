using System;
using System.Configuration;
using System.Threading;
using Demo.SmartWorkers.Messages;
using MassTransit;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Newtonsoft.Json.Linq;

namespace Demo.SmartWorkers.Publisher
{
    class Program
    {
        static void Main(string[] args)
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

            var publisherUrl = ConfigurationManager.AppSettings["publisherUrl"];

            Bus.Initialize(sbc =>
            {
                sbc.UseRabbitMq();
                sbc.ReceiveFrom(string.Format(publisherUrl));
            });


            Console.WriteLine("Are you ready?  (Hit ENTER)");
            Console.ReadLine();

            var generator = new Random();

            var database = GetDatabase();
            var patientVersions = database.GetCollection<PatientVersion>("patientVersionForPublisher");

            const int totalCount = 100;
            for (var counter = 0; counter < totalCount; counter++)
            {
                var index = generator.Next(0, patientChangedMessages.Length - 1);
                var message = patientChangedMessages[index];

                var query = Query.And(
                    Query<PatientVersion>.EQ(e => e.FacilityId, message.FacilityId),
                    Query<PatientVersion>.EQ(e => e.MedicalRecordNumber, message.MedicalRecordNumber));
                var update = Update<PatientVersion>
                    .Set(e => e.FacilityId, message.FacilityId)
                    .Set(e => e.MedicalRecordNumber, message.MedicalRecordNumber)
                    .Inc(e => e.Version, 1);

                var result = patientVersions.FindAndModify(query, SortBy.Null, update, true, true);
                message.Version = GetVersionFromResult(result);
                Bus.Instance.Publish(message);

                Console.WriteLine("Published message for MRN::{0}", message.MedicalRecordNumber);
                Thread.Sleep(250);
            }

            Console.WriteLine("Published {0} messages", totalCount);
            Console.ReadLine();
        }

        private static int GetVersionFromResult(FindAndModifyResult result)
        {
            var value = result.Response["value"];
            return value.IsBsonNull ? 1 : value["Version"].AsInt32;
        }

        private static MongoDatabase GetDatabase()
        {
            const string connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            var database = server.GetDatabase("smartworkers");

            return database;
        }
    }
}
