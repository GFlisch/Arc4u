using Arc4u.Configuration.Memory;
using Arc4u.Dependency;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.Serializer;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Arc4u.Caching.Memory;

[Export("Memory", typeof(ICache))]
public class MemoryCache : BaseDistributeCache<MemoryCache>, ICache
{
    private string? Name { get; set; }

    private readonly ILogger<MemoryCache> _logger;
    /// <summary>
    /// This is a named options => the configuration named instance for this instance will be taken during the intialize method. 
    /// </summary>
    private readonly IOptionsMonitor<MemoryCacheOption> _options;

    public MemoryCache(ILogger<MemoryCache> logger, IContainerResolve container, IOptionsMonitor<MemoryCacheOption> options) : base(logger, container)
    {
        _logger = logger;
        _options = options;
    }

#if NET6_0_OR_GREATER
    public override void Initialize([DisallowNull] string store)
#else
    public override void Initialize(string store)
#endif
    {
#if NET8_0_OR_GREATER
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
                _logger.Technical().System($"Memory Cache {store} is already initialized.").Log();
                return;
            }

            Name = store;

            var config = _options.Get(store);

            var option = new DistriOption(new MemoryDistributedCacheOptions
            {
                CompactionPercentage = config.CompactionPercentage,
                SizeLimit = config.SizeLimitInMB
            });

            DistributeCache = new MemoryDistributedCache(option);


            if (!string.IsNullOrWhiteSpace(config.SerializerName))
            {
                IsInitialized = Container.TryResolve<IObjectSerialization>(config.SerializerName!, out var serializerFactory);
                SerializerFactory = serializerFactory;
            }

            if (!IsInitialized)
            {
                IsInitialized = Container.TryResolve<IObjectSerialization>(out var serializerFactory);
                SerializerFactory = serializerFactory;
            }

            if (!IsInitialized)
            {
                _logger.Technical().LogError($"Memory Cache {store} is not initialized. An IObjectSerialization instance cannot be resolved via the Ioc.");
                return;
            }
            _logger.Technical().System($"Memory Cache {store} is initialized.").Log();
        }
    }

    public override string ToString() => Name ?? throw new NullReferenceException();
}
