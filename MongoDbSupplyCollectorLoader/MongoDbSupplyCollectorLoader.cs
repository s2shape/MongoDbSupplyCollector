using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using MongoDB.Bson;
using MongoDB.Driver;
using S2.BlackSwan.SupplyCollector.Models;
using SupplyCollectorDataLoader;

namespace MongoDbSupplyCollectorLoader
{
    public class MongoDbSupplyCollectorLoader : SupplyCollectorDataLoaderBase
    {
        public override void InitializeDatabase(DataContainer dataContainer) {
            // Nothing to do
        }

        public override void LoadSamples(DataEntity[] dataEntities, long count) {
            var client = new MongoClient(dataEntities[0].Container.ConnectionString);
            string databaseName = MongoDbSupplyCollector.MongoDbSupplyCollector.GetDatabaseNameFromConnectionString(dataEntities[0].Container.ConnectionString);
            var database = client.GetDatabase(databaseName);

            database.CreateCollection(dataEntities[0].Collection.Name);
            var mongoCollection = database.GetCollection<BsonDocument>(dataEntities[0].Collection.Name);
            long rows = 0;

            var r = new Random();
            while (rows < count) {
                if (rows % 1000 == 0) {
                    Console.Write(".");
                }

                var doc = new BsonDocument();

                foreach (var dataEntity in dataEntities) {
                    switch (dataEntity.DataType) {
                        case DataType.String:
                            doc.Set(dataEntity.Name, new BsonString(new Guid().ToString()));
                            break;
                        case DataType.Int:
                            doc.Set(dataEntity.Name, new BsonInt32(r.Next()));
                            break;
                        case DataType.Double:
                            doc.Set(dataEntity.Name, new BsonDouble(r.NextDouble()));
                            break;
                        case DataType.Boolean:
                            doc.Set(dataEntity.Name, new BsonBoolean(r.Next(100) > 50));
                            break;
                        case DataType.DateTime:
                            doc.Set(dataEntity.Name, new BsonDateTime(DateTimeOffset.Now.ToUnixTimeMilliseconds() + r.Next()));
                            break;
                        default:
                            doc.Set(dataEntity.Name, new BsonInt32(0));
                            break;
                    }
                }

                mongoCollection.InsertOne(doc);
                rows++;
            }

            Console.WriteLine();
        }

        public override void LoadUnitTestData(DataContainer dataContainer) {
            var client = new MongoClient(dataContainer.ConnectionString);
            string databaseName = MongoDbSupplyCollector.MongoDbSupplyCollector.GetDatabaseNameFromConnectionString(dataContainer.ConnectionString);
            var database = client.GetDatabase(databaseName);

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
