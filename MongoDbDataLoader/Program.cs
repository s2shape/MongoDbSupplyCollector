using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using MongoDB.Driver;

namespace MongoDbDataLoader
{
    class Program
    {
        static void Main(string[] args) {
            var host = Environment.GetEnvironmentVariable("MONGO_HOST");
            var client = new MongoClient($"mongodb://{host}");
            var database = client.GetDatabase("s2");

            List<ContactsAudit> contactsAudits = LoadFile<ContactsAudit>("./SampleData/CONTACTS_AUDIT.CSV");
            LoadData<ContactsAudit>(database, "contacts_audit", contactsAudits);

            List<Email> emails = LoadFile<Email>("./SampleData/EMAILS.CSV");
            LoadData<Email>(database, "email", emails);


            List<Lead> leads = LoadFile<Lead>("./SampleData/LEADS.CSV");
            LoadData<Lead>(database, "lead", leads);

            LoadData<Person>(database, "person", GeneratePeople());
        }

        private static void LoadData<T>(IMongoDatabase database, string collectionName, List<T> data)
        {
            database.DropCollection(collectionName);
            var collection = database.GetCollection<T>(collectionName);

            collection.InsertMany(data);

            Console.WriteLine("Loaded " + data.Count + " entries into " + collectionName);
        }

        private static List<T> LoadFile<T>(string path)
        {
            Console.WriteLine(path);

            List<T> records;

            using (StreamReader streamReader = new StreamReader(path))
            {
                var reader = new CsvReader(streamReader);

                reader.Read();
                reader.ReadHeader();
                var header = reader.Context.HeaderRecord;

                records = reader.GetRecords<T>().ToList();
            }

            return records;
        }

        private static List<Person> GeneratePeople()
        {
            var list = new List<Person>(200);

            var noType0 = new Person(1000000, 10, 10);
            noType0.Addresses["type0"] = null;

            list.Add(noType0);

            for (int i = 0; i < 200; i++)
            {
                var person = new Person(i, 2, 3);
                list.Add(person);
            }

            return list;
        }
    }
}
