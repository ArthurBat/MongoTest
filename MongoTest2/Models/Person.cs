using System.Collections.Generic;
using MongoDB.Bson;

namespace MongoTest2
{
    public class Person
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public Company Company;
        public List<string> Languages { get; set; }
        public string Surname { get; set; }
    }
}
