
using Microsoft.Extensions.Logging;

namespace Arc4u.Caching;
public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 9010, Level = LogLevel.Trace,
                   Message = "Instantiate a new instance of a cache with a kind of {cacheKind} and named {cacheName}.")]
    public static partial void LogNewCache(this ILogger logger, string cacheKind, string cacheName);

    [LoggerMessage(EventId = 9011, Level = LogLevel.Information,
                  Message = "Register a cache with a kind of {CacheKind} and named {CacheName} for later.")]
    public static partial void LogRegisterNewCache(this ILogger logger, string cacheKind, string cacheName);
}
