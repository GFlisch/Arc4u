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
public class RedisCache : BaseDistributeCache<RedisCache>, ICache
{
    private string? Name { get; set; }

    private readonly ILogger<RedisCache> _logger;

    /// <summary>
    /// This is a named options => the configuration named instance for this instance will be taken during the intialize method. 
    /// </summary>
    private readonly IOptionsMonitor<RedisCacheOption> _options;

    public RedisCache(ILogger<RedisCache> logger, IServiceProvider container, IOptionsMonitor<RedisCacheOption> options) : base(logger, container)
    {
        _logger = logger;
        _options = options;
    }

    public override void Initialize([DisallowNull] string store)
    {
        if (string.IsNullOrEmpty(store))
        {
            NotInitializedReason = "When initializing the Redis cache, the value of the store cannot be an empty string.";
            throw new ArgumentException(NotInitializedReason, nameof(store));
        }

        lock (_lock)
        {
            if (IsInitialized)
            {
                _logger.Technical().System($"Redis Cache {store} is already initialized.").Log();
                return;
            }

            try
            {
                Name = store;

                var config = _options.Get(store);

                var redisOption = new RedisCacheOptions
                {
                    InstanceName = config.InstanceName,
                    Configuration = config.ConnectionString,
                };

                DistributeCache = new StackExchangeRedis(redisOption);

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
                    NotInitializedReason = $"Redis Cache {store} is not initialized. An IObjectSerialization instance cannot be resolved via the Ioc.";

                    _logger.Technical().LogError(NotInitializedReason);

                    return;
                }

                _logger.Technical().System($"Redis Cache {store} is initialized.").Log();
            }
            catch (Exception ex)
            {
                NotInitializedReason = $"Redis Cache {store} is not initialized. With exception: {ex.Message}";
                throw;
            }

        }
    }

    public override string ToString() => Name ?? throw new InvalidOperationException("The 'Name' property must not be null.");
}
