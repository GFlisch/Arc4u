using Arc4u.Diagnostics.Formatter;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.PeriodicBatching;

namespace Arc4u.Diagnostics.Serilog.Sinks.Memory
{
    public class MemoryLogDbSink : IBatchedLogEventSink
    {
        public MemoryLogDbSink() //: base(50, TimeSpan.FromMilliseconds(500))
        {
            LogMessages = new MemoryLogMessages();
            MessageFormatter = new MessageTemplateTextFormatter("{Message}");
            PropertyFormatter = new JsonPropertiesFormatter();

        }
        private MessageTemplateTextFormatter MessageFormatter { get; set; }
        // Serialize in json the properties non Arc4u standard.
        private JsonPropertiesFormatter PropertyFormatter { get; set; }

        public static MemoryLogMessages LogMessages;

        public Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            foreach (var _event in events)
            {
                var (Category, Application, Identity, ClassType, MethodName, ActivityId, ProcessId, ThreadId, Stacktrace, Properties) = Helper.ExtractEventInfo(_event);

                try
                {
                    using var properties = new StringWriter();
                    using var messageText = new StringWriter();
                    PropertyFormatter.Format(Properties, properties);
                    MessageFormatter.Format(_event, messageText);

                    var logMsg = new LogMessage
                    {
                        Timestamp = _event.Timestamp,
                        Message = messageText.ToString(),
                        ActivityId = ActivityId,
                        MethodName = MethodName,
                        ClassType = ClassType,
                        Properties = properties.ToString(),
                        ProcessId = ProcessId,
                        ThreadId = ThreadId,
                        Stacktrace = Stacktrace,
                        MessageCategory = Category.ToString(),
                        MessageType = _event.Level.ToMessageType().ToString(),
                        Application = Application,
                        Identity = Identity,
                    };

                    LogMessages.Add(logMsg);
                }
                catch (Exception)
                {
                }

            }

            return Task.CompletedTask;
        }

        public Task OnEmptyBatchAsync()
        {
            return Task.CompletedTask;
        }
    }
}
