using Arc4u.Caching;
using Microsoft.Identity.Web.TokenCacheProviders;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.Msal.Token;

public class Cache : MsalAbstractTokenCacheProvider
{
    public Cache(ICacheContext cacheContext)
    {
        _tokenCache = string.IsNullOrWhiteSpace(cacheContext.Principal?.CacheName) ? cacheContext.Default : cacheContext[cacheContext.Principal?.CacheName];
    }

    private readonly ICache _tokenCache;

    protected override async Task<byte[]> ReadCacheBytesAsync(string cacheKey) => await _tokenCache.GetAsync<byte[]>(cacheKey).ConfigureAwait(false);

    protected override async Task RemoveKeyAsync(string cacheKey) => await _tokenCache.RemoveAsync(cacheKey).ConfigureAwait(false);

    protected override async Task WriteCacheBytesAsync(string cacheKey, byte[] bytes) => await _tokenCache.PutAsync(cacheKey, bytes).ConfigureAwait(false);
}
