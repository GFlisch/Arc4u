using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.AspNetCore.Blazor;
public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 9220, Level = LogLevel.Warning,
                   Message = "No access token can be retrieved for the current user!")]
    public static partial void LogNoAccessToken(this ILogger logger);

    [LoggerMessage(EventId = 9220, Level = LogLevel.Warning,
                   Message = "The user is not identified!")]
    public static partial void LogUserNotIdentified(this ILogger logger);

}
