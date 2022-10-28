using Microsoft.Extensions.Logging;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace Arc4u.Diagnostics
{
    public static class Helper
    {
        const string ExceptionDetail = nameof(ExceptionDetail);
        /// <summary>
        /// Extract from the LogEventProperties the Arc4u standard ones.
        /// The non standard one are added in the Properties list.
        /// Remove State property, used by Serilog to store the message!
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns></returns>
        public static (MessageCategory Category,
                        String Application,
                        String Identity,
                        String ClassType,
                        String MethodName,
                        String ActivityId,
                        int ProcessId,
                        int ThreadId,
                        String Stacktrace,
                        List<LogEventProperty> Properties)
        ExtractEventInfo(LogEvent logEvent)
        {
            String activityId = String.Empty,
                   methodName = String.Empty,
                   classType = String.Empty,
                   stacktrace = string.Empty,
                   identity = string.Empty,
                   application = string.Empty;
               int processId = -1,
                   threadId = -1;
             short category = -1;

            var filteredProperties = new List<LogEventProperty>();

            foreach (var property in logEvent.Properties)
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
                    case ExceptionDetail:
                        break;
                    default:
                        if (property.Key != "State") filteredProperties.Add(new LogEventProperty(property.Key, property.Value));
                        break;

                }
            }

            MessageCategory _category = MessageCategory.Technical;
            if (Enum.IsDefined(typeof(MessageCategory), category))
                _category = (MessageCategory)Enum.ToObject(typeof(MessageCategory), category);

            return (_category, application, identity, classType, methodName, activityId, processId, threadId, stacktrace, filteredProperties);
        }

        public static LogLevel ToMessageType(this LogEventLevel level)
        {
            switch (level)
            {
                case LogEventLevel.Debug:
                    return LogLevel.Debug;
                case LogEventLevel.Error:
                    return LogLevel.Error;
                case LogEventLevel.Fatal:
                    return LogLevel.Critical;
                case LogEventLevel.Information:
                    return LogLevel.Information;
                case LogEventLevel.Verbose:
                    return LogLevel.Trace;
                case LogEventLevel.Warning:
                    return LogLevel.Warning;
                default:
                    return LogLevel.Debug;
            }
        }

        public static T GetValue<T>(LogEventPropertyValue pv, T defaultValue)
        {
            switch (pv.GetType().Name)
            {
                case nameof(ScalarValue):
                    return (T)((ScalarValue)pv).Value;

            }

            return defaultValue;
        }
    }
}
