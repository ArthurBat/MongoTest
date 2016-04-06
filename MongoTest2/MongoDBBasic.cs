using System;
using MongoDB.Driver;

namespace MongoTest2
{
    /// <summary>
    /// 01 - Получение всех бд с сервера
    /// </summary>
    public class MongoDBBasic
    {
        public static async void GetDatabaseNames(MongoClient client)
        {
            using (var cursor = await client.ListDatabasesAsync())
            {
                var databaseDocuments = await cursor.ToListAsync();
                foreach (var databaseDocument in databaseDocuments)
                {
                    Console.WriteLine(databaseDocument["name"]);
                }
            }
            EndMethodHelper.EndMethod("GetDatabaseNames");
        }
    }
}
