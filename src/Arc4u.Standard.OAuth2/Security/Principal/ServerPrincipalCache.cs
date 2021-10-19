using Arc4u.Caching;
using Arc4u.OAuth2.Token;
using System;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.Security.Principal
{
    [Export(typeof(ISecureCache)), Shared]
    public class ServerPrincipalCache : ISecureCache
    {
        public ServerPrincipalCache(CacheHelper cacheHelper)
        {
            _cache = cacheHelper.GetCache();
        }

        private ICache _cache;
        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
                if (disposing)
                {
                    _cache = null;
                    disposed = true;
                }
        }


        public TValue Get<TValue>(string key)
        {
            return _cache.Get<TValue>(key);
        }

        public Task<TValue> GetAsync<TValue>(string key, CancellationToken cancellation = default)
        {
            return _cache.GetAsync<TValue>(key, cancellation);
        }

        public void Initialize(string store)
        {
        }

        public void Put<T>(string key, T value)
        {
            _cache.Put(key, value);
        }

        public void Put<T>(string key, TimeSpan timeout, T value, bool isSlided = false)
        {
            _cache.Put(key, timeout, isSlided);
        }

        public async Task PutAsync<T>(string key, T value, CancellationToken cancellation = default)
        {
            await _cache.PutAsync(key, value, cancellation);
        }

        public async Task PutAsync<T>(string key, TimeSpan timeout, T value, bool isSlided = false, CancellationToken cancellation = default)
        {
            await _cache.PutAsync(key, timeout, value, isSlided, cancellation);
        }

        public bool Remove(string key)
        {
            return _cache.Remove(key);
        }

        public async Task<bool> RemoveAsync(string key, CancellationToken cancellation = default)
        {
            return await _cache.RemoveAsync(key, cancellation);
        }

        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public async Task<TValue> TryGetValueAsync<TValue>(string key, CancellationToken cancellation = default)
        {
            return await _cache.TryGetValueAsync<TValue>(key, cancellation);
        }
    }
}
