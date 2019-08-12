using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using S2.BlackSwan.SupplyCollector;
using S2.BlackSwan.SupplyCollector.Models;

namespace MongoDbSupplyCollector
{
    public class MongoDbSupplyCollector : SupplyCollectorBase, IDescribeConnectionStrings, IValidateConnectionStrings, IBuildConnectionStrings
    {
        private readonly List<ConnectionStringPart> _connectionStringParts = new List<ConnectionStringPart>();

        private readonly FilterDefinition<BsonDocument> _emptyFilter = Builders<BsonDocument>.Filter.Empty;

        public MongoDbSupplyCollector()
        {
            _connectionStringParts.Add(new ConnectionStringPart("host", true, DataType.text));
            _connectionStringParts.Add(new ConnectionStringPart("port", false, DataType.number));
            _connectionStringParts.Add(new ConnectionStringPart("username", false, DataType.text));
            _connectionStringParts.Add(new ConnectionStringPart("password", true, DataType.password));
            _connectionStringParts.Add(new ConnectionStringPart("port", false, DataType.number));
            _connectionStringParts.Add(new ConnectionStringPart("options", false, DataType.text));
        }

        public string BuildConnectionString(Dictionary<string, string> connectionStringValues)
        {
            string connectionString = "mongodb://";

            if (connectionStringValues.ContainsKey("username") && connectionStringValues.ContainsKey("password"))
            {
                string username = connectionStringValues["username"];
                string password = connectionStringValues["password"];
                connectionString = connectionString + username + ":" + password + "@";
            }

            connectionString = connectionString + connectionStringValues["host"];

            if (connectionStringValues.ContainsKey("port"))
            {
                connectionString = connectionString + ":" + connectionStringValues["port"];
            }

            if (connectionStringValues.ContainsKey("database"))
            {
                connectionString = connectionString + "/" + connectionStringValues["database"];
            }

            if (connectionStringValues.ContainsKey("options"))
            {
                connectionString = connectionString + "?" + connectionStringValues["options"];
            }

            return connectionString;
        }

        public override List<string> CollectSample(DataEntity dataEntity, int sampleSize)
        {
            DataContainer container = dataEntity.Container;
            string databaseName = GetDatabaseNameFromConnectionString(container.ConnectionString);

            var client = new MongoClient(container.ConnectionString);
            var database = client.GetDatabase(databaseName);
            string collectionName = dataEntity.Collection.Name;

            var mongoCollection = database.GetCollection<BsonDocument>(collectionName);

            var documents = mongoCollection.Find<BsonDocument>(_emptyFilter).Limit(sampleSize).ToList();

            var samples = new List<string>();

            foreach(BsonDocument document in documents)
            {
                string sample = document[dataEntity.Name].AsString;
                samples.Add(sample);
            }

            return samples;
            
        }

        public override List<string> DataStoreTypes()
        {
            return new List<string>() { "MongoDB" };
        }

        public List<ConnectionStringPart> GetConnectionStringParts()
        {
            return _connectionStringParts;
        }

        public override List<DataCollectionMetrics> GetDataCollectionMetrics(DataContainer container)
        {
            string databaseName = GetDatabaseNameFromConnectionString(container.ConnectionString);

            var client = new MongoClient(container.ConnectionString);
            var database = client.GetDatabase(databaseName);
            List<string> collectionNames = database.ListCollectionNames().ToList();

            var dataCollectionMetrics = new List<DataCollectionMetrics>();

            foreach (string collectionName in collectionNames)
            {
                var mongoCollection = database.GetCollection<BsonDocument>(collectionName);

                var command = new BsonDocument { { "collStats", collectionName }, { "scale", 1 } };
                var collectionStats = database.RunCommand<BsonDocument>(command);

                var metrics = new DataCollectionMetrics();
                metrics.Name = collectionName;
                metrics.RowCounts = collectionStats["count"].AsInt32;
                metrics.TotalSpaceKB = (collectionStats["storageSize"].AsInt32 + collectionStats["totalIndexSize"].AsInt32) / 1024;
                metrics.TotalSpaceMB = (decimal)metrics.TotalSpaceKB / 1024;
                metrics.UsedSpaceKB = metrics.TotalSpaceKB;
                metrics.UsedSpaceMB = metrics.TotalSpaceMB;

                dataCollectionMetrics.Add(metrics);
            }

            return dataCollectionMetrics;
        }

        private static string GetDatabaseNameFromConnectionString(string connectionString)
        {
            string[] connectionStringElements = connectionString.Split("/");
            string databaseName = connectionStringElements[3].Split("?")[0];

            return databaseName;
        }

        public override (List<DataCollection>, List<DataEntity>) GetSchema(DataContainer container)
        {
            var collections = new List<DataCollection>();
            var entities = new List<DataEntity>();

            string databaseName = GetDatabaseNameFromConnectionString(container.ConnectionString);

            var client = new MongoClient(container.ConnectionString);
            var database = client.GetDatabase(databaseName);
            List<string> collectionNames = database.ListCollectionNames().ToList();

            foreach(string collectionName in collectionNames)
            {
                DataCollection dataCollection = new DataCollection();
                dataCollection.Container = container;
                dataCollection.Name = collectionName;

                collections.Add(dataCollection);

                var mongoCollection = database.GetCollection<BsonDocument>(collectionName);

                var documents = mongoCollection.Find<BsonDocument>(_emptyFilter).Limit(1).ToList();

                if (documents.Count > 0)
                {
                    BsonDocument document = documents[0];

                    foreach(var propertyName in document.Names)
                    {
                        if (propertyName != "_id")
                        {
                            DataEntity dataEntity = new DataEntity();
                            dataEntity.Collection = dataCollection;
                            dataEntity.Container = container;
                            dataEntity.Name = propertyName;
                            SetDataTypes(dataEntity, document[propertyName]);

                            entities.Add(dataEntity);
                        }
                    }
                }
                
            }

            return (collections, entities);
        }

        private static void SetDataTypes(DataEntity dataEntity, BsonValue bsonValue)
        {
            dataEntity.DbDataType = bsonValue.BsonType.ToString();

            if (bsonValue.IsBoolean)
            {
                dataEntity.DataType = "bool";
                return;
            }

            if (bsonValue.IsGuid)
            {
                dataEntity.DataType = "uuid";
                return;
            }

            if (bsonValue.IsInt32)
            {
                dataEntity.DataType = "int";
                return;
            }

            if (bsonValue.IsInt64)
            {
                dataEntity.DataType = "long";
                return;
            }

            if (bsonValue.IsString)
            {
                dataEntity.DataType = "string";
                return;
            }

            if (bsonValue.IsValidDateTime)
            {
                dataEntity.DataType = "datetime";
                return;
            }

            dataEntity.DataType = bsonValue.BsonType.ToString();
        }

        public bool IsValidConnectionString(Dictionary<string, string> connectionStringValues)
        {
            bool isValid = false;

            if (connectionStringValues.ContainsKey("host"))
            {
                isValid = true;
            }

            if (connectionStringValues.ContainsKey("username") && !connectionStringValues.ContainsKey("password"))
            {
                isValid = false;
            }

            if (!connectionStringValues.ContainsKey("username") && connectionStringValues.ContainsKey("password"))
            {
                isValid = false;
            }

            return isValid;
        }

        public override bool TestConnection(DataContainer container)
        {
            var client = new MongoClient(container.ConnectionString);
            var dbList = client.ListDatabaseNames();

            return dbList.Any();
        }
    }
}