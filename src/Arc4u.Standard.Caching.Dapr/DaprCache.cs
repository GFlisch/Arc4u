using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Dapr.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Arc4u.Caching.Dapr;

[Export("Dapr", typeof(ICache))]
public class DaprCache : ICache
{
    public DaprCache(ILogger<DaprCache> logger)
    {
        _logger = logger;
    }

    private readonly ILogger<DaprCache> _logger;
    private DaprClient? _daprClient;
    private string _storeName;

    public void Dispose()
    {
        _daprClient?.Dispose();
    }

    public TValue Get<TValue>(string key)
    {
        if (_daprClient is null)
        {
            throw new CacheNotInitializedException();
        }

        return _daprClient.GetStateAsync<TValue>(_storeName, key).GetAwaiter().GetResult();
    }

    public async Task<TValue> GetAsync<TValue>(string key, CancellationToken cancellation = default)
    {
        if (_daprClient is null)
        {
            throw new CacheNotInitializedException();
        }

        return await _daprClient.GetStateAsync<TValue>(_storeName, key, cancellationToken: cancellation).ConfigureAwait(false);
    }

    public void Initialize(string store)
    {
        lock (_logger)
        {
            if (_daprClient is not null)
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
        if (_daprClient is null)
        {
            throw new CacheNotInitializedException();
        }

        _daprClient.SaveStateAsync(_storeName, key, value).GetAwaiter().GetResult();
    }

    public void Put<T>(string key, TimeSpan timeout, T value, bool isSlided = false)
    {
        if (_daprClient is null)
        {
            throw new CacheNotInitializedException();
        }

        if (isSlided)
        {
            throw new NotSupportedException("Sliding is not supported in Dapr State.");
        }

        _daprClient.SaveStateAsync(_storeName, key, value, metadata: new Dictionary<string, string> { { "ttlInSeconds", timeout.TotalSeconds.ToString() } }).GetAwaiter().GetResult();
    }

    public async Task PutAsync<T>(string key, T value, CancellationToken cancellation = default)
    {
        if (_daprClient is null)
        {
            throw new CacheNotInitializedException();
        }

        await _daprClient.SaveStateAsync(_storeName, key, value, cancellationToken: cancellation).ConfigureAwait(false);
    }

    public async Task PutAsync<T>(string key, TimeSpan timeout, T value, bool isSlided = false, CancellationToken cancellation = default)
    {
        if (isSlided)
        {
            throw new NotSupportedException("Sliding is not supported in Dapr State.");
        }
        if (_daprClient is null)
        {
            throw new CacheNotInitializedException();
        }

        await _daprClient.SaveStateAsync(_storeName, key, value, metadata: new Dictionary<string, string> { { "ttlInSeconds", timeout.TotalSeconds.ToString(CultureInfo.InvariantCulture) } }, cancellationToken: cancellation).ConfigureAwait(false);
    }

    public bool Remove(string key)
    {
        if (_daprClient is null)
        {
            return false;
        }

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
            if (_daprClient is not null)
            {
                await _daprClient.DeleteStateAsync(_storeName, key, cancellationToken: cancellation).ConfigureAwait(false);
            }
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
            return await GetAsync<TValue>(key, cancellation).ConfigureAwait(false);
        }
        catch (Exception)
        {
            return default;
        }
    }
}
