using Microsoft.Extensions.Logging;

namespace Arc4u.NServiceBus.Core;
public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 9050, Level = LogLevel.Information,
                   Message = "Publish event: {EventName}.")]
    public static partial void LogPublishEvent(this ILogger logger, string eventName);

    [LoggerMessage(EventId = 9051, Level = LogLevel.Information,
                  Message = "Send command: {CommandName}.")]
    public static partial void LogSendCommand(this ILogger logger, string commandName);
}
