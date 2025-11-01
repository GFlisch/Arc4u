using System.Globalization;
using System.Text.Json;
using Arc4u.Caching;
using Arc4u.Configuration;
using Arc4u.Dependency.Attribute;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Client.Authentication.Cache;

[Export(typeof(ISecureCache)), Shared]
public sealed class CacheTokens : ISecureCache
{
    public CacheTokens(IOptions<ApplicationConfig> config)
    {
        // Create a cache file from the application name in the config file + environment.
        var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "OAuth2");

        _cacheFilePath = Path.Combine(path, string.Format(CultureInfo.CurrentCulture, "{0}_{1}_", config.Value.Environment.LoggingName, config.Value.Environment.Name));

        if (Directory.Exists(path) == false)
        {
            Directory.CreateDirectory(path);
        }
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

#if NET9_0_OR_GREATER
        static readonly Lock FileLock = new();
#else
    static readonly object FileLock = new();
#endif
    private readonly string _cacheFilePath;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private bool _disposed;

    public void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _disposed = true;
        }
    }

    public TValue Get<TValue>(string key)
    {
        var path = ComputePath(key);

        if (!File.Exists(path))
        {
            return default!;
        }

        lock (FileLock)
        {
            var result = JsonSerializer.Deserialize<TValue>(File.ReadAllText(path));

            return result ?? throw new KeyNotFoundException();
        }
    }

    private string ComputePath(string key)
    {
        return $"{_cacheFilePath}{key.Trim()}.json";
    }

    public void Initialize(string store)
    {
    }

    public bool Remove(string key)
    {
        try
        {
            lock (FileLock)
            {
                var path = ComputePath(key);

                File.Delete(path);

                return true;
            }
        }
        catch (Exception)
        {
            return false;
        }

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
            value = default!;
            return false;
        }
    }

    public void Put<T>(string key, T value)
    {
        var path = ComputePath(key);

        var json = JsonSerializer.Serialize(value, _jsonSerializerOptions);

        File.WriteAllText(path, json);
    }

    public async Task PutAsync<T>(string key, T value, CancellationToken cancellation = default)
    {
        var path = ComputePath(key);

        var json = JsonSerializer.Serialize(value, _jsonSerializerOptions);

        await File.WriteAllTextAsync(path, json, cancellation).ConfigureAwait(true);
    }

    public void Put<T>(string key, TimeSpan timeout, T value, bool isSlided = false)
    {
        throw new NotImplementedException();
    }

    public Task PutAsync<T>(string key, TimeSpan timeout, T value, bool isSlided = false, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public async Task<TValue?> GetAsync<TValue>(string key, CancellationToken cancellation = default)
    {
        var path = ComputePath(key);

        if (!File.Exists(path))
        {
            return default;
        }

        // Lock to avoid concurrent access to the same file.
        // Do not perform async I/O inside the lock: copy bytes synchronously, then deserialize asynchronously.
        byte[] bytes;
        lock (FileLock)
        {
#pragma warning disable CA1849
            bytes = File.ReadAllBytes(path);
#pragma warning restore CA1849
        }

        using var ms = new MemoryStream(bytes, writable: false);
        var result = await JsonSerializer.DeserializeAsync<TValue>(ms, _jsonSerializerOptions, cancellation).ConfigureAwait(false);

        return result ?? throw new KeyNotFoundException();
    }

    public Task<TValue?> TryGetValueAsync<TValue>(string key, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RemoveAsync(string key, CancellationToken cancellation = default)
    {
        return Task.FromResult(Remove(key));
    }
}
