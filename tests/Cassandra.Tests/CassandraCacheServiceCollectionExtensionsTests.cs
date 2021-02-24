namespace DistributedCache.Cassandra.Tests
{
    using System.Linq;
    using global::Cassandra;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    [TestCategory("UnitTests")]
    public class CassandraCacheServiceCollectionExtensionsTests
    {
        [TestMethod]
        public void CassandraCacheServiceCollectionExtensions_AddDistributedCassandraCache_RegisterDistributedCacheAsSingleton()
        {
            var services = new ServiceCollection();

            services.AddDistributedCassandraCache(option => { });

            var distributedCache = services.FirstOrDefault(service => service.ServiceType.Equals(typeof(IDistributedCache)));

            Assert.IsNotNull(distributedCache, "Distributed Cache was not added to the service collection");
            Assert.AreEqual(ServiceLifetime.Singleton, distributedCache.Lifetime);
        }

        [TestMethod]
        public void CassandraCacheServiceCollectionExtensions_AddDistributedCassandraCache_ReplacesPreviouslyUserRegisteredServices()
        {
            var services = new ServiceCollection();
            var distributedCacheMock = new Mock<IDistributedCache>();
            var sessionMock = new Mock<ISession>();

            services.AddScoped(typeof(IDistributedCache), sp => distributedCacheMock.Object);

            services.AddDistributedCassandraCache(options => { options.Session = sessionMock.Object; });

            var serviceProvider = services.BuildServiceProvider();
            var distributedCache = services.FirstOrDefault(service => service.ServiceType.Equals(typeof(IDistributedCache)));

            Assert.IsNotNull(distributedCache, "Distributed Cache was not added to the service collection");
            Assert.AreEqual(ServiceLifetime.Scoped, distributedCache.Lifetime);
            Assert.IsInstanceOfType(serviceProvider.GetRequiredService<IDistributedCache>(), typeof(CassandraCache));
        }

        [TestMethod]
        public void CassandraCacheServiceCollectionExtensions_AddDistributedCassandraCache_AllowsChaning()
        {
            var services = new ServiceCollection();

            Assert.AreSame(services, services.AddDistributedCassandraCache(_ => { }));
        }
    }
}
