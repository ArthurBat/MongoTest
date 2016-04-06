using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
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
            MongoDBBasic.GetDatabaseNames(client);
            Console.ReadLine();
            GetCollectionsNames(client).Wait();
            Console.ReadLine();
            WorkingWithDataModels(client);
            Console.ReadLine();
            SettingsOfModelsWithAttributs();
            Console.ReadLine();
            //            SaveDocs().GetAwaiter().GetResult();
            FindDocs().GetAwaiter().GetResult();
            Console.ReadLine();
            FindPeople().GetAwaiter().GetResult();
            Console.ReadLine();
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

            EndMethodHelper.EndMethod("GetCollectionsNames");
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

            EndMethodHelper.EndMethod(MethodBase.GetCurrentMethod().Name);
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

            EndMethodHelper.EndMethod(MethodBase.GetCurrentMethod().Name);
        }

        private static async Task SaveDocs()
        {
            string connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("test");
            var collection = database.GetCollection<BsonDocument>("people");

            // Для добавления данных в коллекцию используется метод InsertOneAsync, определенный в интерфейсе IMongoCollection. Например, добавим в коллекцию 
            // people один документ:
            BsonDocument person1 = new BsonDocument
            {
                {"Name", "Bill"},
                {"Age", 32},
                {"Languages", new BsonArray{"english", "german"}}
            };
//            await collection.InsertOneAsync(person1);

            // Кроме метода InsertOneAsync мы также можем использовать для сохранения документов метод InsertManyAsync(), который в качестве параметра принимает 
            // набор объектов:
            BsonDocument person2 = new BsonDocument
            {
                {"Name", "Steve"},
                {"Age", 31},
                {"Languages", new BsonArray {"english", "french"}}
            };
            await collection.InsertManyAsync(new[] {person1, person2});

            // Однако мы можем работать не только с объектами BsonDocument, но и со стандартными классами C#. Допустим, нам надо сохранить объекты следующих классов:
            var collection2 = database.GetCollection<Person>("people");
            Person person3 = new Person
            {
                Name = "Jack",
                Age = 29,
                Languages = new List<string> { "english", "german" },
                Company = new Company
                {
                    Name = "Google",
                    Price = 3000000,
                    Year = 1998
                }
            };
            await collection2.InsertOneAsync(person3);
            // При добавлении, если для объекта не установлен идентификатор "_id", то он автоматически генерируется. И затем мы его можем получить:
            Console.WriteLine(person3.Id);

            EndMethodHelper.EndMethod(MethodBase.GetCurrentMethod().Name);
        }

        private static async Task FindDocs()
        {
            string connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("test");
            var collection = database.GetCollection<BsonDocument>("people");

            // Итак, в прошлой теме в коллекцию people в базу данных test было добавлено несколько элементов. Теперь используем метод FindAsync() для их извлечения:
            // Если мы хотим выбрать все документы, как в данном случае, то в качестве фильтра определяем пустой BsonDocument:
            var filter = new BsonDocument();
            // Для создания запросов драйвер C# для MongoDB применяет асинхронный API, и нам надо это учитывать. И так как метод FindAsync() возвращает не просто 
            // IAsyncCursor, а Task<IAsyncCursor>, то затем нам еще надо его получить:
            using (var cursor = await collection.FindAsync(filter))
            {
                while (await cursor.MoveNextAsync())
                {
                    var people = cursor.Current;
                    foreach (var doc in people)
                    {
                        Console.WriteLine(doc);
                    }
                }
            }

            // Для перехода к порции данных используется асинхронный метод MoveNextAsync(), возвращающий объект Task<bool>. Логическое значение в данном случае будет 
            // показывать, есть ли еще данные.
            // И для получения извлеченной коллекции элементов применяется выражение cursor.Current, которое возвращает объект IEnumerable< Computer >.
            // Подобным образом можно получать вместо объектов BsonDocument объекты стандартных классов:
            var collection2 = database.GetCollection<Person>("people");
            using (var cursor = await collection2.FindAsync(filter))
            {
                while (await cursor.MoveNextAsync())
                {
                    var people = cursor.Current;
                    foreach (Person doc in people)
                    {
                        Console.WriteLine("{0} - {1} ({2})", doc.Id, doc.Name, doc.Age);
                    }
                }
            }

            // И аналогичный пример с использованием метода Find:
            var people3 = await collection2.Find(filter).ToListAsync();

            foreach (Person doc in people3)
            {
                Console.WriteLine("{0} - {1} ({2})", doc.Id, doc.Name, doc.Age);
            }

            EndMethodHelper.EndMethod("FindDocs");
        }

        private static async Task FindPeople()
        {
            // Во второй главе рассматривались различные операции, которые можно использовать для фильтрации выборки, например, операторы $gt, $lt и т.д. 
            // И здесь мы также можем использовать все эти операторы. Например, выберем все объекты, у которых значение Age больше 30:
            string connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("test");
            var collection = database.GetCollection<BsonDocument>("people");
            // оператор $gt
            var filter = new BsonDocument("Age", new BsonDocument("$gt", 31));
            var people = await collection.Find(filter).ToListAsync();
            foreach (var p in people)
            {
                Console.WriteLine(p);
            }

            // Или, например, зададим фильтр для выбора документов, у которых поле "Age" имеет значение от 33 и выше, либо поле "Name" имеет значение "Tom":
            var filter2 = new BsonDocument("$or", new BsonArray
            {
                new BsonDocument("Age", new BsonDocument("$gte", 33)),
                new BsonDocument("Name", "Tom")
            });
            var people2 = await collection.Find(filter2).ToListAsync();

            // Другим способом для создания фильтрации представляет применение класса FilterDefinitionBuilder, в котором определен ряд методов:

            // Операции сравнения
            //      Eq: выбирает только те документы, у которых значение определенного свойсва равно некоторому значению.Например, 
            //          Builders<Person>.Filter.Eq(Name", "Tom")
            //      Gt: выбирает только те документы, у которых значение определенного свойства больше некоторого значения.Например, 
            //          Builders<Person>.Filter.Gt("Age", 25)
            //      Gte: выбирает только те документы, у которых значение определенного свойства больше или равно некоторому значению.Например, 
            //          Builders<Person>.Filter.Gte("Name", "T") - выбирает все документы, у которых значение Name начинается с буквы T
            //      Lt: выбирает только те документы, у которых значение определенного свойства меньше некоторого значения Builders<Person>.Filter.Lt("Age", 25)
            //      Lte: выбирает только те документы, у которых значение определенного свойства меньше или равно некоторому значению
            //      Ne: выбирает только те документы, у которых значение определенного свойства не равно некоторому значению Builders<Person>.Filter.Ne("Age", 23)
            //      In: получает все документы, у которых значение свойства может принимать одно из указанных значений.Например, найдем все документы, 
            //          у которых свойство Age равно либо 1977, либо 1989, либо 1981: Builders<Person>.Filter.In("Age", new List<BsonInt32>() { 23, 25, 27 });
            //      Nin: противоположность оператору In - выбирает все документы, у которых значение свойства не принимает одно из указанных значений
            // Объект Person имеет ссылку на объект Company. И, допустим, нам надо сделать выборку объектов Person по определенному свойству Company:
            var filter3 = Builders<BsonDocument>.Filter.Eq("Company.Name", "Microsoft");
            var people3 = await collection.Find(filter3).ToListAsync();
            foreach (var p in people3)
            {
                Console.WriteLine(p);
            }

            // Соответственно если мы получаем коллекцию объектов Person, то и FilterDefinitionBuilder также типизируется классом Person:
            var collection2 = database.GetCollection<Person>("people");
            var filter4 = Builders<Person>.Filter.Eq("Company.Name", "Microsoft");
            var people4 = await collection2.Find(filter4).ToListAsync();
            foreach (var p in people4)
            {
                Console.WriteLine(p.Name);
            }

            // С помощью стандартных операций программирования конъюнкции, дизъюнкции и логического отрицания мы можем комбинировать запросы. Например, 
            // фильтр на всех документов у которых Name=Bill, либо Name=Tom:
            var builder = Builders<BsonDocument>.Filter;
            var filter5 = builder.Eq("Name", "Bill") | builder.Eq("Name", "Tom");
            var people5 = await collection.Find(filter).ToListAsync();

            // Запрос на получение объектов, у которых Name=Bill, но при этом Age не равно 30:
            var builder2 = Builders<BsonDocument>.Filter;
            var filter6 = builder.Eq("Name", "Bill") & !builder.Eq("Name", "Tom");
            var people6 = await collection.Find(filter).ToListAsync();

            // В данном случае стоит отметить, что если мы используем не BsonDocument, а стандартные классы, например, Person, то задавать запросы можем 
            // более удобным способом через лямбда выражения. Так, перепишем предыдущий пример:
            var people7 = await collection2.Find(x => x.Name == "Bill" && x.Age != 30).ToListAsync();
            foreach (var p in people7)
            {
                Console.WriteLine("{0} - {1}", p.Name, p.Age);
            }

            // Логические операции
            //      Not: возвращает документы, которые не попадают под определенное условие Builders< Person >.Filter.Not(Builders<Person>.Filter.Eq("Name", "Tom"))
            //      Or: определяет набор отдельных фильтров, которые используются для выборки.
            //      And: также, как и Or, определяет набор отдельных фильтров, которые используются для выборки, но теперь возвращаются только те документы, которые 
            //          соответствуют все этим объектам FilterDefinition
            //      Используем методы Or и And:
            // метод Or
            var filter7 = Builders<Person>.Filter.Eq("Name", "Tom");
            var filter8 = Builders<Person>.Filter.Eq("Age", 28);
            var filterOr = Builders<Person>.Filter.Or(new List<FilterDefinition<Person>> { filter7, filter8 });

            // метод And
            var filter9 = Builders<Person>.Filter.Eq("Name", "John");
            var filter10 = Builders<Person>.Filter.Eq("Age", 27);
            var filterAnd = Builders<Person>.Filter.And(new List<FilterDefinition<Person>> { filter9, filter10 });

            // Операции с элементами
            //      Exists: выбирает из бд те документы, в которых присутствует определенный ключ, например, Builders<Person>.Filter.Exists("Name");
            //      NotExists: выбирает из бд те документы, в которых отсутствует определенный ключ, например, Builders<Person>.Filter.NotExists("Age");

            // Вычисление значений
            //      Regex: выбирает все документы, у которых значение ключа соответствует регулярному значению.Например, выберем все документы, в именах которых 
            //          встречается буква "o": Builders<Person>.Filter.Regex("Name", new BsonRegularExpression("o")).Объект BsonRegularExpression позволяет задать 
            //          регулярное значение.
            //      Where: возвращает документы, которые соответствуют определенному условию.Условие определяется с помощью лямбда - выражения: 
            //          Builders<Person>.Filter.Where(e => e.Name == "Tom");

            // Операции с массивами
            //      All: выбирает все документы, в которые содержат все элементы массива
            //      Size: выбирает все документы, которые содержат определенное число элементов

            // Теперь найдем людей, у которых имеется два определенных языка, а также найдем людей, у которых имеется в этом массиве только один язык:
            // получим все объекты, у которых определены английский и французский языки
            var filter11 = Builders<Person>.Filter.All("Languages", new List<string>() { "english", "french" });

            // получим все объекты, у которых в массиве languages только 2 элемента
            var filter12 = Builders<Person>.Filter.Size("languages", 2);

            EndMethodHelper.EndMethod("FindPeople");
        }
    }
}
