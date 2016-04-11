using System.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace MongoMVC.Models
{
    public class ComputerContext
    {
        MongoClient client;
        IMongoDatabase database;
        GridFSBucket gridFS;

        public ComputerContext()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString;
            var con = new MongoUrlBuilder(connectionString);

            client = new MongoClient(connectionString);
            database = client.GetDatabase(con.DatabaseName);
//            gridFS = new MongoGridFS(
//                new MongoServer(
//                    new MongoServerSettings { Server = con.Server }),
//                con.DatabaseName,
//                new MongoGridFSSettings()
//            );
            gridFS = new GridFSBucket(database);
        }

        public IMongoCollection<Computer> Computers
        {
            get { return database.GetCollection<Computer>("Computers"); }
        }

        public IGridFSBucket GridFS
        {
            get { return gridFS; }
        }
    }
}
