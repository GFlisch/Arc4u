using Arc4u.Configuration;
using Arc4u.Dependency;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Arc4u.Caching
{

    /// <summary>
    /// CacheContext is a helper class allowing to easily access the caches defined in the caching configuration section.
    /// </summary>
    [Export(typeof(CacheContext)), Shared]
    public class CacheContext
    {
        // Constant used to resolve the ICache interface to retrieve the MemoryCache implementation.
        public const String Memory = "Memory";
        // Constant used to resolve the ICache interface to retrieve the Sql implementation.
        public const String Sql = "Sql";
        // Constant used to resolve the ICache interface to retrieve the Redis implementation.
        public const String Redis = "Redis";

        private Dictionary<String, ICache> _caches = new Dictionary<string, ICache>();
        private Dictionary<String, String> _uninitializedCaches = new Dictionary<string, string>();
        private String _cacheConfigName;

        private static object _lock = new object();

        private readonly IContainerResolve _dependency;
        private readonly ILogger _logger;

        public CachingPrincipal Principal { get; set; }

        /// <summary>
        /// Initialise the cache following the caching config section.
        /// </summary>
        public CacheContext(IConfiguration configuration, ILogger logger, IContainerResolve dependency)
        {
            _logger = logger;
            _dependency = dependency;
            InitializeFromConfig(configuration);
        }

        /// <summary>
        /// Accessor to retrieve the default cache defined in the cachinf config section of the config file.
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

                if (null != config.Default && !String.IsNullOrWhiteSpace(config.Default))
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

                                    _logger.Technical().From<CacheContext>().System($"Instantiate a new instance of a cache with a kind of {cacheConfig.Kind} and named {cacheConfig.Name}.").Log();
                                }
                                else
                                    _logger.Technical().From<CacheContext>().Error($"Cannot resolve an ICache instance with the name: {cacheConfig.Kind}.").Log();
                            }
                            else
                            {
                                _uninitializedCaches.Add(cacheConfig.Name, cacheConfig.Kind);
                                _logger.Technical().From<CacheContext>().System($"Register a cache with a kind of {cacheConfig.Kind} and named {cacheConfig.Name} for later.").Log();
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
                if (_caches.ContainsKey(cacheName))
                    return _caches[cacheName];

                // look if the cache is not yet started and start it.
                lock (_lock)
                {
                    if (_uninitializedCaches.ContainsKey(cacheName))
                    {
                        try
                        {
                            var cache = _dependency.Resolve<ICache>(_uninitializedCaches[cacheName]);

                            cache.Initialize(cacheName);

                            _caches.Add(cacheName, cache);
                            _uninitializedCaches.Remove(cacheName);

                            return cache;
                        }
                        catch (Exception ex)
                        {
                            _logger.Technical().From<CacheContext>().Exception(ex).Log();
                        }
                    }
                }

                throw new InvalidOperationException($"There is no cache configured with the name {cacheName}.");
            }

        }
    }
}
