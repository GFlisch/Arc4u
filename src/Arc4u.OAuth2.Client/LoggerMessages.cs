
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.Client;
public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 9170, Level = LogLevel.Trace,
                   Message = "Null token provider is invoked.")]
    public static partial void LogCallNullTokenProvider(this ILogger logger);

    [LoggerMessage(EventId = 9171, Level = LogLevel.Trace,
               Message = "Null token provider signout is invoked.")]
    public static partial void LogCallSignOutNullTokenProvider(this ILogger logger);

}
