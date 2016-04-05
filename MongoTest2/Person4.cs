using MongoDB.Bson.Serialization.Attributes;

namespace MongoTest2
{
    public class Person4
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        [BsonIgnoreIfDefault]
        public int Age { get; set; }
        [BsonIgnoreIfNull]
        public Company Company { get; set; }
    }
}
