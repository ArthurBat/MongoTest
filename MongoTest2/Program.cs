﻿using System;
using System.Configuration;
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
        }
    }
}
