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
        internal Dictionary<string, object> Properties { get; }
        internal Exception Exception { get; set; }

        internal void Log()
        {
            if (LogLevel < LoggerBase.FilterLevel) return;

            if (null == _logger) return;

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
                        (state, ex) => Text);
        }
    }
}