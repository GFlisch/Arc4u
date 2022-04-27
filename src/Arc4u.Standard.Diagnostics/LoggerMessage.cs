using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Arc4u.Diagnostics
{
    public class LoggerMessage
    {
        static LoggerMessage()
        {
            try
            {
                ProcessId = Process.GetCurrentProcess().Id;
            }
            catch (PlatformNotSupportedException)
            {
                ProcessId = -1;
            }
            
        }

        private static int ProcessId { get; set; }
        private readonly ILogger _logger;

        internal LoggerMessage(ILogger logger, MessageCategory category, string methodName, string typeClass)
        {
            _logger = logger;
            MethodName = methodName;
            TypeClass = typeClass;
            Category = category;
            Properties = new Dictionary<string, object>();
        }

        public MessageCategory Category { get; }
        public LogLevel LogLevel { get; set; }
        public string Text { get; set; }
        public string StackTrace { get; set; }

        public string MethodName { get; }
        public string TypeClass { get; }
        public object[] Args { get; set; }

        internal Dictionary<string, object> Properties { get; }
        internal Exception Exception { get; set; }

        internal void Log()
        {
            if (LogLevel < LoggerBase.FilterLevel) return;

            if (null == _logger) return;

            // Used to extract the properties injected in the message and extracted from the internal struct Microsoft.Extensions.Logging.FormattedLogValues
            var extractor = new StateExtractorLogger();
            extractor.Log(LogLevel,
                          0,
                          Exception,
                          Text,
                          Args);

            foreach(var property in extractor.Properties)
                Properties.AddIfNotExist(property.Key, property.Value);

            if (null != LoggerContext.Current?.All())
            {
                foreach (var property in LoggerContext.Current.All())
                    Properties.AddIfNotExist(property.Key, property.Value);
            }

            Properties.AddIfNotExist(LoggingConstants.Application, LoggerBase.Application);
            Properties.AddIfNotExist(LoggingConstants.ThreadId, Thread.CurrentThread.ManagedThreadId);
            Properties.AddIfNotExist(LoggingConstants.Class, TypeClass);
            Properties.AddIfNotExist(LoggingConstants.MethodName, MethodName);
            Properties.AddIfNotExist(LoggingConstants.ProcessId, ProcessId);
            Properties.AddIfNotExist(LoggingConstants.Category, (short)Category);
            Properties.AddIfNotExist(LoggingConstants.Stacktrace, StackTrace);


            _logger.Log(LogLevel,
                        0,
                        Properties,
                        Exception,
                        (state, ex) => "");
        }
    }

    internal class StateExtractorLogger : ILogger
    {
        public StateExtractorLogger()
        {
            _properties = new();
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<KeyValuePair<string, object>> Properties => _properties;

        public string Message => _message;

        private List<KeyValuePair<string, object>> _properties;
        private string _message;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (state is IEnumerable<KeyValuePair<string, object>> pairs)
            {
                _properties.AddRange(pairs);
            }
        }
    }
}