using Arc4u.Dependency;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.Serializer;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Arc4u.Caching.Memory;

[Export("Memory", typeof(ICache))]
public class MemoryCache : BaseDistributeCache, ICache
{
    private string? Name { get; set; }

    private readonly ILogger<MemoryCache> _logger;
    /// <summary>
    /// This is a named options => the configuration named instance for this instance will be taken during the intialize method. 
    /// </summary>
    private readonly IOptionsMonitor<MemoryCacheOption> _options;

    public MemoryCache(ILogger<MemoryCache> logger, IContainerResolve container, IOptionsMonitor<MemoryCacheOption> options) : base(container)
    {
        _logger = logger;
        _options = options;
    }

    public override void Initialize([DisallowNull] string store)
    {
#if NET6_0
        ArgumentNullException.ThrowIfNull(store, nameof(store));
#endif
#if NET7_0_OR_GREATER
        ArgumentNullException.ThrowIfNullOrEmpty(store, nameof(store));
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
                SizeLimit = config.SizeLimit
            });

            DistributeCache = new MemoryDistributedCache(option);

            if (!Container.TryResolve<IObjectSerialization>(config.SerializerName, out var serializerFactory))
            {
                SerializerFactory = Container.Resolve<IObjectSerialization>();
            }
            else
            {
                SerializerFactory = serializerFactory;
            }

            IsInitialized = true;
            _logger.Technical().System($"Memory Cache {store} is initialized.").Log();
        }
    }

    public override string ToString() => Name ?? throw new NullReferenceException();
}
