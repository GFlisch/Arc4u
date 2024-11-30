using Arc4u.Caching;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Token;

[Export(typeof(ICacheHelper)), Shared]
public class CacheHelper : ICacheHelper
{
    public CacheHelper(ICacheContext cacheContext, ILogger<CacheHelper> logger, IOptions<TokenCacheOptions> options)
    {
        _cacheContext = cacheContext;
        _logger = logger;
        _tokenCacheOptions = options.Value;

        try
        {
            _cache = GetCacheFromConfig();
        }
        catch (Exception ex)
        {
            logger.Technical().Exception(ex).Log();
            logger.Technical().System("Use the default cache!").Log();

            _cache = cacheContext.Default;
        }

    }

    private readonly ICacheContext _cacheContext;
    private readonly ILogger<CacheHelper> _logger;
    private readonly TokenCacheOptions _tokenCacheOptions;

    /// return the cache based on:
    /// 1) a cache exists with the Principal.CacheName
    /// 2) default.
    private ICache GetCacheFromConfig()
    {
        var cacheName = _tokenCacheOptions.CacheName;

        if (!string.IsNullOrWhiteSpace(cacheName) && _cacheContext.Exist(cacheName))
        {
            _logger.Technical().System($"The token cache is {cacheName}.").Log();

            return _cacheContext[cacheName];
        }

        return _cacheContext.Default;
    }

    private readonly ICache _cache;

    public ICache GetCache() => _cache;
}
