using System;
using System.Diagnostics.CodeAnalysis;
using Arc4u.Configuration.Redis;
using Arc4u.Dependency;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.Serializer;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchangeRedis = Microsoft.Extensions.Caching.StackExchangeRedis.RedisCache;

namespace Arc4u.Caching.Redis;

[Export("Redis", typeof(ICache))]
public class RedisCache : BaseDistributeCache, ICache
{
    private string? Name { get; set; }

    private readonly ILogger<RedisCache> _logger;

    /// <summary>
    /// This is a named options => the configuration named instance for this instance will be taken during the intialize method. 
    /// </summary>
    private readonly IOptionsMonitor<RedisCacheOption> _options;

    public RedisCache(ILogger<RedisCache> logger, IContainerResolve container, IOptionsMonitor<RedisCacheOption> options) : base(container)
    {
        _logger = logger;
        _options = options;
    }

    public override void Initialize([DisallowNull] string store)
    {
#if NET7_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(store);
#else
        if (string.IsNullOrEmpty(store))
        {
            throw new ArgumentException("The value cannot be an empty string.", nameof(store));
        }
#endif

        lock (_lock)
        {
            if (IsInitialized)
            {
                _logger.Technical().System($"Redis Cache {store} is already initialized.").Log();
                return;
            }

            Name = store;

            var config = _options.Get(store);

            var redisOption = new RedisCacheOptions
            {
                InstanceName = config.InstanceName,
                Configuration = config.ConnectionString,
            };

            DistributeCache = new StackExchangeRedis(redisOption);

            if (!Container.TryResolve<IObjectSerialization>(config.SerializerName, out var serializerFactory))
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

    public override string ToString() => Name ?? throw new NullReferenceException();

}
