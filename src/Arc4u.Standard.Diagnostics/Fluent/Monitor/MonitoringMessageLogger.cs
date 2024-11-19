using Microsoft.Extensions.Logging;

namespace Arc4u.Diagnostics
{

    public class MonitoringMessageLogger
    {
        private readonly ILogger _logger;
        private LoggerMessage MessageLogger { get; set; }

        internal MonitoringMessageLogger(ILogger logger, string typeClass, string methodName)
        {
            _logger = logger;

            MessageLogger = new LoggerMessage(logger, MessageCategory.Monitoring, methodName, typeClass);
        }

        private MonitoringLoggerProperties AddEntry(LogLevel logLevel, string message, Exception exception = null)
        {
            MessageLogger.LogLevel = logLevel;
            MessageLogger.Text = message;
            MessageLogger.Exception = exception;

            return new MonitoringLoggerProperties(MessageLogger);
        }

        public MonitoringLoggerProperties Debug(string message)
        {
            return AddEntry(LogLevel.Debug, message);
        }

        public MonitoringLoggerProperties Information(string message)
        {
            return AddEntry(LogLevel.Information, message);
        }

        internal MonitoringLoggerProperties System(string message)
        {
            return AddEntry(LogLevel.Trace, message);
        }
    }
}
