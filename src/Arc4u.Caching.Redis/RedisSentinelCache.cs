using System.Diagnostics.CodeAnalysis;
using Arc4u.Configuration.Redis;
using Arc4u.Dependency;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.Serializer;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using StackExchangeRedisCache = Microsoft.Extensions.Caching.StackExchangeRedis.RedisCache;

namespace Arc4u.Caching.Redis;

[Export("RedisSentinel", typeof(ICache))]
public class RedisSentinelCache : BaseDistributeCache<RedisSentinelCache>, ICache
{
    private string? Name { get; set; }

    private readonly ILogger<RedisSentinelCache> _logger;

    /// <summary>
    /// Named options for Sentinel Redis cache
    /// </summary>
    private readonly IOptionsMonitor<RedisSentinelCacheOption> _options;

    public RedisSentinelCache(
        ILogger<RedisSentinelCache> logger,
        IServiceProvider container,
        IOptionsMonitor<RedisSentinelCacheOption> options)
        : base(logger, container)
    {
        _logger = logger;
        _options = options;
    }

    public override void Initialize([DisallowNull] string store)
    {
        if (string.IsNullOrEmpty(store))
        {
            NotInitializedReason = "When initializing the Redis Sentinel cache, the value of the store cannot be an empty string.";
            throw new ArgumentException(NotInitializedReason, nameof(store));
        }

        lock (_lock)
        {
            if (IsInitialized)
            {
                _logger.Technical().LogCacheIsAlreadyInitialized(store);
                return;
            }

            try
            {
                Name = store;

                var config = _options.Get(store);

                if (config.SentinelEndpoints is null || config.SentinelEndpoints.Length == 0)
                {
                    throw new InvalidOperationException("At least one Sentinel endpoint must be configured.");
                }

                if (string.IsNullOrWhiteSpace(config.MasterName))
                {
                    throw new InvalidOperationException("A Sentinel master name must be configured.");
                }

                var redisOptions = new RedisCacheOptions
                {
                    InstanceName = config.InstanceName,
                    ConnectionMultiplexerFactory = async () =>
                    {
                        var sentinelAware = new ConfigurationOptions
                        {
                            AbortOnConnectFail = false,
                            TieBreaker = string.Empty,
                            ServiceName = config.MasterName,
                            AllowAdmin = false,
                        };

                        foreach (var endpoint in config.SentinelEndpoints)
                        {
                            // accepts "host:port" or "host"
                            sentinelAware.EndPoints.Add(endpoint);
                        }

                        // Let StackExchange.Redis handle Sentinel discovery based on ServiceName and EndPoints
                        return await ConnectionMultiplexer.ConnectAsync(sentinelAware).ConfigureAwait(false);
                    }
                };

                // Use the same DistributedCache implementation as the regular Redis cache
                DistributeCache = new StackExchangeRedisCache(redisOptions);

                if (!string.IsNullOrWhiteSpace(config.SerializerName))
                {
                    IsInitialized = Container.TryGetService<IObjectSerialization>(config.SerializerName!, out var serializerFactory);
                    if (IsInitialized)
                    {
                        SerializerFactory = serializerFactory!;
                    }
                }

                if (!IsInitialized)
                {
                    IsInitialized = Container.TryGetService<IObjectSerialization>(out var serializerFactory);
                    if (IsInitialized)
                    {
                        SerializerFactory = serializerFactory!;
                    }
                }

                if (!IsInitialized)
                {
                    NotInitializedReason = $"Redis Sentinel Cache {store} is not initialized. An IObjectSerialization instance cannot be resolved via the Ioc.";

                    _logger.Technical().LogError(NotInitializedReason, store);

                    return;
                }

                _logger.Technical().LogCacheIsInitialized(store);
            }
            catch (Exception ex)
            {
                NotInitializedReason = $"Redis Sentinel Cache {store} is not initialized. With exception: {ex.Message}";
                throw;
            }

        }
    }

    public override string ToString() => Name ?? throw new InvalidOperationException("The 'Name' property must not be null.");
}
