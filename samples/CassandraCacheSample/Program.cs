namespace CassandraCacheSample
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Cassandra;
    using Microsoft.Extensions.Caching.Distributed;
    using Caching.Cassandra;

    public class Program
    {
        public static void Main(string[] args)
        {
            RunSampleAsync().Wait();
        }

        private static async Task RunSampleAsync()
        {
            var key = "myKey";
            var message = "Hello, World!";
            var value = Encoding.UTF8.GetBytes(message);

            Console.WriteLine("Connecting to cassandra");

            var session = Cluster.Builder()
                .AddContactPoint("localhost")
                .WithCredentials("cassandra", "cassandra")
                .WithPort(9042)
                .WithDefaultKeyspace("cachecontroltest")
                .Build()
                .Connect();

            Console.WriteLine("Connected");

            var cache = new CassandraCache(new CassandraCacheOptions
            {
                Session = session,
                Logger = null,
                ReadConsistencyLevel = ConsistencyLevel.LocalOne,
                WriteConsistencyLevel = ConsistencyLevel.LocalOne
            });

            Console.WriteLine("Cassandra Cache initialized");

            Console.WriteLine($"Setting value '{message}' in cache");
            await cache.SetAsync(key, value, new DistributedCacheEntryOptions());
            Console.WriteLine("Value set");

            Console.WriteLine("Getting value from cache");
            var result = await cache.GetAsync(key);

            if (result != null)
            {
                Console.WriteLine("Retrieved: " + Encoding.UTF8.GetString(result));
            }
            else
            {
                Console.WriteLine("Value not found");
            }

            Console.WriteLine("Removing value from cache");
            await cache.RemoveAsync(key);
            Console.WriteLine("Value removed");

            Console.WriteLine("Getting value from cache again");
            result = await cache.GetAsync(key);

            if (result != null)
            {
                Console.WriteLine("Retrieved: " + Encoding.UTF8.GetString(result));
            }
            else
            {
                Console.WriteLine("Value not found");
            }

            Console.ReadLine();
        }
    }
}
