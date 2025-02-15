
using Microsoft.Extensions.Logging;

namespace Arc4u.Caching.Redis;
public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 9020, Level = LogLevel.Warning,
                   Message = "Redis Cache {store} is already initialized.")]
    public static partial void LogCacheIsAlreadyInitialized(this ILogger logger, string store);

    [LoggerMessage(EventId = 9021, Level = LogLevel.Information,
                  Message = "Redis caching for dapr state store {store} is initialized.")]
    public static partial void LogCacheIsInitialized(this ILogger logger, string store);
}
