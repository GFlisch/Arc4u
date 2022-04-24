using Arc4u.Caching;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Microsoft.Extensions.Logging;
using System;

namespace Arc4u.OAuth2.Token
{
    [Export, Shared]
    public class CacheHelper
    {
        public CacheHelper(CacheContext cacheContext, ILogger logger)
        {
            try
            {
                _cacheContext = cacheContext;
                _logger = logger;
                _cache = GetCacheFromConfig();
            }
            catch (Exception ex)
            {
                logger.Technical().From<CacheHelper>().Exception(ex).Log();
                logger.Technical().From<CacheHelper>().System("Use the default cache!").Log();

                _cache = cacheContext.Default;
            }

        }

        private readonly CacheContext _cacheContext;
        private readonly ILogger _logger;
        /// return the cache based on:
        /// 1) a cache exists with the Principal.CacheName
        /// 2) default.
        private ICache GetCacheFromConfig()
        {
            var cacheName = _cacheContext.Principal?.CacheName;

            if (!String.IsNullOrWhiteSpace(cacheName) && _cacheContext.Exist(cacheName))
            {
                _logger.Technical().From<CacheHelper>().System($"The Owin token cache is {cacheName}.").Log();
                return _cacheContext[cacheName];
            }

            return _cacheContext.Default;
        }

        private readonly ICache _cache;

        public ICache GetCache() => _cache;
    }
}
