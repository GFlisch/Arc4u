
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2;
public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 9200, Level = LogLevel.Warning,
                   Message = "Unable to resolve the IEndpointConfiguration with the name '{EndpointConfiguration}'.")]
    public static partial void LogNoEndpointConfiguration(this ILogger logger, string endpointConfiguration);

    [LoggerMessage(EventId = 9201, Level = LogLevel.Warning,
                   Message = "Instance is null for the IEndpointConfiguration with the name '{EndpointConfiguration}'.")]
    public static partial void LogNullEndpointConfiguration(this ILogger logger, string endpointConfiguration);

    [LoggerMessage(EventId = 9202, Level = LogLevel.Warning,
                   Message = "Unable to send any events or commands to the IEndpointConfiguration.")]
    public static partial void LogCannotSendToEndpointConfiguration(this ILogger logger);

    [LoggerMessage(EventId = 9203, Level = LogLevel.Trace,
                   Message = "Send command: {Command}.")]
    public static partial void LogSendCommand(this ILogger logger, string command);

    [LoggerMessage(EventId = 9203, Level = LogLevel.Trace,
               Message = "Publish event: {EventName}.")]
    public static partial void LogPublishEvent(this ILogger logger, string eventName);
}
