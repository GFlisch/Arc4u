
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2;
public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 9070, Level = LogLevel.Trace,
                   Message = "Authorization header found. Skip adding a bearer token for AuthenticationType: {AuthenticationType}.")]
    public static partial void LogSkipAddingBearerToken(this ILogger logger, string authenticationType);

    [LoggerMessage(EventId = 9071, Level = LogLevel.Trace,
                   Message = "No settings or application context is defined with {ContextName}, Check next Delegate Handler.")]
    public static partial void LogNoApplicationContextIsDefined(this ILogger logger, string contextName);

    [LoggerMessage(EventId = 9072, Level = LogLevel.Trace,
                   Message = "No authentication type for {AuthenticationType}, Check next Interceptor.")]
    public static partial void LogNoAuthenticationTypeIsDefined(this ILogger logger, string authenticationType);

    [LoggerMessage(EventId = 9073, Level = LogLevel.Trace,
                   Message = "No user context, Check next Interceptor.")]
    public static partial void LogNoUserContext(this ILogger logger);

    [LoggerMessage(EventId = 9074, Level = LogLevel.Trace,
                   Message = "No token provider is defined for {TokenProvider}, Check next Interceptor.")]
    public static partial void LogNoTokenProviderIsDefined(this ILogger logger, string tokenProvider);

    [LoggerMessage(EventId = 9075, Level = LogLevel.Trace,
                   Message = "No token provider is defined for {TokenProvider}, Check next Interceptor.")]
    public static partial void LogNoTokenIsProvided(this ILogger logger, string tokenProvider);

    [LoggerMessage(EventId = 9076, Level = LogLevel.Trace,
                   Message = "Token is expired! Next Interceptor will be called.")]
    public static partial void LogGrpcTokenIsExpired(this ILogger logger);

    [LoggerMessage(EventId = 9080, Level = LogLevel.Trace,
                   Message = "Add the {Scheme} token to provide authentication evidence.")]
    public static partial void LogAddSchemeToken(this ILogger logger, string scheme);

    [LoggerMessage(EventId = 9077, Level = LogLevel.Trace,
                   Message = "Add the current culture to the request: {CurrentCulture}.")]
    public static partial void LogAddCurrentCulture(this ILogger logger, string currentCulture);

    [LoggerMessage(EventId = 9078, Level = LogLevel.Trace,
                   Message = "Certificate callback received with Subject = {CertificateSubject}.")]
    public static partial void LogCertificateFeedBackReceived(this ILogger logger, string certificateSubject);

    [LoggerMessage(EventId = 9079, Level = LogLevel.Error,
                  Message = "Certificate callback received with no certificate!")]
    public static partial void LogCertificateFeedBackWithNoCertificate(this ILogger logger);
}
