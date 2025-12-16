
using Microsoft.Extensions.Logging;

namespace Arc4u.Caching.Redis;
public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 9040, Level = LogLevel.Warning,
                   Message = "Redis Cache {store} is already initialized.")]
    public static partial void LogCacheIsAlreadyInitialized(this ILogger logger, string store);

    [LoggerMessage(EventId = 9041, Level = LogLevel.Information,
                  Message = "Redis caching {store} is initialized.")]
    public static partial void LogCacheIsInitialized(this ILogger logger, string store);
}
