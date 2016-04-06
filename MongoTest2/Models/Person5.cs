using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoTest2
{
    public class Person5
    {
        // BsonRepresentation
        // Еще один атрибут BsonRepresentation отвечает за представление свойства в базе данных. Например:
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        [BsonRepresentation(BsonType.String)]
        public int Age { get; set; }

        // В этом случае для свойства Id указывается, что оно будет выполнять роль идентификатора и в базе данных соответствующее поле будет иметь тип ObjectId. 
        // А вот свойству целочисленному Age в базе данных будет соответствовать строковое поле Age из-за применения атрибута [BsonRepresentation(BsonType.String)].
    }
}
