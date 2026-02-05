#if NET10_0_OR_GREATER

using Microsoft.Extensions.Logging;

namespace Arc4u.AspNetCore.gRpc;

public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 10100, Level = LogLevel.Debug,
                   Message = "Using custom root certificate option key: {Key}.")]
    public static partial void LogUsingCustomRootCertificateOptionKey(this ILogger logger, string key);

    [LoggerMessage(EventId = 10101, Level = LogLevel.Debug,
                   Message = "Loaded custom root certificate: {CertificateFriendlyName}.")]
    public static partial void LogLoadedCustomRootCertificate(this ILogger logger, string certificateFriendlyName);

    [LoggerMessage(EventId = 10102, Level = LogLevel.Warning,
                   Message = "No valid CA certificates loaded for custom root trust.")]
    public static partial void LogNoValidCACertificatesLoaded(this ILogger logger);

    [LoggerMessage(EventId = 10103, Level = LogLevel.Error,
                   Message = "Failed to load internal CA certificate from {CertificateOptionKey}.")]
    public static partial void LogFailedToLoadCACertificate(this ILogger logger, Exception ex, string? certificateOptionKey);

    [LoggerMessage(EventId = 10104, Level = LogLevel.Warning,
                   Message = "Failed to build certificate chain.")]
    public static partial void LogFailedToBuildCertificateChain(this ILogger logger);

    [LoggerMessage(EventId = 10105, Level = LogLevel.Warning,
                   Message = "Failed to get remote certificate.")]
    public static partial void LogFailedToGetRemoteCertificate(this ILogger logger);

    [LoggerMessage(EventId = 10106, Level = LogLevel.Warning,
                   Message = "Certificate validation failed for {Subject}. Chain status: {ChainStatus}.")]
    public static partial void LogCertificateValidationFailed(this ILogger logger, string? subject, string chainStatus);
}

#endif
