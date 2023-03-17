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

#if NET6_0_OR_GREATER
    public override void Initialize([DisallowNull] string store)
#else
    public override void Initialize(string store)
#endif
    {

#if NET6_0_OR_GREATER
#if NET7_0_OR_GREATER
        ArgumentNullException.ThrowIfNullOrEmpty(store, nameof(store));
#else
        ArgumentNullException.ThrowIfNull(store, nameof(store));
#endif
#else
        if (store is null)
        {
            throw new ArgumentNullException(nameof(store));
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
