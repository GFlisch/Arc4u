
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.AspNetCore;
public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 9080, Level = LogLevel.Information,
                   Message = "Time to complete method call")]
    public static partial void TimeToCompleteMethodCall(this ILogger logger);

    [LoggerMessage(EventId = 9081, Level = LogLevel.Debug,
               Message = "Thread UI Culture is set to {UICultureName}")]
    public static partial void LogThreadCultureName(this ILogger logger, string uICultureName);

}
