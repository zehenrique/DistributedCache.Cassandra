namespace DistributedCache.Cassandra.Helpers
{
    using System;
    using System.Collections.Generic;
    using global::Cassandra;
    using Microsoft.Extensions.Caching.Distributed;

    internal static class CassandraCacheHelper
    {
        internal static byte[] GetReturnValue(RowSet rowSet)
        {
            if (rowSet == null)
            {
                return default(byte[]);
            }

            var rows = new List<Row>(rowSet.GetRows());

            if (rows.Count != 0)
            {
                return rows[0].GetValue<byte[]>(ColumnNames.Value);

            }

            return default(byte[]);
        }

        internal static DateTimeOffset? GetAbsoluteExpiration(DateTimeOffset creationDate, DistributedCacheEntryOptions options)
        {
            if (options.AbsoluteExpiration.HasValue && options.AbsoluteExpiration <= creationDate)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(DistributedCacheEntryOptions.AbsoluteExpiration).ToString(), 
                    options.AbsoluteExpiration, 
                    "The absolute expiration value must be in the future.");
            }

            var absoluteExpiration = options.AbsoluteExpiration;

            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                absoluteExpiration = creationDate + options.AbsoluteExpirationRelativeToNow;
            }

            return absoluteExpiration;
        }

        internal static int GetExpirationInSeconds(DateTimeOffset creationTime, DateTimeOffset? absoluteExpiration)
        {
            if (absoluteExpiration.HasValue)
            {
                return (int)(absoluteExpiration.Value - creationTime).TotalSeconds;
            }

            return DefaultValues.DefaultTtl;
        }
    }
}
