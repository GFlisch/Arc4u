using Arc4u.Security;
using System;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Arc4u.Caching
{
    [Export(typeof(ISecureCache)), Shared]
    public class KeyChainCache : ISecureCache
    {
        bool isInitialised;
        SafeStore secureStore;
        const string DefaultStoreName = "Arc4u.Secure.Store";

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
                    secureStore = null;
                    disposed = true;
                }
        }

        public TValue Get<TValue>(string key)
        {
            if (!isInitialised) InternalInitialize();

            return secureStore.Get<TValue>(key);
        }

        public void Initialize(string store)
        {
            InternalInitialize(store);
        }

        void InternalInitialize(string store = "")
        {
            lock (this)
            {
                if (!isInitialised)
                {
                    var storeName = String.IsNullOrWhiteSpace(store) ? DefaultStoreName : store;
                    secureStore = new SafeStore(ApplicationCtx.Context, ApplicationCtx.StorePassword, storeName);
                    isInitialised = true;
                }
            }

        }

        public void Put<T>(string key, T value)
        {
            if (!isInitialised) InternalInitialize();

            secureStore.Save(key, value);
        }

        public void Put<T>(string key, TimeSpan timeout, T value, bool isSlided = false)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            if (!isInitialised) InternalInitialize();

            try
            {
                secureStore.Delete(key);

                return true;
            }
            catch (Exception) { }

            return false;
        }

        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            try
            {
                value = Get<TValue>(key);
                return true;
            }
            catch (Exception)
            {
                value = default;
                return false;
            }
        }

        public Task PutAsync<T>(string key, T value, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public Task PutAsync<T>(string key, TimeSpan timeout, T value, bool isSlided = false, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public Task<TValue> GetAsync<TValue>(string key, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public Task<TValue> TryGetValueAsync<TValue>(string key, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveAsync(string key, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }
    }
}