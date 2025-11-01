
using Microsoft.Extensions.Logging;

namespace Arc4u.Caching.Sql;
public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 9050, Level = LogLevel.Warning,
                   Message = "Sql Cache {store} is already initialized.")]
    public static partial void LogCacheIsAlreadyInitialized(this ILogger logger, string store);

    [LoggerMessage(EventId = 9051, Level = LogLevel.Information,
                  Message = "Sql caching for dapr state store {store} is initialized.")]
    public static partial void LogCacheIsInitialized(this ILogger logger, string store);
}
