namespace Caching.Cassandra
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Caching.Cassandra.Helpers;
    using global::Cassandra;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class CassandraCache : IDistributedCache
    {
        private readonly ConcurrentDictionary<DbOperations, PreparedStatement> preparedStatements = new ConcurrentDictionary<DbOperations, PreparedStatement>();
        private readonly CassandraCacheOptions cacheOptions;

        public CassandraCache(IOptions<CassandraCacheOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.Value.Session == null)
            {
                throw new ArgumentNullException(nameof(options.Value.Session).ToString());
            }

            this.cacheOptions = options.Value;

            this.InitializePreparedStatements();
        }

        public byte[] Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var boundStatement = this.preparedStatements[DbOperations.Select].Bind(key).SetConsistencyLevel(this.cacheOptions.ReadConsistencyLevel);

            var rowSet = this.cacheOptions.Session.Execute(boundStatement);

            return CassandraCacheHelper.GetReturnValue(rowSet);
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            var boundStatement = this.preparedStatements[DbOperations.Select].Bind(key).SetConsistencyLevel(this.cacheOptions.ReadConsistencyLevel);

            var rowSet = await this.cacheOptions.Session.ExecuteAsync(boundStatement);

            return CassandraCacheHelper.GetReturnValue(rowSet);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var creationTime = DateTimeOffset.UtcNow;
            var expiryDate = CassandraCacheHelper.GetAbsoluteExpiration(creationTime, options);
            var ttl = CassandraCacheHelper.GetExpirationInSeconds(creationTime, expiryDate);

            var boundStatement = this.preparedStatements[DbOperations.Insert].Bind(key, expiryDate, value, ttl).SetConsistencyLevel(this.cacheOptions.WriteConsistencyLevel);

            this.cacheOptions.Session.Execute(boundStatement);
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            token.ThrowIfCancellationRequested();

            var creationTime = DateTimeOffset.UtcNow;
            var expiryDate = CassandraCacheHelper.GetAbsoluteExpiration(creationTime, options);
            var ttl = CassandraCacheHelper.GetExpirationInSeconds(creationTime, expiryDate);

            var boundStatement = this.preparedStatements[DbOperations.Insert].Bind(key, expiryDate, value, ttl).SetConsistencyLevel(this.cacheOptions.WriteConsistencyLevel);

            await this.cacheOptions.Session.ExecuteAsync(boundStatement);
        }

        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var boundStatement = this.preparedStatements[DbOperations.Delete].Bind(key).SetConsistencyLevel(this.cacheOptions.WriteConsistencyLevel);

            this.cacheOptions.Session.Execute(boundStatement);
        }

        public async Task RemoveAsync(string key, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            var boundStatement = this.preparedStatements[DbOperations.Delete].Bind(key).SetConsistencyLevel(this.cacheOptions.WriteConsistencyLevel);

            await this.cacheOptions.Session.ExecuteAsync(boundStatement);
        }

        public void Refresh(string key)
        {
            throw new NotImplementedException();
        }

        public Task RefreshAsync(string key, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        private void InitializePreparedStatements()
        {
            try
            {
                this.preparedStatements.GetOrAdd(DbOperations.Select, this.cacheOptions.Session.Prepare("SELECT value FROM cassandra_cache WHERE id=?"));
                this.preparedStatements.GetOrAdd(DbOperations.Insert, this.cacheOptions.Session.Prepare("INSERT INTO cassandra_cache (id, expiration_time, value) VALUES (?, ?, ?) USING TTL ?"));
                this.preparedStatements.GetOrAdd(DbOperations.Update, this.cacheOptions.Session.Prepare("UPDATE cassandra_cache SET expiration_time=?, value=? WHERE id=?"));
                this.preparedStatements.GetOrAdd(DbOperations.Delete, this.cacheOptions.Session.Prepare("DELETE FROM cassandra_cache WHERE id=?"));
            }
            catch (Exception ex)
            {
                this.cacheOptions.Logger.LogError("Something went wrong when trying to prepare cassandra statements.", ex);
                throw;
            }
        }
    }
}
