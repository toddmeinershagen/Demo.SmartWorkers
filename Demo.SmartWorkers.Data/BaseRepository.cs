using System.Configuration;
using MongoDB.Driver;

namespace Demo.SmartWorkers.Data
{
    public abstract class BaseRepository
    {
        protected MongoDatabase GetDatabase()
        {
            var connectionString = ConfigurationManager.AppSettings["db.connectionString"];
            var databaseName = ConfigurationManager.AppSettings["db.databaseName"];

            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            var database = server.GetDatabase(databaseName);

            return database;
        }
    }
}