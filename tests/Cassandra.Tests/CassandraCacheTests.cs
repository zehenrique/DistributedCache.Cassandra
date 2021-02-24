namespace DistributedCache.Cassandra.Tests
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using global::Cassandra;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Options;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    // Run setup-local-cassandra.sh before running these tests
    [TestClass]
    [TestCategory("IntegrationTests")]
    public class CassandraCacheTests
    {
        private readonly CassandraCache cassandraCache;

        public CassandraCacheTests()
        {
            var cassandraCacheOptions = new CassandraCacheOptions
            {
                Session =
                    Cluster.Builder()
                    .AddContactPoint("localhost")
                    .WithCredentials("cassandra", "cassandra")
                    .WithPort(9042)
                    .WithDefaultKeyspace("cassandracache")
                    .Build()
                    .Connect(),
                ReadConsistencyLevel = ConsistencyLevel.LocalOne,
                WriteConsistencyLevel = ConsistencyLevel.LocalOne
            };

            var options = Options.Create(cassandraCacheOptions);

            this.cassandraCache = new CassandraCache(options);
        }

        [TestMethod]
        public void CassandraCache_Get_NonExistingKey_ReturnNull()
        {
            var key = "test";

            var result = this.cassandraCache.Get(key);

            Assert.IsNull(result, "Result is not null");
        }

        [TestMethod]
        public async Task CassandraCache_GetAsync_NonExistingKey_ReturnNull()
        {
            var key = "test";

            var result = await this.cassandraCache.GetAsync(key);

            Assert.IsNull(result, "Result is not null");
        }

        [TestMethod]
        public void CassandraCache_SetAndGet_InsertRowAndReturnValue()
        {
            var key = "test" + Guid.NewGuid();
            var value = Encoding.UTF8.GetBytes("this is a test");
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(1),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            };

            this.cassandraCache.Set(key, value, options);

            var result = this.cassandraCache.Get(key);

            Assert.IsNotNull(result, "Result is null");
            Assert.IsTrue(result.Length > 0, "Result length is 0");
        }

        [TestMethod]
        public async Task CassandraCache_SetAndGetAsync_InsertRowAndReturnValue()
        {
            var key = "test" + Guid.NewGuid();
            var value = Encoding.UTF8.GetBytes("this is a test");
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(1),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            };

            await this.cassandraCache.SetAsync(key, value, options);

            var result = await this.cassandraCache.GetAsync(key);

            Assert.IsNotNull(result, "Result is null");
            Assert.IsTrue(result.Length > 0, "Result length is 0");
        }

        [TestMethod]
        public void CassandraCache_Remove_RemoveRowFromDb()
        {
            var key = "test" + Guid.NewGuid();
            var value = Encoding.UTF8.GetBytes("this is a test");
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(1),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            };

            this.cassandraCache.Set(key, value, options);

            this.cassandraCache.Remove(key);
            var result = this.cassandraCache.Get(key);

            Assert.IsNull(result, "Result is not null");
        }

        [TestMethod]
        public async Task CassandraCache_RemoveAsync_RemoveRowFromDb()
        {
            var key = "test" + Guid.NewGuid();
            var value = Encoding.UTF8.GetBytes("this is a test");
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(1),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            };

            await this.cassandraCache.SetAsync(key, value, options);

            await this.cassandraCache.RemoveAsync(key);

            var result = await this.cassandraCache.GetAsync(key);

            Assert.IsNull(result, "Result is not null");
        }
    }
}
