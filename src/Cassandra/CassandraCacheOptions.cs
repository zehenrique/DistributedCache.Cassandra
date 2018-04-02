namespace Caching.Cassandra
{
    using global::Cassandra;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class CassandraCacheOptions : IOptions<CassandraCacheOptions>
    {
        public ISession Session { get; set; }

        public ILogger Logger { get; set; }

        public ConsistencyLevel ReadConsistencyLevel { get; set; }

        public ConsistencyLevel WriteConsistencyLevel { get; set; }

        CassandraCacheOptions IOptions<CassandraCacheOptions>.Value => this;
    }
}
