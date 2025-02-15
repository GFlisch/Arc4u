using Microsoft.Extensions.Logging;

namespace Arc4u.Diagnostics.Monitoring;
public static partial class MonitoringMessages
{
    [LoggerMessage(EventId = 9001,
                   Level = LogLevel.Information,
                   Message = "Cpu & Memory",
                   SkipEnabledCheck = true)]
    public static partial void LogMonitoring(this ILogger logger);

    [LoggerMessage(EventId = 9002,
                   Level = LogLevel.Information,
                   Message = "Time to complete method call",
                   SkipEnabledCheck = true)]
    public static partial void LogTimeToCompleteCall(this ILogger logger);
}
