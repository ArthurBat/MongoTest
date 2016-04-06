using System;
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoTest2.Models;

namespace MongoTest2
{
    // 08 - Интерфейс IFindFluent и его методы
    public class InterfaceIFindFluent
    {
        public static async Task SortPeople()
        {
            /*
             Метод Find() объекта IMongoCollection возвращает объект IFindFluent, определяющий ряд методов, которые мы можем использовать при получении объектов 
             из БД. Рассмотрим некоторые его основные методы:
                  Sort(): задает параметры сортировки
                  SortBy(): задает сортировку по возрастанию по определенному свойству
                  SortByDescending(): задает сортировку по убыванию
                  Skip(): пропускает при выборке определенное количество объектов
                  Limit(): устанавливает максимальное количество документов, которое можно получить
                  CountAsync(): возвращает число документов в выборке
                  FirstAsync() / FirstOrDefaultAsync(): возвращает первый документ в выборке
                  Projection(): проекция объектов одних типов на другие
              Отсортируем все документы по возрастанию по полю Age:
            */
            string connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("test");
            var collection = database.GetCollection<BsonDocument>("people");
            var people = await collection.Find(new BsonDocument()).Sort("{Age:1}").ToListAsync();
            foreach (var p in people)
                Console.WriteLine(p);
            /*
            Соответственно для сортировки по убыванию нужно написать Sort("{Age:-1}").

            Если мы работаем с классами типа Person, то можно использовать методы SortByDescending и SortBy с лямбда-выражениями:
            */
            var collection2 = database.GetCollection<Person>("people");
//            //сортировка по убыванию по свойству Name
//            var result1 = await collection2.Find(new BsonDocument()).SortByDescending(e =< e.Name);
//            //сортировка по возрастанию по свойству Age
//            var result1 = await collection2.Find(new BsonDocument()).SortBy(e =< e.Age);

            // последовательная сортировка по нескольким ключам
            // по возрастанию Company.Name и убыванию Age
            var sort = Builders<Person>.Sort.Ascending("Company.Name").Descending("Age");
            var people2 = await collection2.Find(new BsonDocument()).Sort(sort).ToListAsync();
            /*
            Пропуск и лимитация выборки:
            */
            //пропустим первые два документа и возьмем следующие три
            // то есть с третьего по пятый
            var filter = new BsonDocument();
            var people3 = await collection.Find(filter).Skip(2).Limit(3).ToListAsync();
            /*
            Количество документов выборки:
            */
            var filter2 = new BsonDocument();
            long number = await collection.Find(filter2).CountAsync();

            EndMethodHelper.EndMethod("SortPeople");
        }

        public static async Task ProjectPeople()
        {
            /*
            Если у нас документы имеют сложную структуру, которая нам не очень подходит для вывода, то мы можем использовать проекции:
            */
            var client = new MongoClient("mongodb://localhost");
            var database = client.GetDatabase("test");
            var collection = database.GetCollection<BsonDocument>("people");

            var people = await collection.Find(new BsonDocument()).Project("{Name:1, Age:1, _id:0}").ToListAsync();
            foreach (var p in people)
                Console.WriteLine(p);
            /*
            Выражение Project("{Name:1, Age:1, _id:0}") указывает, что из всех полей в финальную выборку будут попадать только поля Name и Age, так как для ни 
            установлена единица. Все остальные поля не добавляются. Исключение только _id, поэтому если мы не хотим видеть данное поле в выборке, то для него 
            надо установить ноль.

            Альтернативный способ - применение класса ProjectionDefinitionBuilder:
            */
            var people2 =
                await
                    collection.Find(new BsonDocument())
                        .Project(Builders<BsonDocument>.Projection.Include("Name").Include("Age").Exclude("_id"))
                        .ToListAsync();
            /*
            С помощью методов Include() указываем, какие поля надо включить. Соответственно метод Exclude() исключает поле из выборки.

            Теперь осуществим выборку объектов Person и проецируем их на какой-нибудь другой тип. Для проекции определим дополнительный класс Employee
            Теперь при выборке осуществим проекцию из Person в Employee:
            */
//            var filter = new BsonDocument();
//            var projection = Builders<Person>.Projection.Expression(p => new Employee { Name = p.Name, Age = p.Age });
//            var employees = await collection.Find(filter).Project<Employee>(projection).ToListAsync();
//            foreach (var e in employees)
//            {
//                Console.WriteLine("{0} - {1}", e.Name, e.Age);
//            }

            EndMethodHelper.EndMethod("ProjectPeople");
        }
    }
}
