namespace DistributedCache.Cassandra
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using DistributedCache.Cassandra.Helpers;
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
            var rowSet = this.cacheOptions.Session.Execute(this.CreateGetBoundStatement(key));

            return CassandraCacheHelper.GetReturnValue(rowSet);
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var rowSet = await this.cacheOptions.Session.ExecuteAsync(this.CreateGetBoundStatement(key));

            return CassandraCacheHelper.GetReturnValue(rowSet);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            this.cacheOptions.Session.Execute(this.CreateSetBoundStatement(key, value, options));
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            await this.cacheOptions.Session.ExecuteAsync(this.CreateSetBoundStatement(key, value, options));
        }

        public void Remove(string key)
        {
            this.cacheOptions.Session.Execute(this.CreateRemoveBoundStatement(key));
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            await this.cacheOptions.Session.ExecuteAsync(this.CreateRemoveBoundStatement(key));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public void Refresh(string key)
        {
            // Sliding Expiration not implemented
        }

        public Task RefreshAsync(string key, CancellationToken token = default(CancellationToken))
        {
            // Sliding Expiration not implemented
            return Task.CompletedTask;
        }

        private IStatement CreateGetBoundStatement(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var boundStatement = this.preparedStatements[DbOperations.Select].Bind(key).SetConsistencyLevel(this.cacheOptions.ReadConsistencyLevel);
            return boundStatement;
        }

        private IStatement CreateSetBoundStatement(string key, byte[] value, DistributedCacheEntryOptions options)
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
            return boundStatement;
        }

        private IStatement CreateRemoveBoundStatement(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var boundStatement = this.preparedStatements[DbOperations.Delete].Bind(key).SetConsistencyLevel(this.cacheOptions.WriteConsistencyLevel);
            return boundStatement;
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
