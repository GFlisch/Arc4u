namespace Arc4u.Caching;

/// <summary>
/// The interface define the contract implemented by a cache.
/// </summary>
public interface ICache : IDisposable
{
    /// <summary>
    /// Initialize the cache. This must be called once ervery time a cache is created.
    /// </summary>
    /// <param name="store">The store name for identify the cache used.</param>
    void Initialize(string store);

    /// <summary>
    /// Add or set a value in the cache.
    /// </summary>
    /// <param name="key">The key used to identify the <see cref="object"/> in the cache.</param>
    /// <param name="value">The object to save.</param>
    /// <returns>Return an <see cref="object"/> defining a version for the data added in the cache.</returns>
    /// <exception cref="DataCacheException">Thrown when the cache encounters a problem.</exception>
    void Put<T>(string key, T value);

    /// <summary>
    /// Add or set a value in the cache.
    /// </summary>
    /// <param name="key">The key used to identify the <see cref="object"/> in the cache.</param>
    /// <param name="value">The object to save.</param>
    /// <param name="cancellation">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>Return an <see cref="object"/> defining a version for the data added in the cache.</returns>
    /// <exception cref="DataCacheException">Thrown when the cache encounters a problem.</exception>
    Task PutAsync<T>(string key, T value, CancellationToken cancellation = default);

    /// <summary>
    /// Add or set a value in the cache.
    /// </summary>
    /// <param name="key">The key used to identify the <see cref="object"/> in the cache.</param>
    /// <param name="timeout">Define the period of validity for the data added to the cache. When the timeout is expired, the data is removed from the cache.</param>
    /// <param name="value">The object to save.</param>
    /// <param name="isSlided">A <see cref="bool"/> value indicating if the timeout period is reset again. If true, the data availability will be prolounge each time the data is accessed.</param>
    /// <returns>Return an <see cref="object"/> defining a version for the data added in the cache.</returns>
    /// <exception cref="DataCacheException">Thrown when the cache encounters a problem.</exception>
    void Put<T>(string key, TimeSpan timeout, T value, bool isSlided = false);

    /// <summary>
    /// Add or set a value in the cache.
    /// </summary>
    /// <param name="key">The key used to identify the <see cref="object"/> in the cache.</param>
    /// <param name="timeout">Define the period of validity for the data added to the cache. When the timeout is expired, the data is removed from the cache.</param>
    /// <param name="value">The object to save.</param>
    /// <param name="isSlided">A <see cref="bool"/> value indicating if the timeout period is reset again. If true, the data availability will be prolounge each time the data is accessed.</param>
    /// <param name="cancellation">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>Return an <see cref="object"/> defining a version for the data added in the cache.</returns>
    /// <exception cref="DataCacheException">Thrown when the cache encounters a problem.</exception>
    Task PutAsync<T>(string key, TimeSpan timeout, T value, bool isSlided = false, CancellationToken cancellation = default);

    /// <summary>
    /// retrieve an <see cref="object"/> from the cache and cast it to the type of TValue.
    /// </summary>
    /// <typeparam name="TValue">The type used to cast the <see cref="object"/> from the cache.</typeparam>
    /// <param name="key">The key used to identify the <see cref="object"/> in the cache.</param>
    /// <returns>An object typed to TValue, or the default value defined for the type TValue if no value exist for the specified key.</returns>
    /// <exception cref="DataCacheException">Thrown when the cache encounters a problem.</exception>
    /// <exception cref="InvalidCastException">Thrown when the <see cref="object"/> cannot be casted to the type TValue or when a cache has a problem.</exception>
    TValue? Get<TValue>(string key);

    /// <summary>
    /// retrieve an <see cref="object"/> from the cache and cast it to the type of TValue.
    /// </summary>
    /// <typeparam name="TValue">The type used to cast the <see cref="object"/> from the cache.</typeparam>
    /// <param name="key">The key used to identify the <see cref="object"/> in the cache.</param>
    /// <param name="cancellation"></param>
    /// <returns>An object typed to TValue, or the default value defined for the type TValue if no value exist for the specified key.</returns>
    /// <exception cref="DataCacheException">Thrown when the cache encounters a problem.</exception>
    /// <exception cref="InvalidCastException">Thrown when the <see cref="object"/> cannot be casted to the type TValue or when a cache has a problem.</exception>
    Task<TValue?> GetAsync<TValue>(string key, CancellationToken cancellation = default);

    /// <summary>
    /// Get a value from the cache, if a value exist for the specified key. Does not thrown an exception, if the cast to TValue is not possible.
    /// </summary>
    /// <typeparam name="TValue">The type expected from the cache.</typeparam>
    /// <param name="key">The key used to identify the <see cref="object"/> in the cache.</param>
    /// <param name="value">An object typed to TValue. The default value defined for the type TValue if no value exist for the specified key or if the cast is not possible.</param>
    /// <returns>An object typed to TValue, or the default value defined for the type TValue if no value exist for the specified key.</returns>
    bool TryGetValue<TValue>(string key, out TValue? value);

    /// <summary>
    /// Get a value from the cache, if a value exist for the specified key. Does not thrown an exception, if the cast to TValue is not possible.
    /// </summary>
    /// <typeparam name="TValue">The type expected from the cache.</typeparam>
    /// <param name="key">The key used to identify the <see cref="object"/> in the cache.</param>
    /// <param name="cancellation">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>An object typed to TValue. The default value defined for the type TValue if no value exist for the specified key or if the cast is not possible.</returns>
    Task<TValue?> TryGetValueAsync<TValue>(string key, CancellationToken cancellation = default);

    /// <summary>
    /// Rempve the data from the cache for the specified key.
    /// </summary>
    /// <param name="key">The key used to identify the <see cref="object"/> in the cache.</param>
    /// <returns>True if the data was removed.</returns>
    bool Remove(string key);

    /// <summary>
    /// Rempve the data from the cache for the specified key.
    /// </summary>
    /// <param name="key">The key used to identify the <see cref="object"/> in the cache.</param>
    /// <param name="cancellation">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>True if the data was removed.</returns>
    Task<bool> RemoveAsync(string key, CancellationToken cancellation = default);
}
