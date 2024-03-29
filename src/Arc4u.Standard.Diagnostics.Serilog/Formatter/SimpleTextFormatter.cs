﻿using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Arc4u.Diagnostics.Formatter
{
    public class SimpleTextFormatter : ITextFormatter
    {
        public SimpleTextFormatter()
        {
            MessageFormatter = new MessageTemplateTextFormatter("{Message}");
            PropertyFormatter = new JsonPropertiesFormatter();

            MessageCategoryMap = Enum.GetValues(typeof(MessageCategory)).Cast<MessageCategory>()
                .ToDictionary(x => x, x => x.ToString());

            MessageLogLevelMap = Enum.GetValues(typeof(LogEventLevel)).Cast<LogEventLevel>()
                .ToDictionary(x => x, x => x.ToMessageType().ToString());
        }

        private IDictionary<MessageCategory, string> MessageCategoryMap { get; }

        private IDictionary<LogEventLevel, string> MessageLogLevelMap { get; }

        private MessageTemplateTextFormatter MessageFormatter { get; set; }

        // Serialize in json the properties non Arc4u standard.
        private JsonPropertiesFormatter PropertyFormatter { get; set; }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            try
            {
                var (Category, Application, Identity, ClassType, MethodName, ActivityId, ProcessId, ThreadId, Stacktrace, Properties) = Helper.ExtractEventInfo(logEvent);

                using (var properties = new StringWriter())
                using (var messageText = new StringWriter())
                {
                    MessageFormatter.Format(logEvent, messageText);
                    PropertyFormatter.Format(Properties, properties);

                    // Output the text.

                    output.WriteLine(string.Format("{0} {1} {2} {3} {4} {5} {6} {7} - {8} - {9} - {10}",
                                                                logEvent.Timestamp.ToString("dd/MM/yyyy HH:mm:ss,fff").PadRight(24),
                                                                MessageLogLevelMap[logEvent.Level].PadRight(13),
                                                                MessageCategoryMap[Category].PadRight(14),
                                                                Identity.PadRight(15),
                                                                ProcessId.ToString().PadRight(6),
                                                                ThreadId.ToString().PadRight(6),
                                                                ActivityId.PadRight(40),
                                                                ClassType,
                                                                MethodName,
                                                                messageText.ToString(),
                                                                properties.ToString()));

                    if (null != logEvent.Exception)
                        output.Write(logEvent.Exception.ToFormattedString());

                    if (!String.IsNullOrWhiteSpace(Stacktrace))
                        output.WriteLine(Stacktrace);
                }
            }
            catch (Exception)
            {
            }

        }
    }

    internal static class DumpException
    {
        internal static string ToFormattedString(this Exception exception)
        {
            IEnumerable<string> messages = exception
                .GetAllExceptions()
                .Where(e => !String.IsNullOrWhiteSpace(e.Message))
                .Select(e => e.GetType().FullName + " : " + e.Message.Trim());
            var sb = new StringBuilder();
            int i = 0;
            foreach (var message in messages)
            {
                sb.Append("".PadLeft(i++ * 4));
                sb.Append("|---".PadLeft(i > 0 ? 4 : 0));
                sb.AppendLine(message);
            }

            return sb.ToString();
        }

        private static IEnumerable<Exception> GetAllExceptions(this Exception exception)
        {
            yield return exception;

            if (exception is AggregateException aggrEx)
            {
                foreach (Exception innerEx in aggrEx.InnerExceptions.SelectMany(e => e.GetAllExceptions()))
                {
                    yield return innerEx;
                }
            }
            else if (exception.InnerException != null)
            {
                foreach (Exception innerEx in exception.InnerException.GetAllExceptions())
                {
                    yield return innerEx;
                }
            }
        }
    }

}
