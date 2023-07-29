using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Arc4u.Diagnostics
{
    [Obsolete("Use Serilog")]
    public class TraceLogger : ILogger
    {
        public TraceLogger(string name)
        {

        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (state is Dictionary<string, object>)
            {
                var (Category, Application, Identity, ClassType, MethodName, ActivityId, Code, Subject, ProcessId, ThreadId, Stacktrace, Properties) = ExtractStateInfo(state as Dictionary<string, object>);

                var message = new TraceMessage
                {
                    Category = Category,
                    Code = Code,
                    Subject = Subject,
                    Type = logLevel,
                    Text = formatter(state, exception),
                    Source = new MessageSource(ClassType, MethodName, Application, Identity, -1),
                    RawData = null,
                    ActivityId = ActivityId,
                };

                message.Source.ThreadId = ThreadId;
                message.Source.ProcessId = ProcessId;
                message.Source.Stacktrace = Stacktrace;

                Trace.Write(message);

            }
        }



        private static (MessageCategory Category,
                        String Application,
                        String Identity,
                        String ClassType,
                        String MethodName,
                        String ActivityId,
                        String Code,
                        String Subject,
                        int ProcessId,
                        int ThreadId,
                        String Stacktrace,
                        Dictionary<string, object> Properties)
            ExtractStateInfo(Dictionary<string, object> states)
        {
            String activityId = String.Empty,
            methodName = String.Empty,
            classType = String.Empty,
            stacktrace = string.Empty,
            identity = string.Empty,
            code = string.Empty,
            subject = string.Empty,
            application = string.Empty;

            int processId = -1,
            threadId = -1;
            short category = -1;

            var filteredProperties = new Dictionary<string, object>();

            foreach (var property in states)
            {
                switch (property.Key)
                {
                    case LoggingConstants.ActivityId:
                        activityId = GetValue(property.Value, Guid.Empty.ToString());
                        break;
                    case LoggingConstants.MethodName:
                        methodName = GetValue(property.Value, String.Empty);
                        break;
                    case LoggingConstants.Class:
                        classType = GetValue(property.Value, String.Empty);
                        break;
                    case LoggingConstants.ProcessId:
                        processId = GetValue(property.Value, -1);
                        break;
                    case LoggingConstants.ThreadId:
                        threadId = GetValue(property.Value, -1);
                        break;
                    case LoggingConstants.Category:
                        category = GetValue(property.Value, (short)1);
                        break;
                    case LoggingConstants.Stacktrace:
                        stacktrace = GetValue(property.Value, String.Empty);
                        break;
                    case LoggingConstants.Identity:
                        identity = GetValue(property.Value, String.Empty);
                        break;
                    case LoggingConstants.Application:
                        application = GetValue(property.Value, String.Empty);
                        break;
                    case "Code":
                        code = GetValue(property.Value, String.Empty);
                        break;
                    case "Subject":
                        subject = GetValue(property.Value, String.Empty);
                        break;
                    default:
                        if (property.Key != "State") filteredProperties.Add(property.Key, property.Value);
                        break;

                }
            }

            MessageCategory _category = MessageCategory.Technical;
            if (Enum.IsDefined(typeof(MessageCategory), category))
                _category = (MessageCategory)Enum.ToObject(typeof(MessageCategory), category);

            return (_category, application, identity, classType, methodName, activityId, code, subject, processId, threadId, stacktrace, filteredProperties);
        }

        private static T GetValue<T>(object value, T defaultValue)
        {
            try
            {
                return (T)value;
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
