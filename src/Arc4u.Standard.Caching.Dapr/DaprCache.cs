using Arc4u.Caching;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Dapr.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Arc4u.Caching.Dapr
{
    [Export("Dapr", typeof(ICache))]
    public class DaprCache : ICache
    {
        public DaprCache(ILogger<DaprCache> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<DaprCache> _logger;
        private DaprClient _daprClient;
        private string _storeName;

        public void Dispose()
        {
            _daprClient?.Dispose();
        }

        public TValue Get<TValue>(string key)
        {
            return _daprClient.GetStateAsync<TValue>(_storeName, key).GetAwaiter().GetResult();
        }

        public async Task<TValue> GetAsync<TValue>(string key, CancellationToken cancellation = default)
        {
            return await _daprClient?.GetStateAsync<TValue>(_storeName, key, cancellationToken: cancellation);
        }

        public void Initialize(string store)
        {
            lock (_logger)
            {
                if (null != _daprClient)
                {
                    _logger.Technical().Information($"Dapr caching for dapr state store {store} is already initialized.").Log();
                }
                else
                {
                    _storeName = store;
                    _daprClient = new DaprClientBuilder().Build();
                    _logger.Technical().Information($"Dapr caching for dapr state store {store} is initialized.").Log();
                }
            }

        }

        public void Put<T>(string key, T value)
        {
            _daprClient?.SaveStateAsync(_storeName, key, value).GetAwaiter().GetResult();
        }

        public void Put<T>(string key, TimeSpan timeout, T value, bool isSlided = false)
        {
            if (isSlided) throw new NotSupportedException("Sliding is not supported in Dapr State.");

            _daprClient?.SaveStateAsync(_storeName, key, value, metadata: new Dictionary<string, string> { { "ttlInSeconds", timeout.TotalSeconds.ToString() } }).GetAwaiter().GetResult();
        }

        public async Task PutAsync<T>(string key, T value, CancellationToken cancellation = default)
        {
            await _daprClient?.SaveStateAsync(_storeName, key, value);
        }

        public async Task PutAsync<T>(string key, TimeSpan timeout, T value, bool isSlided = false, CancellationToken cancellation = default)
        {
            if (isSlided) throw new NotSupportedException("Sliding is not supported in Dapr State.");

            await _daprClient?.SaveStateAsync(_storeName, key, value, metadata: new Dictionary<string, string> { { "ttlInSeconds", timeout.TotalSeconds.ToString() } });
        }

        public bool Remove(string key)
        {
            try
            {
                _daprClient.DeleteStateAsync(_storeName, key).GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> RemoveAsync(string key, CancellationToken cancellation = default)
        {
            try
            {
                await _daprClient.DeleteStateAsync(_storeName, key, cancellationToken: cancellation);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
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

        public async Task<TValue> TryGetValueAsync<TValue>(string key, CancellationToken cancellation = default)
        {
            try
            {
                return await GetAsync<TValue>(key, cancellation);
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}
