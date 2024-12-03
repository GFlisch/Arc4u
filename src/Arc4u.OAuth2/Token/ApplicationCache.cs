using Arc4u.Caching;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Token;

/// <summary>
/// Read the cache used to store the tokens! If nothing is identified, Default is used!
/// </summary>
[Export(typeof(ITokenCache)), Shared]
public class ApplicationCache(ICacheHelper cacheHelper, ILogger logger, IOptions<TokenCacheOptions> options) : ITokenCache
{
    private readonly ICache _cache = cacheHelper.GetCache();
    private readonly TokenCacheOptions _tokenCacheOptions = options.Value;

    /// <summary>
    /// Remove at the same time the token and the extra claims added to the cache via a call to an implementation of the IClaimsFiller...
    /// </summary>
    /// <param name="key"></param>
    public void DeleteItem(string key)
    {
        logger.Technical().From<ApplicationCache>().System($"Deleting information from the token cache for the id: {key}.").Log();
        _cache.Remove(ApplicationCache.GetKey(key));
        logger.Technical().From<ApplicationCache>().System($"Deleted information from the token cache for the id: {key}.").Log();
    }

    public void Put<T>(string key, T data)
    {
        if (null == data)
        {
            logger.Technical().From<ApplicationCache>().System("A null token data information was provided to the cache. We skip this data from the cache.");
            return;
        }

        logger.Technical().From<ApplicationCache>().System($"Adding token data information to the cache: {key}.").Log();
        _cache.Put(ApplicationCache.GetKey(key), _tokenCacheOptions.MaxTime, data);
        logger.Technical().From<ApplicationCache>().System($"Added token data information to the cache: {key}.").Log();
    }

    public T? Get<T>(string key)
    {
        logger.Technical().From<ApplicationCache>().System($"Retrieve token information for user: {key}.").Log();
        var data = _cache.Get<T>(ApplicationCache.GetKey(key));

        if (null == data)
        {
            logger.Technical().From<ApplicationCache>().System($"The data in cache is null for user: {key}.").Log();
        }

        return data;
    }

    public IEnumerable<byte[]> GetAll()
    {
        logger.Technical().From<ApplicationCache>().Warning("Geting all data from the token cache is not implemented.").Log();

        return [];
    }

    private static string GetKey(string id)
    {
        return (id + "_TokenCache").ToLowerInvariant();
    }

}
