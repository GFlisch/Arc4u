using Arc4u.Dependency;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.Serializer;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Logging;
using System;
using StackExchangeRedis = Microsoft.Extensions.Caching.StackExchangeRedis.RedisCache;

namespace Arc4u.Caching.Redis;

[Export("Redis", typeof(ICache))]
public class RedisCache : BaseDistributeCache, ICache
{
    public const string ConnectionStringKey = "ConnectionString";
    public const string DatabaseNameKey = "Name";
    private const string SerializerNameKey = "SerializerName";

    private String Name { get; set; }

    private readonly ILogger<RedisCache> _logger;

    public RedisCache(ILogger<RedisCache> logger, IContainerResolve container) : base(container)
    {
        _logger = logger;
    }

    public override void Initialize(string store)
    {
        lock (_lock)
        {
            if (IsInitialized)
            {
                _logger.Technical().Debug($"Redis Cache {store} is already initialized.").Log();
                return;
            }
            Name = store;

            if (Container.TryResolve<IKeyValueSettings>(store, out var settings))
            {
                if (settings.Values.ContainsKey(ConnectionStringKey))
                {
                    ConnectionString = settings.Values[ConnectionStringKey];
                }

                if (settings.Values.ContainsKey(DatabaseNameKey))
                {
                    DatabaseName = settings.Values[DatabaseNameKey];
                }

                if (settings.Values.ContainsKey(SerializerNameKey))
                {
                    SerializerName = settings.Values[SerializerNameKey];
                }
                else
                {
                    SerializerName = store;
                }

                var option = new RedisCacheOptions
                {
                    InstanceName = DatabaseName,
                    Configuration = ConnectionString
                };

                DistributeCache = new StackExchangeRedis(option);

                if (!Container.TryResolve<IObjectSerialization>(SerializerName, out var serializerFactory))
                {
                    SerializerFactory = Container.Resolve<IObjectSerialization>();
                }
                else
                {
                    SerializerFactory = serializerFactory;
                }

                IsInitialized = true;
                _logger.Technical().System($"Redis Cache {store} is initialized.").Log();

            }
        }
    }

    public override string ToString() => Name;

    private string ConnectionString { get; set; }
    private string DatabaseName { get; set; } = "Default";
    private string SerializerName { get; set; }
}
