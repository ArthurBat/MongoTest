using MongoDB.Bson.Serialization.Attributes;

namespace MongoTest2
{
    public class Person2
    {
        // Каждый объект в базе данных имеет поле _id, которое выполняет роль уникального идентификатора объекта. Используя атрибут BsonId мы можем явно 
        // установить свойство, которое будет выполнять роль идентификатора:
        [BsonId]
        public int PersonId { get; set; }
        public string Name { get; set; }
    }
}
