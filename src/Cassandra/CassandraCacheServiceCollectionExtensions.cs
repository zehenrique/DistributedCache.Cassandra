namespace Caching.Cassandra
{
    using System;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.DependencyInjection;

    public static class CassandraCacheServiceCollectionExtensions
    {
        public static IServiceCollection AddDistributedCassandraCache(this IServiceCollection services, Action<CassandraCacheOptions> options)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));

            }

            services.AddOptions();
            services.Configure(options);
            services.Add(ServiceDescriptor.Singleton<IDistributedCache, CassandraCache>());

            return services;
        }
    }
}
