namespace DistributedCache.Cassandra.Helpers
{
    internal static class ColumnNames
    {
        public const string Id = "id";
        public const string Value = "value";
        public const string ExpirationTime = "expiration_time";
    }

    internal static class DefaultValues
    {
        /// <summary>
        /// The default time-to-live value (in seconds).
        /// </summary>
        public const int DefaultTtl = 3600;
    }
}
