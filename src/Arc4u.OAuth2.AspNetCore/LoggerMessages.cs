
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

    [LoggerMessage(EventId = 9082, Level = LogLevel.Debug,
                   Message = "Username receives is: {UserName}.")]
    public static partial void LogUserName(this ILogger logger, string userName);

    [LoggerMessage(EventId = 9083, Level = LogLevel.Debug,
                   Message = "Change user name receivde from {UserName} to {NewUserName}.")]
    public static partial void LogChangeUserName(this ILogger logger, string userName, string newUserName);

    [LoggerMessage(EventId = 9084, Level = LogLevel.Debug,
                   Message = "Force an OpenId connection.")]
    public static partial void LogForceOpenIdConnect(this ILogger logger);

    [LoggerMessage(EventId = 9084, Level = LogLevel.Error,
                   Message = "No claim type found equal to: {IdentifierOptions} in the current identity.")]
    public static partial void LogNoClaimTypeFound(this ILogger logger, string identifierOptions);

    [LoggerMessage(EventId = 9085, Level = LogLevel.Debug,
                   Message = "Claim Type id used to identify the user is {ClaimTypeId}.")]
    public static partial void LogClaimTypeIdFound(this ILogger logger, string claimTypeId);

    [LoggerMessage(EventId = 9086, Level = LogLevel.Warning,
                   Message = "Loading extra claims needs an identity!")]
    public static partial void LogNoIdentity(this ILogger logger);

    [LoggerMessage(EventId = 9087, Level = LogLevel.Error,
               Message = "Basic authentication is not well formed!")]
    public static partial void LogBasicAuthentityBadFormat(this ILogger logger);

    [LoggerMessage(EventId = 9088, Level = LogLevel.Trace,
           Message = "Create the principal.")]
    public static partial void LogPrincipalCreation(this ILogger logger);

    [LoggerMessage(EventId = 9089, Level = LogLevel.Trace,
       Message = "Check if the cache contains a token for {TokenCacheKey}.")]
    public static partial void LogCheckContainsKeyInCache(this ILogger logger, string tokenCacheKey);

    [LoggerMessage(EventId = 9090, Level = LogLevel.Trace,
        Message = "Token loaded from the cache for {TokenCacheKey}.")]
    public static partial void LogTokenLoadedFromCache(this ILogger logger, string tokenCacheKey);

    [LoggerMessage(EventId = 9091, Level = LogLevel.Trace,
        Message = "Token loaded from the cache is expired {TokenCacheKey}.")]
    public static partial void LogTokenExpiredLoadedFromCache(this ILogger logger, string tokenCacheKey);

    [LoggerMessage(EventId = 9092, Level = LogLevel.Trace,
        Message = "Contact the Service Token Provider to create an access token for {TokenCacheKey}.")]
    public static partial void LogCallSTS(this ILogger logger, string tokenCacheKey);

    [LoggerMessage(EventId = 9093, Level = LogLevel.Trace,
        Message = "Save the token in the cache for {TokenCacheKey}, will expire at {ExpiresOnUtc} Utc.")]
    public static partial void LogSaveTokenInCache(this ILogger logger, string tokenCacheKey, DateTime expiresOnUtc);

    [LoggerMessage(EventId = 9094, Level = LogLevel.Warning,
        Message = "No cache is defined. STS is called for every call!")]
    public static partial void LogNoCachePerformance(this ILogger logger);

    [LoggerMessage(EventId = 9095, Level = LogLevel.Error,
    Message = "No token provider found for {CredentialTokenProviderProviderName}.")]
    public static partial void LogNoTokenProvider(this ILogger logger, string credentialTokenProviderProviderName);

    [LoggerMessage(EventId = 9096, Level = LogLevel.Trace,
    Message = "Creating an authentication context for the request.")]
    public static partial void LogCreateAuthenticationContext(this ILogger logger);
}
