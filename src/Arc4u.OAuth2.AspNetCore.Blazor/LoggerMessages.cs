using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.AspNetCore.Blazor;
public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 9160, Level = LogLevel.Warning,
                   Message = "No access token can be retrieved for the current user!")]
    public static partial void LogNoAccessToken(this ILogger logger);

    [LoggerMessage(EventId = 9161, Level = LogLevel.Warning,
                   Message = "The user is not identified!")]
    public static partial void LogUserNotIdentified(this ILogger logger);

    [LoggerMessage(EventId = 9162, Level = LogLevel.Error,
               Message = "Token provider for {ProviderId} is null")]
    public static partial void LogTokenProviderIsNull(this ILogger logger, string providerId);
}
