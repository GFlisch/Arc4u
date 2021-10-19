using Arc4u.Dependency.Attribute;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Arc4u.Caching
{
    [Export(typeof(ISecureCache)), Shared]
    public class SecureCache : ISecureCache
    {

        private Dictionary<string, object> _repository;

        public void Dispose()
        {
        }

        public TValue Get<TValue>(string key)
        {
            return (TValue)_repository[key];
        }

        public Task<TValue> GetAsync<TValue>(string key, CancellationToken cancellation = default)
        {
            return Task.FromResult((TValue)_repository[key]);
        }

        public void Initialize(string store)
        {
            _repository = new Dictionary<string, object>();
        }

        public void Put<T>(string key, T value)
        {
            _repository.TryAdd(key, value);
        }

        public void Put<T>(string key, TimeSpan timeout, T value, bool isSlided = false)
        {
            _repository.TryAdd(key, value);
        }

        public Task PutAsync<T>(string key, T value, CancellationToken cancellation = default)
        {
            _repository.TryAdd(key, value);

            return Task.CompletedTask;
        }

        public Task PutAsync<T>(string key, TimeSpan timeout, T value, bool isSlided = false, CancellationToken cancellation = default)
        {
            _repository.TryAdd(key, value);

            return Task.CompletedTask;
        }

        public bool Remove(string key)
        {
            return _repository.Remove(key);
        }

        public Task<bool> RemoveAsync(string key, CancellationToken cancellation = default)
        {
            return Task.FromResult(_repository.Remove(key));
        }

        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            var result = _repository.TryGetValue(key, out var objectValue);

            value = (TValue)objectValue;

            return result;
        }

        public Task<TValue> TryGetValueAsync<TValue>(string key, CancellationToken cancellation = default)
        {
            var result = _repository.TryGetValue(key, out var objectValue);

            return Task.FromResult((TValue)objectValue);
        }
    }
}
