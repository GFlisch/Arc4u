
using Microsoft.Extensions.Logging;

namespace Arc4u.Caching.Memory;
public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 9020, Level = LogLevel.Warning,
                   Message = "Memory Cache {store} is already initialized.")]
    public static partial void LogCacheIsAlreadyInitialized(this ILogger logger, string store);

    [LoggerMessage(EventId = 9021, Level = LogLevel.Information,
                  Message = "Memory caching for state store {store} is initialized.")]
    public static partial void LogCacheIsInitialized(this ILogger logger, string store);

    [LoggerMessage(EventId = 9022, Level = LogLevel.Warning,
               Message = "The size limit for the {Store} cache is {SizeLimitInMB}.")]
    public static partial void LogCacheSizeLimit(this ILogger logger, string store, long sizeLimitInMB);
}
