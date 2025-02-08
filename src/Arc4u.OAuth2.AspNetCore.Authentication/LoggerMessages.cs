
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2;
public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 9120, Level = LogLevel.Error,
               Message = "No settings or application context is defined with {ResolvingName}.")]
    public static partial void LogResolvingIssueSettingsByName(this ILogger logger, string resolvingName);

    [LoggerMessage(EventId = 9121, Level = LogLevel.Trace,
               Message = "{DelegateName} JwtHttpHandler is called.")]
    public static partial void LogHttpHandlerIsCalled(this ILogger logger, string delegateName);

    [LoggerMessage(EventId = 9123, Level = LogLevel.Trace,
       Message = "No authentication type for {DelegateName}, Check next Delegate Handler.")]
    public static partial void LogNoAuthenticationTypeCallNextHttpHandler(this ILogger logger, string delegateName);

    [LoggerMessage(EventId = 9124, Level = LogLevel.Trace,
    Message = "Different authentication type for {DelegateName}, Check next Delegate Handler.")]
    public static partial void LogDifferentAuthenticationTypeCallNextHttpHandler(this ILogger logger, string delegateName);

    [LoggerMessage(EventId = 9125, Level = LogLevel.Trace,
    Message = "An authorization header already exist for handler {DelegateName}, Check next Delegate Handler.")]
    public static partial void LogAlreadyHasAnAuthorizationHeaderCallNextHttpHandler(this ILogger logger, string delegateName);

    [LoggerMessage(EventId = 9126, Level = LogLevel.Trace,
        Message = "{DelegateName}, Get the authentication Token Provider.")]
    public static partial void LogGetTheTokenProvider(this ILogger logger, string delegateName);

    [LoggerMessage(EventId = 9127, Level = LogLevel.Trace,
        Message = "{DelegateName}, No authentication Token Provider.")]
    public static partial void LogNoTokenProvider(this ILogger logger, string delegateName);

    [LoggerMessage(EventId = 9128, Level = LogLevel.Trace,
        Message = "Requesting an authentication token.")]
    public static partial void LogRequestingToken(this ILogger logger);

    [LoggerMessage(EventId = 9129, Level = LogLevel.Trace,
        Message = "Token is expired! Next Hanlder will be called.")]
    public static partial void LogTokenIsExpired(this ILogger logger);

    [LoggerMessage(EventId = 9130, Level = LogLevel.Trace,
        Message = "Remove any Bearer token attached.")]
    public static partial void LogRemoveAnyBearer(this ILogger logger);

    [LoggerMessage(EventId = 9131, Level = LogLevel.Trace,
    Message = "Add the {Scheme} token to provide authentication evidence.")]
    public static partial void LogAddSchentoToken(this ILogger logger, string scheme);

    [LoggerMessage(EventId = 9131, Level = LogLevel.Trace,
Message = "Add the current culture to the request: {Culture}")]
    public static partial void LogUseCurrentCulture(this ILogger logger, string culture);

}
