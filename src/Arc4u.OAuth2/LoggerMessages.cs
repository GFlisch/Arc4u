
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2;
public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 9050, Level = LogLevel.Trace,
                   Message = "The token cache is {cacheName}.")]
    public static partial void LogTokenCacheName(this ILogger logger, string cacheName);

    [LoggerMessage(EventId = 9051, Level = LogLevel.Trace,
                   Message = "The token cache is using the default cache defined in the config.")]
    public static partial void LogDefaultTokenCache(this ILogger logger);

    [LoggerMessage(EventId = 9052, Level = LogLevel.Trace,
               Message = "Requesting an authentication token.")]
    public static partial void LogRequestingAuthenticationToken(this ILogger logger);

    [LoggerMessage(EventId = 9053, Level = LogLevel.Error,
           Message = "Requesting an authentication token.")]
    public static partial void LogNoToken(this ILogger logger);

    [LoggerMessage(EventId = 9054, Level = LogLevel.Trace,
           Message = "Skip fetching claims, no setting found for authentication type {AuthenticationType}.")]
    public static partial void LogSkipFetchingClaims(this ILogger logger, string authenticationType);

    [LoggerMessage(EventId = 9055, Level = LogLevel.Error,
           Message = "Token endpoint for {Upn} returned {ResponseStatusCode}: {LoggedResponseBody}.")]
    public static partial void LogCredentialToken(this ILogger logger, string upn, string responseStatusCode, string loggedResponseBody);

    [LoggerMessage(EventId = 9056, Level = LogLevel.Debug,
       Message = "Creating an authentication context for the request.")]
    public static partial void LogCreatingAuthenticationContext(this ILogger logger);

    [LoggerMessage(EventId = 9057, Level = LogLevel.Debug,
       Message = "Call STS: {Authority} for user: {Upn}.")]
    public static partial void LogStsAndUser(this ILogger logger, string authority, string upn);

    [LoggerMessage(EventId = 9058, Level = LogLevel.Debug,
       Message = "Get token endpoint")]
    public static partial void LogGetEndpoint(this ILogger logger);
}
