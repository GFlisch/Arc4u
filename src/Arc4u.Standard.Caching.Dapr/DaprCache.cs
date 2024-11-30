using System.Globalization;
using Arc4u.Configuration.Dapr;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Dapr.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.Caching.Dapr;

[Export("Dapr", typeof(ICache))]
public sealed class DaprCache : ICache
{
    public DaprCache(ILogger<DaprCache> logger, IOptionsMonitor<DaprCacheOption> options)
    {
        _logger = logger;
        _options = options;
    }

    private readonly ILogger<DaprCache> _logger;
    private readonly IOptionsMonitor<DaprCacheOption> _options;

    private DaprClient? _daprClient;
    private string _storeName = string.Empty;

    public void Dispose()
    {
        _daprClient?.Dispose();
    }

    public TValue? Get<TValue>(string key)
    {
        if (_daprClient is null)
        {
            throw new CacheNotInitializedException();
        }

        return _daprClient.GetStateAsync<TValue>(_storeName, key).GetAwaiter().GetResult();
    }

    public async Task<TValue?> GetAsync<TValue>(string key, CancellationToken cancellation = default)
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
                if (string.IsNullOrEmpty(store))
                {
                    NotInitializedReason = "When initializing the Dapr cache, the value of the store cannot be an empty string.";
                    throw new ArgumentException(NotInitializedReason, nameof(store));
                }

                try
                {
                    var config = _options.Get(store);

                    _storeName = config.Name ?? throw new NullReferenceException("There is no name defined in the configuration for the Dapr section!");
                    _daprClient = new DaprClientBuilder().Build();
                    _logger.Technical().Information($"Dapr caching for dapr state store {store} is initialized.").Log();
                }
                catch (Exception ex)
                {
                    NotInitializedReason = $"Dapr Cache {store} is not initialized. With exception: {ex.Message}";

                    throw;
                }

            }
        }
    }

    public void Put<T>(string key, T value)
    {
        CheckIfInitialized();

        _daprClient!.SaveStateAsync(_storeName, key, value).GetAwaiter().GetResult();
    }

    public void Put<T>(string key, TimeSpan timeout, T value, bool isSlided = false)
    {
        CheckIfInitialized();

        if (isSlided)
        {
            throw new NotSupportedException("Sliding is not supported in Dapr State.");
        }

        _daprClient!.SaveStateAsync(_storeName, key, value, metadata: new Dictionary<string, string> { { "ttlInSeconds", timeout.TotalSeconds.ToString(CultureInfo.InvariantCulture) } }).GetAwaiter().GetResult();
    }

    public async Task PutAsync<T>(string key, T value, CancellationToken cancellation = default)
    {
        CheckIfInitialized();

        await _daprClient!.SaveStateAsync(_storeName, key, value, cancellationToken: cancellation).ConfigureAwait(false);
    }

    public async Task PutAsync<T>(string key, TimeSpan timeout, T value, bool isSlided = false, CancellationToken cancellation = default)
    {
        CheckIfInitialized();

        if (isSlided)
        {
            throw new NotSupportedException("Sliding is not supported in Dapr State.");
        }

        await _daprClient!.SaveStateAsync(_storeName, key, value, metadata: new Dictionary<string, string> { { "ttlInSeconds", timeout.TotalSeconds.ToString(CultureInfo.InvariantCulture) } }, cancellationToken: cancellation).ConfigureAwait(false);
    }

    public bool Remove(string key)
    {
        CheckIfInitialized();

        try
        {
            _daprClient!.DeleteStateAsync(_storeName, key).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.Technical().LogException(ex);
            return false;
        }

        return true;
    }

    public async Task<bool> RemoveAsync(string key, CancellationToken cancellation = default)
    {
        CheckIfInitialized();

        try
        {
            await _daprClient!.DeleteStateAsync(_storeName, key, cancellationToken: cancellation).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.Technical().LogException(ex);
            return false;
        }

        return true;
    }

    public bool TryGetValue<TValue>(string key, out TValue? value)
    {
        CheckIfInitialized();

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

    public async Task<TValue?> TryGetValueAsync<TValue>(string key, CancellationToken cancellation = default)
    {
        CheckIfInitialized();

        try
        {
            return await GetAsync<TValue>(key, cancellation).ConfigureAwait(false);
        }
        catch (Exception)
        {
            return default;
        }
    }

    // The reason why the cache is not initialized, this will be used when an exception is thrown.
    string NotInitializedReason { get; set; } = string.Empty;

    void CheckIfInitialized()
    {
        if (_daprClient is null)
        {
            throw new CacheNotInitializedException(NotInitializedReason);
        }
    }

    public override string ToString() => _storeName ?? throw new InvalidOperationException("The Store name parameter must not be null.");
}
