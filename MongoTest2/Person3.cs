using MongoDB.Bson.Serialization.Attributes;

namespace MongoTest2
{
    public class Person3
    {
        // Используя атрибуты, мы можем управлять настройкой классов моделей и их сериализацией в документы mongodb. Например:
        [BsonElement("First Name")]
        public string Name { get; set; }
        [BsonIgnore]
        public string Surname { get; set; }
        public int Age { get; set; }
        public Company Company { get; set; }
    }
}