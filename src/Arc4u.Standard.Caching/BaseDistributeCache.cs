using System.Diagnostics;
using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.Serializer;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Arc4u.Caching;

public abstract class BaseDistributeCache<T> : ICache
{
    private bool disposed;

    protected DistributedCacheEntryOptions DefaultOption = new DistributedCacheEntryOptions();
    private IObjectSerialization? _serializerFactory;
    private readonly ILogger<T> _logger;

    protected IDistributedCache? DistributeCache { get; set; }

    protected readonly object _lock = new object();
    protected bool IsInitialized { get; set; }

    // The reason why the cache is not initialized, this will be used when an exception is thrown.
    protected string NotInitializedReason { get; set; } = string.Empty;

    protected readonly IContainerResolve _container;

    protected IContainerResolve Container => _container;
    private readonly ActivitySource? _activitySource;

    public BaseDistributeCache(ILogger<T> logger, IContainerResolve container)
    {
#if NETSTANDARD2_0
        if (null == container)
        {
            throw new ArgumentNullException(nameof(container));
        }
#else
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(container);
#endif

        _container = container;
        _logger = logger;

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
        {
            if (!disposing)
            {
                return;
            }
            if (DistributeCache is IDisposable disposable)
            {
                disposable.Dispose();
            }

            disposed = true;
        }
    }

    public TValue? Get<TValue>(string key)
    {
        CheckIfInitialized();

        try
        {
            byte[]? blob = [];
            using var activity = _activitySource?.StartActivity("Get from cache.", ActivityKind.Producer);
            activity?.SetTag("cacheKey", key);

            blob = DistributeCache?.Get(key);
            if (null == blob)
            {
                return default(TValue);
            }

            using var serializerActivity = _activitySource?.StartActivity("Deserialize.", ActivityKind.Producer);
            return SerializerFactory.Deserialize<TValue>(blob);
        }
        catch (Exception ex)
        {
            throw new DataCacheException(ex.Message);
        }
    }

    public async Task<TValue?> GetAsync<TValue>(string key, CancellationToken cancellation = default(CancellationToken))
    {
        CheckIfInitialized();

        try
        {
            byte[]? blob = null;
            using var activity = _activitySource?.StartActivity("Get from cache.", ActivityKind.Producer);
            activity?.SetTag("cacheKey", key);

            blob = DistributeCache is null ? null : await DistributeCache.GetAsync(key, cancellation).ConfigureAwait(false);
            if (null == blob)
            {
                return default(TValue);
            }

            using var serializerActivity = _activitySource?.StartActivity("Deserialize.", ActivityKind.Producer);
            return SerializerFactory.Deserialize<TValue>(blob);

        }
        catch (Exception ex)
        {
            throw new DataCacheException(ex.Message);
        }
    }

    public virtual void Initialize(string store) { }

    protected IObjectSerialization SerializerFactory
    {
        get
        {
            if (null == _serializerFactory)
            {
                _logger.Technical().LogError("A strong dependency on IObjectSerialization exists in Arc4u and distribute caching!");
            }

            return _serializerFactory ?? throw new NullReferenceException("No serializer exists!");
        }
        set => _serializerFactory = value;
    }

    public void Put<TValue>(string key, TValue value)
    {
        CheckIfInitialized();

#if NET8_0_OR_GREATER
        if (EqualityComparer<TValue>.Default.Equals(value, default(TValue)))
#else
        if (value == null)
#endif
        {
            throw new ArgumentNullException(nameof(value));
        }

        using var activity = _activitySource?.StartActivity("Put to cache.", ActivityKind.Producer);
        byte[] blob = [];

        using (var serializerActivity = _activitySource?.StartActivity("Serialize.", ActivityKind.Producer))
        {
            blob = SerializerFactory.Serialize<TValue>(value);
        }

        activity?.SetTag("cacheKey", key);
        DistributeCache?.Set(key, blob, DefaultOption);
    }

