namespace DistributedCache.Cassandra
{
    using global::Cassandra;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class CassandraCacheOptions : IOptions<CassandraCacheOptions>
    {
        public ISession Session { get; set; }

        public ILogger Logger { get; set; }

        public ConsistencyLevel ReadConsistencyLevel { get; set; } = ConsistencyLevel.LocalQuorum;

        public ConsistencyLevel WriteConsistencyLevel { get; set; } = ConsistencyLevel.LocalQuorum;

        CassandraCacheOptions IOptions<CassandraCacheOptions>.Value => this;
    }
}
