using Arc4u.Caching;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.Token
{
    /// <summary>
    /// This cache is used for a local client application
    /// </summary>
    [Export(typeof(ITokenCache)), Shared]
    public class ApplicationLocalDataCache : ITokenCache
    {
        private object? _token;
        private readonly ICache Cache;
        private readonly ILogger Logger;

        public ApplicationLocalDataCache(ISecureCache cache, ILogger logger)
        {
            Cache = cache;
            Logger = logger;
        }

        public void Clear(string key)
        {
            try
            {
                Cache.Remove(key);
                _token = null;
            }
            catch { }
        }

        public void DeleteItem(string key)
        {
            Clear(key);
        }

        public void Put<T>(string key, TimeSpan timeout, T data)
        {
            try
            {
                Cache.Put(key, data);
                _token = data;
            }
            catch (Exception ex)
            {
                Logger.Technical().From<ApplicationLocalDataCache>().Exception(ex).Log();
            }

        }

        public T Get<T>(string key)
        {
            if (_token is T token)
            {
                return token;
            }

            Load<T>(key);

            return (T)_token;
        }

        void Load<T>(string key)
        {
            _token = null;

            try
            {
                _token = Cache.Get<T>(key);
            }
            catch (Exception ex)
            {
                Logger.Technical().From<ApplicationLocalDataCache>().Exception(ex).Log();
            }
        }

    }
}
