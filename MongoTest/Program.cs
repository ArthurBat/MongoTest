using System;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoTest
{
    class Program
    {
        protected static IMongoClient _client;
        protected static IMongoDatabase _database;

        static void Main(string[] args)
        {
            _client = new MongoClient();
            _database = _client.GetDatabase("test");

//            InsertData();

            Console.WriteLine("\n============================================\n");
            FindOrQueryData();
            Console.ReadLine();
            Console.WriteLine("\n============================================\n");
            UpdateData();
            Console.ReadLine();
            //            Console.WriteLine("\n============================================\n");
            //            RemoveData();
            Console.WriteLine("\n============================================\n");
            DataAgregation();
            Console.ReadLine();
        }

        private static async void FindOrQueryData()
        {
            // Query for All Documents in a Collection
            var collection = _database.GetCollection<BsonDocument>("restaurants");
            var filter = new BsonDocument();
            var count = 0;
            try
            {
                using (var cursor = await collection.FindAsync(filter))
                {
                    while (await cursor.MoveNextAsync())
                    {
                        var batch = cursor.Current;
                        foreach (var document in batch)
                        {
                            // process document
                            count++;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("Query for All Documents in a Collection: " + count);

            // Query by a Top Level Field
            collection = _database.GetCollection<BsonDocument>("restaurants");
            var filter2 = Builders<BsonDocument>.Filter.Eq("borough", "Manhattan");
            var result = await collection.Find(filter2).ToListAsync();
            Console.WriteLine("Query by a Top Level Field: " + result.Count);

            // Query by a Field in an Embedded Document
            collection = _database.GetCollection<BsonDocument>("restaurants");
            var filter3 = Builders<BsonDocument>.Filter.Eq("address.zipcode", "10075");
            result = await collection.Find(filter3).ToListAsync();
            Console.WriteLine("Query by a Top Level Field: " + result.Count);

            // Query by a Field in an Array
            collection = _database.GetCollection<BsonDocument>("restaurants");
            var filter4 = Builders<BsonDocument>.Filter.Eq("grades.grade", "B");
            result = await collection.Find(filter4).ToListAsync();
            Console.WriteLine("Query by a Field in an Array: " + result.Count);

            // Greater Than Operator ($gt)
            collection = _database.GetCollection<BsonDocument>("restaurants");
            var filter5 = Builders<BsonDocument>.Filter.Gt("grades.score", 30);
            result = await collection.Find(filter5).ToListAsync();
            Console.WriteLine("Greater Than Operator ($gt): " + result.Count);

            // Less Than Operator ($lt)
            collection = _database.GetCollection<BsonDocument>("restaurants");
            var filter6 = Builders<BsonDocument>.Filter.Lt("grades.score", 10);
            result = await collection.Find(filter6).ToListAsync();
            Console.WriteLine("Less Than Operator ($lt): " + result.Count);

            // Logical AND
            collection = _database.GetCollection<BsonDocument>("restaurants");
            var builder = Builders<BsonDocument>.Filter;
            var filter7 = builder.Eq("cuisine", "Italian") & builder.Eq("address.zipcode", "10075");
            result = await collection.Find(filter7).ToListAsync();
            Console.WriteLine("Logical AND: " + result.Count);

            // Logical OR
            collection = _database.GetCollection<BsonDocument>("restaurants");
            builder = Builders<BsonDocument>.Filter;
            var filter8 = builder.Eq("cuisine", "Italian") | builder.Eq("address.zipcode", "10075");
            result = await collection.Find(filter8).ToListAsync();
            Console.WriteLine("Logical OR: " + result.Count);

            // Sort Query Results
            collection = _database.GetCollection<BsonDocument>("restaurants");
            var filter9 = new BsonDocument();
            var sort = Builders<BsonDocument>.Sort.Ascending("borough").Ascending("address.zipcode");
            result = await collection.Find(filter9).Sort(sort).ToListAsync();
            Console.WriteLine("Sort Query Results: " + result.Count);
        }

        private static async void UpdateData()
        {
            // Update Top-Level Fields
            var collection = _database.GetCollection<BsonDocument>("restaurants");
            var filter = Builders<BsonDocument>.Filter.Eq("name", "Juni");
            var update = Builders<BsonDocument>.Update
                .Set("cuisine", "American (New)")
                .CurrentDate("lastModified");
            var result = await collection.UpdateOneAsync(filter, update);
            Console.WriteLine("Update Top-Level Fields: " + result.ModifiedCount);

            // Update an Embedded Field
            collection = _database.GetCollection<BsonDocument>("restaurants");
            filter = Builders<BsonDocument>.Filter.Eq("restaurant_id", "41156888");
            update = Builders<BsonDocument>.Update.Set("address.street", "East 31st Street");
            result = await collection.UpdateOneAsync(filter, update);
            Console.WriteLine("Update an Embedded Field: " + result.ModifiedCount);

            // Update Multiple Documents
            collection = _database.GetCollection<BsonDocument>("restaurants");
            var builder = Builders<BsonDocument>.Filter;
            filter = builder.Eq("address.zipcode", "10016") & builder.Eq("cuisine", "Other");
            update = Builders<BsonDocument>.Update
                .Set("cuisine", "Category To Be Determined")
                .CurrentDate("lastModified");
        }

        private static async void RemoveData()
        {
            // Remove All Documents That Match a Condition
            var collection = _database.GetCollection<BsonDocument>("restaurants");
            var filter = Builders<BsonDocument>.Filter.Eq("borough", "Manhattan");
            var result = await collection.DeleteManyAsync(filter);
            Console.WriteLine("Remove All Documents That Match a Condition: " + result.DeletedCount);

            // Remove All Documents
            collection = _database.GetCollection<BsonDocument>("restaurants");
            filter = new BsonDocument();
            result = await collection.DeleteManyAsync(filter);
            Console.WriteLine("Remove All Documents: " + result.DeletedCount);

            // Drop a Collection
            await _database.DropCollectionAsync("restaurants");
        }

        private static async void DataAgregation()
        {
            // Group Documents by a Field and Calculate Count
            var collection = _database.GetCollection<BsonDocument>("restaurants");
            var aggregate = collection.Aggregate().Group(new BsonDocument { { "_id", "$borough" }, { "count", new BsonDocument("$sum", 1) } });
            var results = await aggregate.ToListAsync();
            Console.WriteLine("Group Documents by a Field and Calculate Count: " + results.Count);
        }

        private static void InsertData()
        {
            var document = new BsonDocument
            {
                {
                    "address", new BsonDocument
                    {
                        {"street", "2 Avenue"},
                        {"zipcode", "10075"},
                        {"building", "1480"},
                        {"coord", new BsonArray {73.9557413, 40.7720266}}
                    }
                },
                {"borough", "Manhattan"},
                {"cuisine", "Italian"},
                {
                    "grades", new BsonArray
                    {
                        new BsonDocument
                        {
                            {"date", new DateTime(2014, 10, 1, 0, 0, 0, DateTimeKind.Utc)},
                            {"grade", "A"},
                            {"score", 11}
                        },
                        new BsonDocument
                        {
                            {"date", new DateTime(2014, 1, 6, 0, 0, 0, DateTimeKind.Utc)},
                            {"grade", "B"},
                            {"score", 17}
                        }
                    }
                },
                {"name", "Vella"},
                {"restaurant_id", "41704620"}
            };

            var collection = _database.GetCollection<BsonDocument>("restaurants");
            collection.InsertOne(document);
        }
    }
}
