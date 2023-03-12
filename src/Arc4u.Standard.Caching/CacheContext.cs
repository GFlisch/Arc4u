using System;
using System.Collections.Generic;
using Arc4u.Configuration;
using Arc4u.Dependency;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Arc4u.Caching;

/// <summary>
/// CacheContext is a helper class allowing to easily access the caches defined in the caching configuration section.
/// </summary>
[Export(typeof(ICacheContext)), Shared]
public class CacheContext : ICacheContext
{
    // Constant used to resolve the ICache interface to retrieve the MemoryCache implementation.
    public const string Memory = "Memory";

    // Constant used to resolve the ICache interface to retrieve the Sql implementation.
    public const string Sql = "Sql";

    // Constant used to resolve the ICache interface to retrieve the Redis implementation.
    public const string Redis = "Redis";

    public const string Dapr = "Dapr";

    private readonly Dictionary<string, ICache> _caches = new();
    private readonly Dictionary<string, string> _uninitializedCaches = new();
    private string _cacheConfigName;

    private static readonly object _lock = new();

    private readonly IContainerResolve _dependency;
    private readonly ILogger<CacheContext> _logger;

    public CachingPrincipal Principal { get; set; }

    /// <summary>
    /// Initialise the cache following the caching config section.
    /// </summary>
    public CacheContext(IConfiguration configuration, ILogger<CacheContext> logger, IContainerResolve dependency)
    {
        _logger = logger;
        _dependency = dependency;
        InitializeFromConfig(configuration);
    }

    /// <summary>
    /// Accessor to retrieve the default cache defined in the caching config section of the config file.
    /// </summary>
    public ICache Default
    {
        get { return this[_cacheConfigName]; }
    }

    private void InitializeFromConfig(IConfiguration configuration)
    {
        lock (_lock)
        {
            if (null == configuration)
            {
                return;
            }

            var config = new Configuration.Caching();
            configuration.Bind("Caching", config);

            Principal = config.Principal;

            if (null != config.Default && !string.IsNullOrWhiteSpace(config.Default))
            {
                _cacheConfigName = config.Default;

                if (null != config.Caches)
                {
                    // retrieve the caches and start if asked!
                    foreach (var cacheConfig in config.Caches)
                    {
                        if (cacheConfig.IsAutoStart)
                        {
                            if (_dependency.TryResolve<ICache>(cacheConfig.Kind, out var cache))
                            {
                                cache.Initialize(cacheConfig.Name);

                                _caches.Add(cacheConfig.Name, cache);

                                _logger.Technical().System($"Instantiate a new instance of a cache with a kind of {cacheConfig.Kind} and named {cacheConfig.Name}.").Log();
                            }
                            else
                            {
                                _logger.Technical().LogError($"Cannot resolve an ICache instance with the name: {cacheConfig.Kind}.");
                            }
                        }
                        else
                        {
                            _uninitializedCaches.Add(cacheConfig.Name, cacheConfig.Kind);
                            _logger.Technical().System($"Register a cache with a kind of {cacheConfig.Kind} and named {cacheConfig.Name} for later.").Log();
                        }
                    }
                }
            }
        }
    }

    public bool Exist(string cacheName)
    {
        return _caches.ContainsKey(cacheName) || _uninitializedCaches.ContainsKey(cacheName);
    }

    /// <summary>
    /// Indexer to retrieve the <see cref="ICache"/> implementation based on the caching config section. The key is the cache name.
    /// </summary>
    /// <param name="cacheName"></param>
    /// <returns>An <see cref="ICache"/> implementation defined in the cache config section.</returns>
    /// <exception cref="ConfigurationErrorsException">Throw the exception when the cache name does not exist in the caching config section.</exception>
    public ICache this[string cacheName]
    {
        get
        {
            // look inside the collection if the cache is initialized.
            if (_caches.TryGetValue(cacheName, out var value))
            {
                return value;
            }

            // look if the cache is not yet started and start it.
            lock (_lock)
            {
                if (_uninitializedCaches.TryGetValue(cacheName, out var cacheKind))
                {
                    try
                    {
                        var cache = _dependency.Resolve<ICache>(cacheKind);

                        cache.Initialize(cacheName);

                        _caches.Add(cacheName, cache);
                        _uninitializedCaches.Remove(cacheName);

                        return cache;
                    }
                    catch (Exception ex)
                    {
                        _logger.Technical().LogException(ex);
                    }
                }
            }

            throw new InvalidOperationException($"There is no cache configured with the name {cacheName}.");
        }
    }
}
