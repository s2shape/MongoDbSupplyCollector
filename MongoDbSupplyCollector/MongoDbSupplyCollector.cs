using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
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
            _connectionStringParts.Add(new ConnectionStringPart("host", true, CnnnectionStringDataType.text));
            _connectionStringParts.Add(new ConnectionStringPart("port", false, CnnnectionStringDataType.number));
            _connectionStringParts.Add(new ConnectionStringPart("username", false, CnnnectionStringDataType.text));
            _connectionStringParts.Add(new ConnectionStringPart("password", true, CnnnectionStringDataType.password));
            _connectionStringParts.Add(new ConnectionStringPart("port", false, CnnnectionStringDataType.number));
            _connectionStringParts.Add(new ConnectionStringPart("options", false, CnnnectionStringDataType.text));
        }

        public string BuildConnectionString(Dictionary<string, string> connectionStringValues)
        {
            var connectionString = "mongodb://";

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

        private void AddSamples(List<string> samples, BsonValue value, string propertyName)
        {
            string[] nameParts = propertyName.Split(".");

            string subProperty = propertyName;

            string currentProperty = nameParts[0];

            if (nameParts.Length > 1)
            {
                subProperty = string.Join(".", nameParts, 1, nameParts.Length - 1);
            }

            if (value.IsBsonDocument)
            {
                if (value.ToBsonDocument().Contains(currentProperty))
                {
                    value = value[currentProperty];
                    AddSamples(samples, value, subProperty);
                }               
            }
            else
            {
                if (value.IsBsonArray)
                {
                    for (int i = 0; i < value.AsBsonArray.Count; i++)
                    {
                        AddSamples(samples, value[i], subProperty);
                    }
                }
                else
                {
                    if(!(value is BsonNull))
                        samples.Add(value.ToString());
                }
            }
        }

        public override List<string> CollectSample(DataEntity dataEntity, int sampleSize)
        {
            DataContainer container = dataEntity.Container;
            string databaseName = GetDatabaseNameFromConnectionString(container.ConnectionString);

            var client = new MongoClient(container.ConnectionString);
            var database = client.GetDatabase(databaseName);
            string collectionName = dataEntity.Collection.Name;

            var mongoCollection = database.GetCollection<BsonDocument>(collectionName);
            var docs = mongoCollection.AsQueryable(new AggregateOptions() {AllowDiskUse = true}).Sample(sampleSize);

            /*var field = "{'_id': 0, '" + dataEntity.Name + "': 1}";

            var documents = mongoCollection.Find<BsonDocument>(_emptyFilter).Project<BsonDocument>(field).Limit(sampleSize).ToList();
            */

            var samples = new List<string>();

            foreach(BsonDocument document in docs)
            {
                BsonValue value = document;

                AddSamples(samples, value, dataEntity.Name);
            }

            return samples.Take(sampleSize).ToList();
            
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
                var command = new BsonDocument { { "collStats", collectionName }, { "scale", 1 } };
                var collectionStats = database.RunCommand<BsonDocument>(command);

                var metrics = new DataCollectionMetrics();
                metrics.Name = collectionName;
                metrics.RowCount = collectionStats["count"].AsInt32;
                metrics.TotalSpaceKB = (collectionStats["storageSize"].AsInt32 + collectionStats["totalIndexSize"].AsInt32) / 1024.0M;
                metrics.UsedSpaceKB = metrics.TotalSpaceKB;

                dataCollectionMetrics.Add(metrics);
            }

            return dataCollectionMetrics;
        }

        public static string GetDatabaseNameFromConnectionString(string connectionString)
        {
            string[] connectionStringElements = connectionString.Split("/");
            string databaseName = connectionStringElements[3].Split("?")[0];

            return databaseName;
        }

        private DataEntity GetDataEntity(BsonValue bsonValue, string propertyName, string parentProperty, DataCollection dataCollection, DataContainer container)
        {
            DataType dataType = GetDataType(bsonValue);

            string name = propertyName;

            if (!string.IsNullOrWhiteSpace(parentProperty))
            {
                name = parentProperty + "." + propertyName;
            }
            DataEntity dataEntity = new DataEntity(name, dataType, bsonValue.BsonType.ToString(), container, dataCollection);

            return dataEntity;
        }

        private List<DataEntity> GetDataEntitiesFromArray(BsonArray bsonArray, string propertyName, string parentProperty, DataCollection dataCollection, DataContainer container)
        {
            var entities = new List<DataEntity>();

            if (bsonArray.Count > 0)
            {
                var bsonValue = bsonArray[0];

                AddEntityAndChildren(bsonValue, propertyName, parentProperty, dataCollection, container, entities);

            }

            return entities;
        }

        private List<DataEntity> GetDataEntitiesFromDocument(BsonDocument bsonDocument, string parentProperty, DataCollection dataCollection, DataContainer container)
        {
            var entities = new List<DataEntity>();

            foreach (var propertyName in bsonDocument.Names)
            {
                if (propertyName != "_id")
                {
                    BsonValue bsonValue = bsonDocument[propertyName];
                    AddEntityAndChildren(bsonValue, propertyName, parentProperty, dataCollection, container, entities);

                }
            }

            return entities;
        }

        private void AddEntityAndChildren(BsonValue bsonValue, string propertyName, string parentProperty, DataCollection dataCollection, DataContainer container, List<DataEntity> entities)
        { 
            switch (bsonValue.BsonType)
            {
                case BsonType.Array:
                    entities.AddRange(GetDataEntitiesFromArray(bsonValue.AsBsonArray, propertyName, parentProperty, dataCollection, container));
                    break;
                case BsonType.Document:
                    if (!string.IsNullOrWhiteSpace(parentProperty))
                    {
                        propertyName = parentProperty + "." + propertyName;
                    }
                    entities.AddRange(GetDataEntitiesFromDocument(bsonValue.AsBsonDocument, propertyName, dataCollection, container));
                    break;
                default:
                    DataEntity dataEntity = GetDataEntity(bsonValue, propertyName, parentProperty, dataCollection, container);
                    entities.Add(dataEntity);
                    break;

            }
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
                var collectionEntities = new List<DataEntity>();

                DataCollection dataCollection = new DataCollection(container, collectionName);
                dataCollection.Container = container;
                dataCollection.Name = collectionName;

                collections.Add(dataCollection);

                var mongoCollection = database.GetCollection<BsonDocument>(collectionName);

                var documents = mongoCollection.Find(_emptyFilter).Limit(10).ToList();

                int schemaSampleSize = 10;
                int sample = 0;

                while (sample < documents.Count && sample < schemaSampleSize)
                {
                    BsonDocument document = documents[sample];

                    collectionEntities.AddRange(GetDataEntitiesFromDocument(document, null, dataCollection, container));

                    sample++;
                }

                entities.AddRange(collectionEntities.DistinctBy(e => e.Name).ToList());
            }

            return (collections, entities);
        }

        private static DataType GetDataType(BsonValue bsonValue)
        {
            if (bsonValue.IsBoolean)
            {
                return DataType.Boolean;
            }

            if (bsonValue.IsGuid)
            {
                return DataType.Guid;
            }

            if (bsonValue.IsInt32)
            {
                return DataType.Int;
            }

            if (bsonValue.IsInt64)
            {
                return DataType.Long;
            }

            if (bsonValue.IsString)
            {
                return DataType.String;
            }

            if (bsonValue.IsValidDateTime)
            {
                return DataType.DateTime;
            }

            if (bsonValue.IsDouble)
            {
                return DataType.Double;
            }

            return DataType.Unknown;
        }

        public bool IsValidConnectionString(Dictionary<string, string> connectionStringValues)
        {
            bool isValid = connectionStringValues.ContainsKey("host");

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

    public static class LinqExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }

}