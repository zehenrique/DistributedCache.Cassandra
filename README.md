# Distributed Cache - Cassandra Extension
.Net Core [IDistributedCache](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-5.0) implementation for [Cassandra](https://cassandra.apache.org/).

## Configure

1. Create the table *cassandra_cache* in your Cassandra Keyspace. You can find the cql file inside the *deploy* folder.

2. Configure your application to make use of Cassandra.

3. Add the following code to `Startup.ConfigureServices` and perform the necessary changes:
```csharp
services.AddDistributedCassandraCache(options =>
{
    options.Session = <YourCassandraSession>;
    options.Logger = <YourLogger>;
    options.ReadConsistencyLevel = ConsistencyLevel.LocalQuorum;
    options.WriteConsistencyLevel = ConsistencyLevel.LocalQuorum;
})
```

4. To use the IDistributedCache interface, request an instance of IDistributedCache from any constructor in the app. The instance is provided by dependency injection (DI).

