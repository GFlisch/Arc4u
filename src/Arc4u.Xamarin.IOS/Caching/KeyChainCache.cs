using Arc4u.Security;
using System;
using System.Composition;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Arc4u.Caching
{
    [Export(typeof(ISecureCache)), Shared]
    public class KeyChainCache : ISecureCache
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public TValue Get<TValue>(string key)
        {
            var value = Record.GetValue(key);

            if (typeof(TValue) == typeof(String))
                return (TValue)Convert.ChangeType(value, typeof(TValue));

            var serializer = new DataContractJsonSerializer(typeof(TValue));

            return serializer.ReadObject<TValue>(value);

        }


        public Task<TValue> GetAsync<TValue>(string key, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public void Initialize(string store)
        {
            throw new ApplicationException("No store can be defined.");
        }

        public void Put<T>(string key, T value)
        {
            if (value is String)
            {
                Record.SetValue(key, value.ToString());

            }
            else
            {
                var serializer = new DataContractJsonSerializer(value.GetType());
                serializer.WriteObject(value, out var result);
                Record.SetValue(key, result);
            }
        }

        public void Put<T>(string key, TimeSpan timeout, T value, bool isSlided = false)
        {
            throw new NotImplementedException();
        }

        public object Put<T>(string key, T value, object version)
        {
            throw new NotImplementedException();
        }

        public Task PutAsync<T>(string key, T value, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public Task PutAsync<T>(string key, TimeSpan timeout, T value, bool isSlided = false, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            var result = Record.Remove(key);

            return result;
        }

        public Task<bool> RemoveAsync(string key, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
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
                value = default(TValue);
                return false;
            }
        }

        public Task<TValue> TryGetValueAsync<TValue>(string key, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }
    }
}
