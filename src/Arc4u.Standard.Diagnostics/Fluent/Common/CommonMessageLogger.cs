using Microsoft.Extensions.Logging;
using System;

namespace Arc4u.Diagnostics
{
    public class CommonMessageLogger
    {
        private LoggerMessage MessageLogger { get; set; }

        internal CommonMessageLogger(ILogger logger, MessageCategory category, string typeClass, string methodName)
        {
            MessageLogger = new LoggerMessage(logger, category, methodName, typeClass);
        }

        private CommonLoggerProperties AddEntry(LogLevel logLevel, string message, Exception exception = null)
        {
            MessageLogger.LogLevel = logLevel;
            MessageLogger.Text = message;
            MessageLogger.Exception = exception;

            return new CommonLoggerProperties(MessageLogger);
        }

        public CommonLoggerProperties Debug(string message)
        {
            return AddEntry(LogLevel.Debug, message);
        }

        public CommonLoggerProperties Information(string message)
        {
            return AddEntry(LogLevel.Information, message);
        }

        public CommonLoggerProperties Warning(string message)
        {
            return AddEntry(LogLevel.Warning, message);
        }

        public CommonLoggerProperties Fatal(string message)
        {
            return AddEntry(LogLevel.Critical, message);
        }

        public CommonLoggerProperties Error(string message)
        {
            return AddEntry(LogLevel.Error, message);
        }

        public CommonLoggerProperties Exception(Exception exception)
        {
            var property = AddEntry(LogLevel.Error, exception.Message, exception);
            return property.AddStacktrace(exception.StackTrace ?? Environment.StackTrace);
        }

        internal CommonLoggerProperties System(string message)
        {
            return AddEntry(LogLevel.Trace, message);
        }
    }
}
