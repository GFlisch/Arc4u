using System;
using System.Composition;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace Arc4u.Caching
{
    [Export(typeof(ISecureCache)), Shared]
    public class PasswordVaultCache : ISecureCache
    {
        private PasswordVault _vault;

        public PasswordVaultCache()
        {
            _vault = new PasswordVault();
        }

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
                    disposed = true;
                }
        }

        public TValue Get<TValue>(string key)
        {
            var values = _vault.FindAllByResource(key);

            if (values.Any())
            {
                var json = _vault.Retrieve(key, values.First().UserName).Password;

                if (typeof(TValue) == typeof(String))
                    return (TValue)Convert.ChangeType(json, typeof(TValue));

                var serializer = new DataContractJsonSerializer(typeof(TValue));

                return serializer.ReadObject<TValue>(json);
            }
            else return default(TValue);
        }

        public Task<TValue> GetAsync<TValue>(string key, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public void Initialize(string store)
        {
            throw new AppException("No store can be defined.");
        }

        public void Put<T>(string key, T value)
        {
            string json;

            try
            {

                if (value is String)
                {
                    json = value.ToString();
                }
                else
                {
                    var serializer = new DataContractJsonSerializer(value.GetType());
                    serializer.WriteObject(value, out json);
                }

                try
                {
                    var values = _vault.FindAllByResource(key);

                    if (null != values && values.Count > 0)
                    {
                        // remove any other references.
                        if (values.Count > 1)
                            foreach (var credential in values) _vault.Remove(credential);
                        else // check if we have a change?
                        {
                            if (_vault.Retrieve(key, values.First().UserName).Password.Equals(json, StringComparison.CurrentCultureIgnoreCase))
                                return; // we have already the same information stored.
                            else _vault.Remove(values[0]);
                        }
                    }


                }
                catch
                {

                }

                _vault.Add(new PasswordCredential(key, "data", json));
            }

            catch { }
        }

        public void Put<T>(string key, TimeSpan timeout, T value, bool isSlided = false)
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
            try
            {
                var values = _vault.FindAllByResource(key);

                if (null != values && values.Any())
                {
                    foreach (var credential in values) _vault.Remove(credential);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
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
