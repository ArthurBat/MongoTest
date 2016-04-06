using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoTest2
{
    // 07 - Фильтрация данных
    public class FilteringData
    {
        public static async Task FindPeople()
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
