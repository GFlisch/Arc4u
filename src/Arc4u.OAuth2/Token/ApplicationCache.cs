using Arc4u.Caching;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Token;

/// <summary>
/// Read the cache used to store the tokens! If nothing is identified, Default is used!
/// </summary>
[Export(typeof(ITokenCache)), Shared]
public class ApplicationCache(ICacheHelper cacheHelper, [FromKeyedServices("Transient")] ILogger logger, IOptions<TokenCacheOptions> options) : ITokenCache
{
    private readonly ICache _cache = cacheHelper.GetCache();
    private readonly TokenCacheOptions _tokenCacheOptions = options.Value;

    /// <summary>
    /// Remove at the same time the token and the extra claims added to the cache via a call to an implementation of the IClaimsFiller...
    /// </summary>
    /// <param name="key"></param>
    public void DeleteItem(string key)
    {
        logger.Technical<ApplicationCache>().LogTrace($"Deleting information from the token cache for the id: {key}.");
        _cache.Remove(ApplicationCache.GetKey(key));
        logger.Technical<ApplicationCache>().LogTrace($"Deleted information from the token cache for the id: {key}.");
    }

    public void Put<T>(string key, T data)
    {
        if (null == data)
        {
            logger.Technical<ApplicationCache>().LogTrace("A null token data information was provided to the cache. We skip this data from the cache.");
            return;
        }

        logger.Technical<ApplicationCache>().LogTrace($"Adding token data information to the cache: {key}.");
        _cache.Put(ApplicationCache.GetKey(key), _tokenCacheOptions.MaxTime, data);
        logger.Technical<ApplicationCache>().LogTrace($"Added token data information to the cache: {key}.");
    }

    public T? Get<T>(string key)
    {
        logger.Technical<ApplicationCache>().LogTrace($"Retrieve token information for user: {key}.");
        var data = _cache.Get<T>(ApplicationCache.GetKey(key));

        if (null == data)
        {
            logger.Technical<ApplicationCache>().LogTrace($"The data in cache is null for user: {key}.");
        }

        return data;
    }

    public IEnumerable<byte[]> GetAll()
    {
        logger.Technical<ApplicationCache>().LogWarning("Geting all data from the token cache is not implemented.");

        return [];
    }

    private static string GetKey(string id)
    {
        return (id + "_TokenCache").ToLowerInvariant();
    }

}
