
using Microsoft.Extensions.Logging;

namespace Arc4u;
public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 9100, Level = LogLevel.Warning,
                   Message = "Zone {Zone} is not found!")]
    public static partial void LogZoneNotFound(this ILogger logger, string zone);

    [LoggerMessage(EventId = 9101, Level = LogLevel.Trace,
                   Message = "Try to define the time zone to {Zone}.")]
    public static partial void LogTryToUseTimeZone(this ILogger logger, string zone);

    [LoggerMessage(EventId = 9102, Level = LogLevel.Error,
                   Message = "Certificate was not retrieved from the uri, {Uri}.")]
    public static partial void LogCertificateUriError(this ILogger logger, string uri);

    [LoggerMessage(EventId = 9103, Level = LogLevel.Trace,
                   Message = "Certificate used is {CertificateSubject}.")]
    public static partial void LogCertificateUsed(this ILogger logger, string certificateSubject);

    [LoggerMessage(EventId = 9104, Level = LogLevel.Error,
                   Message = "Pem extraction for Certificate {CertificateSubject} is empty.")]
    public static partial void LogEmptyPemCertificate(this ILogger logger, string certificateSubject);
}
