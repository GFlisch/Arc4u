using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.Serializer;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Arc4u.Caching
{
    public abstract class BaseDistributeCache : ICache
    {
        private bool disposed = false;

        protected IDistributedCache DistributeCache { get; set; }

        protected object _lock = new object();
        protected bool IsInitialized { get; set; }

        protected readonly IContainerResolve _container;

        protected IContainerResolve Container => _container;
        private readonly ActivitySource _activitySource;

        public BaseDistributeCache(IContainerResolve container)
        {
            if (null == container)
                throw new ArgumentNullException(nameof(container));

            _container = container;

            if (container.TryResolve<IActivitySourceFactory>(out var activitySourceFactory))
            {
                _activitySource = activitySourceFactory.GetArc4u();
            }
        }

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
                    if (null != DistributeCache && DistributeCache is IDisposable)
                        ((IDisposable)DistributeCache).Dispose();
                    disposed = true;
                }
        }

        public TValue Get<TValue>(string key)
        {
            CheckIfInitialized();

            try
            {
                byte[] blob = null;
                using (var activity = _activitySource?.StartActivity("Get from cache.", ActivityKind.Producer))
                {
                    activity?.AddTag("cacheKey", key);

                    blob = DistributeCache?.Get(key);
                    if (null == blob)
                        return default(TValue);

                    using (var serializerActivity = _activitySource?.StartActivity("Deserialize.", ActivityKind.Producer))
                        return SerializerFactory.Deserialize<TValue>(blob);

                }
            }
            catch (Exception ex)
            {
                throw new DataCacheException(ex.Message);
            }
        }


        public async Task<TValue> GetAsync<TValue>(string key, CancellationToken cancellation = default(CancellationToken))
        {
            CheckIfInitialized();

            try
            {
                byte[] blob = null;
                using (var activity = _activitySource?.StartActivity("Get from cache.", ActivityKind.Producer))
                {
                    activity?.AddTag("cacheKey", key);

                    blob = await DistributeCache?.GetAsync(key, cancellation);
                    if (null == blob)
                        return default(TValue);

                    using (var serializerActivity = _activitySource?.StartActivity("Deserialize.", ActivityKind.Producer))
                        return SerializerFactory.Deserialize<TValue>(blob);
                }

            }
            catch (Exception ex)
            {
                throw new DataCacheException(ex.Message);
            }
        }

        public virtual void Initialize(string store) { }

        protected IObjectSerialization SerializerFactory { get; set; }

        protected DistributedCacheEntryOptions DefaultOption = new DistributedCacheEntryOptions();

        public void Put<T>(string key, T value)
        {
            CheckIfInitialized();

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            using (var activity = _activitySource?.StartActivity("Put to cache.", ActivityKind.Producer))
            {
                byte[] blob = null;

                using (var serializerActivity = _activitySource?.StartActivity("Serialize.", ActivityKind.Producer))
                    blob = SerializerFactory.Serialize<T>(value);


                activity?.AddTag("cacheKey", key);
                DistributeCache?.Set(key, blob, DefaultOption);
            }

        }
        public async Task PutAsync<T>(string key, T value, CancellationToken cancellation = default(CancellationToken))
        {
            CheckIfInitialized();

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            using (var activity = _activitySource?.StartActivity("Put to cache.", ActivityKind.Producer))
            {
                byte[] blob = null;

                using (var serializerActivity = _activitySource?.StartActivity("Serialize.", ActivityKind.Producer))
                    blob = SerializerFactory.Serialize<T>(value);


                activity?.AddTag("cacheKey", key);

                await DistributeCache.SetAsync(key, blob, DefaultOption, cancellation);
            }
        }

        public void Put<T>(string key, TimeSpan timeout, T value, bool isSlided = false)
        {
            CheckIfInitialized();

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            byte[] blob = null;

            using (var activity = _activitySource?.StartActivity("Put to cache.", ActivityKind.Producer))
            {
                using (var serializerActivity = _activitySource?.StartActivity("Serialize.", ActivityKind.Producer))
                    blob = SerializerFactory.Serialize<T>(value);

                var dceo = new DistributedCacheEntryOptions();
                if (isSlided)
                    dceo.SetSlidingExpiration(timeout);
                else
                    dceo.SetAbsoluteExpiration(timeout);


                activity?.AddTag("cacheKey", key);

                DistributeCache?.Set(key, blob, dceo);
            }
        }

        public async Task PutAsync<T>(string key, TimeSpan timeout, T value, bool isSlided = false, CancellationToken cancellation = default(CancellationToken))
        {
            CheckIfInitialized();

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            byte[] blob = null;

            using (var activity = _activitySource?.StartActivity("Put to cache.", ActivityKind.Producer))
            {
                using (var serializerActivity = _activitySource?.StartActivity("Serialize.", ActivityKind.Producer))
                    blob = SerializerFactory.Serialize<T>(value);

                var dceo = new DistributedCacheEntryOptions();
                if (isSlided)
                    dceo.SetSlidingExpiration(timeout);
                else
                    dceo.SetAbsoluteExpiration(timeout);

                activity?.AddTag("cacheKey", key);


                await DistributeCache.SetAsync(key, blob, dceo, cancellation);
            }
        }


        public bool Remove(string key)
        {
            using (var activity = _activitySource?.StartActivity("Remove from cache.", ActivityKind.Producer))
            {
                activity?.AddTag("cacheKey", key);

                CheckIfInitialized();

                try
                {
                    DistributeCache.Remove(key);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

        }
        public async Task<bool> RemoveAsync(string key, CancellationToken cancellation = default(CancellationToken))
        {
            using (var activity = _activitySource?.StartActivity("Remove from cache.", ActivityKind.Producer))
            {
                activity?.AddTag("cacheKey", key);
                CheckIfInitialized();

                try
                {
                    await DistributeCache.RemoveAsync(key, cancellation);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }


        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            CheckIfInitialized();

            try
            {
                value = Get<TValue>(key);
                return value.Equals(default(TValue)) ? false : true;
            }
            catch (Exception)
            {
                value = default(TValue);
                return false;
            }

        }

        public async Task<TValue> TryGetValueAsync<TValue>(string key, CancellationToken cancellation = default(CancellationToken))
        {
            CheckIfInitialized();

            try
            {
                return await GetAsync<TValue>(key, cancellation);
            }
            catch (Exception)
            {
                return default(TValue);
            }
        }

        protected void CheckIfInitialized()
        {
            if (!IsInitialized) throw new CacheNotInitializedException();
        }
    }
}

