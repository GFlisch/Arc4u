using Arc4u.Caching;
using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Token;

namespace Arc4u.OAuth2.Security.Principal;

[Export(typeof(ISecureCache)), Shared]
public class ServerPrincipalCache : ISecureCache
{
    public ServerPrincipalCache(ICacheHelper cacheHelper)
    {
        _cache = cacheHelper.GetCache();
    }

    private ICache? _cache;
    private bool disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                _cache = null;
                disposed = true;
            }
        }
    }

    public TValue? Get<TValue>(string key)
    {
        if (_cache is null)
        {
            return default;
        }
        return _cache.Get<TValue>(key) ;
    }

    public async Task<TValue?> GetAsync<TValue>(string key, CancellationToken cancellation = default)
    {
        if (_cache is null)
        {
            return default;
        }
        return await _cache.GetAsync<TValue>(key, cancellation).ConfigureAwait(false);
    }

    public void Initialize(string store)
    {
    }

    public void Put<T>(string key, T value)
    {
        _cache?.Put(key, value);
    }

    public void Put<T>(string key, TimeSpan timeout, T value, bool isSlided = false)
    {
        _cache?.Put(key, timeout, isSlided);
    }

    public async Task PutAsync<T>(string key, T value, CancellationToken cancellation = default)
    {
        if (_cache is null)
        {
            return;
        }
        await _cache.PutAsync(key, value, cancellation).ConfigureAwait(false);
    }

    public async Task PutAsync<T>(string key, TimeSpan timeout, T value, bool isSlided = false, CancellationToken cancellation = default)
    {
        if (_cache is null)
        {
            return;
        }
        await _cache.PutAsync(key, timeout, value, isSlided, cancellation).ConfigureAwait(false);
    }

    public bool Remove(string key)
    {
        return _cache?.Remove(key) ?? false;
    }

    public async Task<bool> RemoveAsync(string key, CancellationToken cancellation = default)
    {
        return _cache is null ? false : await _cache.RemoveAsync(key, cancellation).ConfigureAwait(false);
    }

    public bool TryGetValue<TValue>(string key, out TValue? value)
    {
        if (_cache is null)
        {
            value = default;
            return false;
        }

        return _cache.TryGetValue(key, out value);
    }

    public async Task<TValue?> TryGetValueAsync<TValue>(string key, CancellationToken cancellation = default)
    {
        if (_cache is null)
        {
            return default!;
        }

        return await _cache.TryGetValueAsync<TValue>(key, cancellation).ConfigureAwait(false);
    }
}
