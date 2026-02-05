
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2;
public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 9080, Level = LogLevel.Trace,
                   Message = "The token cache is {cacheName}.")]
    public static partial void LogTokenCacheName(this ILogger logger, string cacheName);

    [LoggerMessage(EventId = 9081, Level = LogLevel.Trace,
                   Message = "The token cache is using the default cache defined in the config.")]
    public static partial void LogDefaultTokenCache(this ILogger logger);

    [LoggerMessage(EventId = 9082, Level = LogLevel.Trace,
               Message = "Requesting an authentication token.")]
    public static partial void LogRequestingAuthenticationToken(this ILogger logger);

    [LoggerMessage(EventId = 9083, Level = LogLevel.Error,
           Message = "Requesting an authentication token.")]
    public static partial void LogNoToken(this ILogger logger);

    [LoggerMessage(EventId = 9084, Level = LogLevel.Trace,
           Message = "Skip fetching claims, no setting found for authentication type {AuthenticationType}.")]
    public static partial void LogSkipFetchingClaims(this ILogger logger, string authenticationType);

    [LoggerMessage(EventId = 9085, Level = LogLevel.Error,
           Message = "Token endpoint for {Upn} returned {ResponseStatusCode}: {LoggedResponseBody}.")]
    public static partial void LogCredentialToken(this ILogger logger, string upn, string responseStatusCode, string loggedResponseBody);

    [LoggerMessage(EventId = 9086, Level = LogLevel.Debug,
       Message = "Creating an authentication context for the request.")]
    public static partial void LogCreatingAuthenticationContext(this ILogger logger);

    [LoggerMessage(EventId = 9087, Level = LogLevel.Debug,
       Message = "Call STS: {Authority} for user: {Upn}.")]
    public static partial void LogStsAndUser(this ILogger logger, string authority, string upn);

    [LoggerMessage(EventId = 9088, Level = LogLevel.Debug,
       Message = "Get token endpoint")]
    public static partial void LogGetEndpoint(this ILogger logger);

    [LoggerMessage(EventId = 9089, Level = LogLevel.Debug,
               Message = "Token is received for user {UserUpn}.")]
    public static partial void LogTokenReceived(this ILogger logger, string userUpn);

    [LoggerMessage(EventId = 9090, Level = LogLevel.Debug,
           Message = "Access token will expire at {TokenExpirationDateUtc} utc.")]
    public static partial void LogTokenExpiration(this ILogger logger, DateTime tokenExpirationDateUtc);

    [LoggerMessage(EventId = 9091, Level = LogLevel.Warning,
        Message = "Geting all data from the token cache is not implemented.")]
    public static partial void LogTokenCacheNotImplemented(this ILogger logger);

    [LoggerMessage(EventId = 9092, Level = LogLevel.Trace,
        Message = "Deleting information from the token cache for the id: {TokenKey}.")]
    public static partial void LogDeleteInTokenCache(this ILogger logger, string tokenKey);

    [LoggerMessage(EventId = 9093, Level = LogLevel.Trace,
        Message = "Deleted information from the token cache for the id: {TokenKey}.")]
    public static partial void LogDeletedInTokenCache(this ILogger logger, string tokenKey);

    [LoggerMessage(EventId = 9094, Level = LogLevel.Warning,
        Message = "A null token data information was provided to the cache, with an id: {TokenKey}.")]
    public static partial void LogNullTokenData(this ILogger logger, string tokenKey);

    [LoggerMessage(EventId = 9095, Level = LogLevel.Trace,
    Message = "Adding token data information to the cache with id: {TokenKey}.")]
    public static partial void LogAddingInTokenCache(this ILogger logger, string tokenKey);

    [LoggerMessage(EventId = 9096, Level = LogLevel.Trace,
        Message = "Added token data information to the cache with id: {TokenKey}.")]
    public static partial void LogAddedInTokenCache(this ILogger logger, string tokenKey);

    [LoggerMessage(EventId = 9097, Level = LogLevel.Trace,
        Message = "Retrieve token information for user: {TokenKey}.")]
    public static partial void LogGetDataTokenCache(this ILogger logger, string tokenKey);

    [LoggerMessage(EventId = 9098, Level = LogLevel.Warning,
        Message = "The data in cache is null for user: {TokenKey}.")]
    public static partial void LogGetNullDataTokenCache(this ILogger logger, string tokenKey);

#if NET10_0_OR_GREATER
    // Custom Root CA logging messages
    [LoggerMessage(EventId = 9100, Level = LogLevel.Debug,
                   Message = "Using custom root certificate option key: {Key}.")]
    public static partial void LogUsingCustomRootCertificateOptionKey(this ILogger logger, string key);

    [LoggerMessage(EventId = 9101, Level = LogLevel.Debug,
                   Message = "Loaded custom root certificate: {CertificateFriendlyName}.")]
    public static partial void LogLoadedCustomRootCertificate(this ILogger logger, string certificateFriendlyName);

    [LoggerMessage(EventId = 9102, Level = LogLevel.Warning,
                   Message = "No valid CA certificates loaded for custom root trust.")]
    public static partial void LogNoValidCaCertificatesLoaded(this ILogger logger);

    [LoggerMessage(EventId = 9103, Level = LogLevel.Error,
                   Message = "Failed to load internal CA certificate from {CertificateOptionKey}.")]
    public static partial void LogFailedToLoadCaCertificate(this ILogger logger, Exception ex, string? certificateOptionKey);

    [LoggerMessage(EventId = 9104, Level = LogLevel.Warning,
                   Message = "Failed to build certificate chain.")]
    public static partial void LogFailedToBuildCertificateChain(this ILogger logger);

    [LoggerMessage(EventId = 9105, Level = LogLevel.Warning,
                   Message = "Certificate validation failed for {Subject}. Chain status: {ChainStatus}.")]
    public static partial void LogCertificateValidationFailed(this ILogger logger, string? subject, string chainStatus);

    [LoggerMessage(EventId = 9106, Level = LogLevel.Warning,
        Message = "Failed to get remote certificate.")]
    public static partial void LogFailedToGetRemoteCertificate(this ILogger logger);

#endif
}
