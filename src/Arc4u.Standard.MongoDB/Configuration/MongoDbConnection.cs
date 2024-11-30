using Arc4u.MongoDB;
using Arc4u.MongoDB.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;

namespace Microsoft.Extensions.DependencyInjection;

public static class MongoDbConnection
{
    /// <summary>
    /// Configure the Database context for the database.
    /// There is one context per database and collections per context!
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="ConnectionStringKey"></param>
    public static void AddMongoDatabase<TContext>(this IServiceCollection services, IConfiguration configuration, string ConnectionStringKey) where TContext : DbContext, new()
    {
        var ConnectionString = configuration.GetConnectionString(ConnectionStringKey);

        var mongoUrl = new MongoUrl(ConnectionString);

        services.Configure<MongoClientSettings>(mongoUrl.DatabaseName.ToLowerInvariant(), options =>
        {
            var c = MongoClientSettings.FromConnectionString(configuration.GetConnectionString(ConnectionStringKey));
            options.AllowInsecureTls = c.AllowInsecureTls;
            options.ApplicationName = c.ApplicationName;
            options.AutoEncryptionOptions = c.AutoEncryptionOptions;
            options.ClusterConfigurator = c.ClusterConfigurator;
            options.ConnectTimeout = c.ConnectTimeout;
            options.Credential = c.Credential;
            options.HeartbeatInterval = c.HeartbeatInterval;
            options.IPv6 = c.IPv6;
            options.LocalThreshold = c.LocalThreshold;
            options.MaxConnectionIdleTime = c.MaxConnectionIdleTime;
            options.MaxConnectionLifeTime = c.MaxConnectionLifeTime;
            options.MaxConnectionPoolSize = c.MaxConnectionPoolSize;
            options.MinConnectionPoolSize = c.MinConnectionPoolSize;
            options.ReadConcern = c.ReadConcern;
            options.ReadEncoding = c.ReadEncoding;
            options.ReadPreference = c.ReadPreference;
            options.ReplicaSetName = c.ReplicaSetName;
            options.RetryReads = c.RetryReads;
            options.RetryWrites = c.RetryWrites;
            options.Scheme = c.Scheme;
            options.Servers = c.Servers;
            options.ServerSelectionTimeout = c.ServerSelectionTimeout;
            options.SocketTimeout = c.SocketTimeout;
            options.SslSettings = c.SslSettings;
            options.UseTls = c.UseTls;
            options.WaitQueueTimeout = c.WaitQueueTimeout;
            options.WriteConcern = c.WriteConcern;
            options.WriteEncoding = c.WriteEncoding;
        });

        var contextBuilder = new DbContextBuilder(services, mongoUrl.DatabaseName);

        var dbContext = new TContext();

        services.TryAddSingleton(typeof(TContext), (provider) => dbContext);
        services.TryAddSingleton(typeof(IMongoClientFactory<TContext>), typeof(DefaultMongoClientFactory<TContext>));

        dbContext.Configure(contextBuilder);
    }

    public static void AddMongoDatabase<TContext>(this IServiceCollection services, string databaseName, Action<MongoClientSettings> options) where TContext : DbContext, new()
    {
        services.Configure<MongoClientSettings>(databaseName.ToLowerInvariant(), options);

        var contextBuilder = new DbContextBuilder(services, databaseName);

        var dbContext = new TContext();

        services.TryAddSingleton(typeof(TContext), (provider) => dbContext);
        services.TryAddSingleton(typeof(IMongoClientFactory<TContext>), typeof(DefaultMongoClientFactory<TContext>));

        dbContext.Configure(contextBuilder);
    }
}
