using System;
using System.Configuration;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MongoTest2
{
    class Program
    {
        static void Main(string[] args)
        {
            string con =
                ConfigurationManager.ConnectionStrings["MongoTest2.Properties.Settings.MongoDb"].ConnectionString;
            MongoClient client = new MongoClient(con);
            GetDatabaseNames(client);
            Console.ReadLine();
            GetCollectionsNames(client).Wait();
            Console.ReadLine();
            WorkingWithDataModels(client);
            Console.ReadLine();
            SettingsOfModelsWithAttributs();
            Console.ReadLine();
        }

        private static async void GetDatabaseNames(MongoClient client)
        {
            using (var cursor = await client.ListDatabasesAsync())
            {
                var databaseDocuments = await cursor.ToListAsync();
                foreach (var databaseDocument in databaseDocuments)
                {
                    Console.WriteLine(databaseDocument["name"]);
                }
            }
            EndMethod();
        }

        private static async Task GetCollectionsNames(MongoClient client)
        {
            IMongoDatabase database;
            using (var cursor = await client.ListDatabasesAsync())
            {
                var dbs = await cursor.ToListAsync();
                foreach (var db in dbs)
                {
                    Console.WriteLine("В базе данных {0} имеются следующие коллекции:", db["name"]);

                    database = client.GetDatabase(db["name"].ToString());

                    using (var collCursor = database.ListCollectionsAsync())
                    {
                        var colls = collCursor.Result.ToListAsync().Result; ;
                        foreach (var col in colls)
                        {
                            Console.WriteLine(col["name"]);
                        }
                    }
                    Console.WriteLine();
                }
            }
            // Есть база данных test, в которой имеется коллекция users. И чтобы непосредственно получить эту коллекцию, можно использовать метод GetCollection, 
            // который возвращает объект IMongoCollection:
            database = client.GetDatabase("info");
            IMongoCollection<BsonDocument> col2 = database.GetCollection<BsonDocument>("users");

            // Например, совершим переход от BsonInt32 к стандартному типу int:
            BsonValue bv = new BsonInt32(20);
            int i = bv.AsInt32;

            // Создание документа

            // Для создания документа мы можем использовать одну из форм конструктора BsonDocument. Например, создадим пустой документ:
            BsonDocument doc = new BsonDocument();
            Console.WriteLine(doc);

            // Теперь создадим документ с одним элементом:
            doc = new BsonDocument { { "name", "Bill" } };
            Console.WriteLine(doc);

            // Теперь выведем на консоль значение поля name:
            doc = new BsonDocument { { "name", "Bill" } };
            Console.WriteLine(doc["name"]);
            // изменим поле name
            doc["name"] = "Tom";
            Console.WriteLine(doc.GetValue("name"));

            // Так как каждая такая пара ключ-значение представляет элемент BsonElement, то мы могли бы написать и так:
            BsonElement bel = new BsonElement("name", "Bill");
            doc = new BsonDocument(bel);
            Console.WriteLine(doc);

            // Или использовать метод Add для добавления нового элемента:
            bel = new BsonElement("name", "Bill");
            doc = new BsonDocument();
            doc.Add(bel);
            Console.WriteLine(doc);

            // Теперь создадим более сложный по составу элемент:
            doc = new BsonDocument
            {
                {"name", "Bill"},
                {"surname", "Gates"},
                {"age", new BsonInt32(48)},
                {
                    "company",
                    new BsonDocument
                    {
                        {"name", "microsoft"},
                        {"year", new BsonInt32(1974)},
                        {"price", new BsonInt32(300000)},
                    }
                }
            };
            Console.WriteLine(doc);

            // И еще пример - добавим в документ массив:
            BsonDocument chemp = new BsonDocument();
            chemp.Add("countries", new BsonArray(new[] { "Бразилия", "Аргентина", "Германия", "Нидерланды" }));
            chemp.Add("finished", new BsonBoolean(true));
            Console.WriteLine(chemp);

            EndMethod();
        }

        private static void WorkingWithDataModels(MongoClient client)
        {
            // Пространство имен MongoDB.Bson добавляет ряд функциональностей к классам C#, которые позволяют использовать объекты этих классов в качестве документов:
            Person p = new Person { Name = "Bill", Surname = "Gates", Age = 48 };
            p.Company = new Company { Name = "Microsoft", Year = 1974, Price = 300000 };

            Console.WriteLine(p.ToJson());

            // При создании документа мы можем воспользоваться как стандартным классом C#, так и классом BsonDocument, и при необходимости перейти от одного к другому. 
            // Например:
            BsonDocument doc = new BsonDocument
            {
                {"Name","Bill"},
                {"Surname", "Gates"},
                {"Age", new BsonInt32(48)},
                { "Company",
                    new BsonDocument{
                        {"Name" , "microsoft"},
                        {"Year", new BsonInt32(1974)},
                        {"Price", new BsonInt32(3000000)},
                    }
                }
            };
            p = BsonSerializer.Deserialize<Person>(doc);
            Console.WriteLine(p.ToJson());

            // С помощью метода Deserialize класса BsonSerializer из пространства имен MongoDB.Bson.Serialization мы можем выполнить десериализацию из документа в 
            // объект модели Person. При этом важно, чтобы имена свойств модели совпадали с именами элементов в документе (в том числе и по регистру), иначе 
            // программе не удастся сопоставить элементы и свойства.
            // Также можно выполнить обратную операцию по преобразованию объекта в BsonDocument:
            p = new Person { Name = "Bill", Surname = "Gates", Age = 48 };
            p.Company = new Company { Name = "Microsoft", Year = 1974, Price = 300000 };
            doc = p.ToBsonDocument();
            Console.WriteLine(doc);

            EndMethod();
        }

        private static void SettingsOfModelsWithAttributs()
        {
            // Атрибут BsonIgnore позволяет не учитывать свойство Surname при сериализации объекта в документ. А атрибут BsonElement позволяет задать настройки 
            // элемента для данного свойства. В частности, здесь изменяется название элемента с Name на First Name. Поэтому при создании документа:
            Person3 p = new Person3 { Name = "Bill", Surname = "Gates", Age = 48 };
            p.Company = new Company { Name = "Microsoft", Year = 1974, Price = 300000 };
            Console.WriteLine(p.ToJson());

            // Игнорирование значений по умолчанию
            // В примере выше для объекта Person задается объект Company.Однако в какой - то ситуации для объекта Person данный объект может отсутствовать. 
            // Например, человек не работает ни в какой компании.Однако даже если мы не укажем компанию, такой документ все равно будет содержать данный элемент, 
            // только у него будет значение null.Чтобы избежать добавление в документ элементов, которые имеют значение, можно использовать атрибут BsonIgnoreIfNull:
            Person4 p2 = new Person4 { Name = "Bill", Surname = "Gates", Age = 48 };
            Console.WriteLine(p2.ToJson());

            EndMethod();
        }

        private static void EndMethod()
        {
            Console.WriteLine("\n===================================\n");
        }
    }
}
