namespace Caching.Cassandra
{
    using System;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.DependencyInjection;

    public static class CassandraCacheServiceCollectionExtensions
    {
        public static IServiceCollection AddDistributedCassandraCache(this IServiceCollection services, Action<CassandraCacheOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));

            }

            services.AddOptions();
            services.Configure(setupAction);
            services.Add(ServiceDescriptor.Singleton<IDistributedCache, CassandraCache>());

            return services;
        }
    }
}
