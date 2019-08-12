using System;
using Xunit;
using S2.BlackSwan.SupplyCollector.Models;
using System.Collections.Generic;

namespace MongoSupplyCollectorTests
{
    public class MongoSupplyCollectorTests
    {
        public readonly MongoDbSupplyCollector.MongoDbSupplyCollector _instance;
        public readonly DataContainer _container;
        public readonly DataEntity _emailToAddress;

        public MongoSupplyCollectorTests()
        {
            _instance = new MongoDbSupplyCollector.MongoDbSupplyCollector();
            var connectionStringValues = new Dictionary<string, string>();
            connectionStringValues["host"] = "localhost";
            connectionStringValues["port"] = "27017";
            connectionStringValues["database"] = "s2";
            connectionStringValues["options"] = "connectTimeoutMS=3000";

            _container = new DataContainer()
            {
                ConnectionString = _instance.BuildConnectionString(connectionStringValues)
            };

            _emailToAddress = new DataEntity()
            {
                Container = _container,
                Collection = new DataCollection()
                {
                    Name = "email"
                },
                Name = "TO_ADDRS_EMAILS"
            };
        }

        [Fact]
        public void DataStoreTypesTest()
        {
            var result = _instance.DataStoreTypes();
            Assert.Contains("MongoDB", result);
        }

        [Fact]
        public void TestConnectionTest()
        {
            var result = _instance.TestConnection(_container);
            Assert.True(result);
        }

        [Fact]
        public void GetSchemaTest()
        {
            var (tables, elements) = _instance.GetSchema(_container);
            Assert.Equal(3, tables.Count);
            Assert.Equal(156, elements.Count);
            foreach(DataEntity element in elements)
            {
                Assert.NotEmpty(element.DataType);
                Assert.NotEmpty(element.DbDataType);
            }
        }

        [Fact]
        public void CollectSampleTest()
        {
            var samples = _instance.CollectSample(_emailToAddress, 161);
            Assert.Equal(161, samples.Count);
            Assert.Contains("qa25@example.com", samples);
        }

        [Fact]
        public void GetDataCollectionMetricsTest()
        {
            var result = _instance.GetDataCollectionMetrics(_container);
            var contactsAuditMetrics = result.Find(x => x.Name == "contacts_audit");
            var leadsMetrics = result.Find(x => x.Name == "lead");
            var emailMetrics = result.Find(x => x.Name == "email");

            Assert.Equal(3, result.Count);

            Assert.Equal(200, contactsAuditMetrics.RowCounts);
            Assert.Equal(116, contactsAuditMetrics.TotalSpaceKB);
            Assert.Equal(.1M, contactsAuditMetrics.TotalSpaceMB, 1);

            Assert.Equal(200, leadsMetrics.RowCounts);
            Assert.Equal(112, leadsMetrics.TotalSpaceKB);
            Assert.Equal(.1M, leadsMetrics.TotalSpaceMB, 1);

            Assert.Equal(200, emailMetrics.RowCounts);
            Assert.Equal(84, emailMetrics.TotalSpaceKB);
            Assert.Equal(.1M, emailMetrics.TotalSpaceMB, 1);

        }

        [Fact]
        public void IsValidConnectionStringTest()
        {
            var connectionStringValues = new Dictionary<string, string>();
            var result = _instance.IsValidConnectionString(connectionStringValues);
            Assert.False(result);

            connectionStringValues["host"] = "localhost";
            result = _instance.IsValidConnectionString(connectionStringValues);
            Assert.True(result);
        }

        [Fact]
        public void GetConnectionStringPartsTest()
        {
            var connectionStringParts = _instance.GetConnectionStringParts();

            Assert.True(connectionStringParts.Count > 0);
        }

    }
}