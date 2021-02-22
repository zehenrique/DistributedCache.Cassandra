# Caching - Cassandra Extension
.Net Core [IDistributedCache](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-5.0) implementation for Cassandra.

## Configure

1. Configure your application to make use of [Cassandra](https://cassandra.apache.org/).

2. Add the following code to `Startup.ConfigureServices` and perform the necessary changes:

```csharp
services.AddDistributedCassandraCache(options =>
{
    options.Session = <YourCassandraSession>;
    options.Logger = <YourLogger>;
    options.ReadConsistencyLevel = ConsistencyLevel.LocalQuorum;
    options.WriteConsistencyLevel = ConsistencyLevel.LocalQuorum;
})

```

3. To use the IDistributedCache interface, request an instance of IDistributedCache from any constructor in the app. The instance is provided by dependency injection (DI).