    public async Task PutAsync<TValue>(string key, TValue value, CancellationToken cancellation = default(CancellationToken))
    {
        CheckIfInitialized();

        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        using var activity = _activitySource?.StartActivity("Put to cache.", ActivityKind.Producer);
        byte[] blob = [];

        using (var serializerActivity = _activitySource?.StartActivity("Serialize.", ActivityKind.Producer))
        {
            blob = SerializerFactory.Serialize<TValue>(value);
        }

        activity?.SetTag("cacheKey", key);

        if (null != DistributeCache)
        {
            await DistributeCache.SetAsync(key, blob, DefaultOption, cancellation).ConfigureAwait(false);
        }
    }

    public void Put<TValue>(string key, TimeSpan timeout, TValue value, bool isSlided = false)
    {
        CheckIfInitialized();

        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        byte[] blob = [];

        using var activity = _activitySource?.StartActivity("Put to cache.", ActivityKind.Producer);
        using (var serializerActivity = _activitySource?.StartActivity("Serialize.", ActivityKind.Producer))
        {
            blob = SerializerFactory.Serialize<TValue>(value);
        }

        var dceo = new DistributedCacheEntryOptions();
        if (isSlided)
        {
            dceo.SetSlidingExpiration(timeout);
        }
        else
        {
            dceo.SetAbsoluteExpiration(timeout);
        }

        activity?.SetTag("cacheKey", key);

        DistributeCache?.Set(key, blob, dceo);
    }

    public async Task PutAsync<TValue>(string key, TimeSpan timeout, TValue value, bool isSlided = false, CancellationToken cancellation = default)
    {
        CheckIfInitialized();

        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        byte[] blob = [];

        using var activity = _activitySource?.StartActivity("Put to cache.", ActivityKind.Producer);

        using (var serializerActivity = _activitySource?.StartActivity("Serialize.", ActivityKind.Producer))
        {
            blob = SerializerFactory.Serialize<TValue>(value);
        }

        var dceo = new DistributedCacheEntryOptions();
        if (isSlided)
        {
            dceo.SetSlidingExpiration(timeout);
        }
        else
        {
            dceo.SetAbsoluteExpiration(timeout);
        }

        activity?.SetTag("cacheKey", key);

        if (null != DistributeCache)
        {
            await DistributeCache.SetAsync(key, blob, dceo, cancellation).ConfigureAwait(false);
        }
    }

    public bool Remove(string key)
    {
        using var activity = _activitySource?.StartActivity("Remove from cache.", ActivityKind.Producer);

        activity?.SetTag("cacheKey", key);

        CheckIfInitialized();

        try
        {
            DistributeCache?.Remove(key);
            return true;
        }
        catch (Exception)
        {
            return false;
        }

    }
    public async Task<bool> RemoveAsync(string key, CancellationToken cancellation = default(CancellationToken))
    {
        using var activity = _activitySource?.StartActivity("Remove from cache.", ActivityKind.Producer);

        activity?.SetTag("cacheKey", key);
        CheckIfInitialized();

        try
        {
            if (null != DistributeCache)
            {
                await DistributeCache.RemoveAsync(key, cancellation).ConfigureAwait(false);
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool TryGetValue<TValue>(string key, out TValue? value)
    {
        CheckIfInitialized();

        try
        {
            value = Get<TValue>(key);
            return null != value;
        }
        catch (Exception)
        {
            value = default(TValue);
            return false;
        }

    }

    public async Task<TValue?> TryGetValueAsync<TValue>(string key, CancellationToken cancellation = default(CancellationToken))
    {
        CheckIfInitialized();

        try
        {
            return await GetAsync<TValue>(key, cancellation).ConfigureAwait(false);
        }
        catch (Exception)
        {
            return default(TValue);
        }
    }

    protected void CheckIfInitialized()
    {
        if (!IsInitialized)
        {
            throw new CacheNotInitializedException(NotInitializedReason);
        }
    }

    public override string ToString()
    {
        return typeof(T).Name;
    }
}

