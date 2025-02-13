
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

    [LoggerMessage(EventId = 9059, Level = LogLevel.Debug,
               Message = "Token is received for user {UserUpn}.")]
    public static partial void LogTokenReceived(this ILogger logger, string userUpn);

    [LoggerMessage(EventId = 9060, Level = LogLevel.Debug,
           Message = "Access token will expire at {TokenExpirationDateUtc} utc.")]
    public static partial void LogTokenExpiration(this ILogger logger, DateTime tokenExpirationDateUtc);

    [LoggerMessage(EventId = 9061, Level = LogLevel.Warning,
        Message = "Geting all data from the token cache is not implemented.")]
    public static partial void LogTokenCacheNotImplemented(this ILogger logger);

    [LoggerMessage(EventId = 9062, Level = LogLevel.Trace,
        Message = "Deleting information from the token cache for the id: {TokenKey}.")]
    public static partial void LogDeleteInTokenCache(this ILogger logger, string tokenKey);

    [LoggerMessage(EventId = 9062, Level = LogLevel.Trace,
        Message = "Deleted information from the token cache for the id: {TokenKey}.")]
    public static partial void LogDeletedInTokenCache(this ILogger logger, string tokenKey);

    [LoggerMessage(EventId = 9063, Level = LogLevel.Warning,
        Message = "A null token data information was provided to the cache, with an id: {TokenKey}.")]
    public static partial void LogNullTokenData(this ILogger logger, string tokenKey);

    [LoggerMessage(EventId = 9064, Level = LogLevel.Trace,
    Message = "Adding token data information to the cache with id: {TokenKey}.")]
    public static partial void LogAddingInTokenCache(this ILogger logger, string tokenKey);

    [LoggerMessage(EventId = 9064, Level = LogLevel.Trace,
        Message = "Added token data information to the cache with id: {TokenKey}.")]
    public static partial void LogAddedInTokenCache(this ILogger logger, string tokenKey);

    [LoggerMessage(EventId = 9065, Level = LogLevel.Trace,
        Message = "Retrieve token information for user: {TokenKey}.")]
    public static partial void LogGetDataTokenCache(this ILogger logger, string tokenKey);

    [LoggerMessage(EventId = 9066, Level = LogLevel.Warning,
        Message = "The data in cache is null for user: {TokenKey}.")]
    public static partial void LogGetNullDataTokenCache(this ILogger logger, string tokenKey);

}
