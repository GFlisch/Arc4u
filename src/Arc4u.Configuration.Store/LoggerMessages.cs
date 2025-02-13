
using Microsoft.Extensions.Logging;

namespace Arc4u.Configuration.Store;

public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 9070, Level = LogLevel.Information,
                   Message = "{ServiceName} service starting.")]
    public static partial void LogServiceStarting(this ILogger logger, string serviceName);

    [LoggerMessage(EventId = 9070, Level = LogLevel.Information,
                   Message = "{ServiceName} service started.")]
    public static partial void LogServiceStarted(this ILogger logger, string serviceName);

    [LoggerMessage(EventId = 9070, Level = LogLevel.Information,
               Message = "{ServiceName} service stopping.")]
    public static partial void LogServiceStopping(this ILogger logger, string serviceName);

    [LoggerMessage(EventId = 9070, Level = LogLevel.Information,
                   Message = "{ServiceName} service stopped.")]
    public static partial void LogServiceStopped(this ILogger logger, string serviceName);
}
