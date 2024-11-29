using Arc4u.Diagnostics.Formatter;
using Realms;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.PeriodicBatching;

namespace Arc4u.Diagnostics.Serilog.Sinks.RealmDb;

public class RealmDBSink : IBatchedLogEventSink
{
    public RealmDBSink(RealmConfiguration config)  //: base(50, TimeSpan.FromMilliseconds(500))
    {
        MessageFormatter = new MessageTemplateTextFormatter("{Message}");
        PropertyFormatter = new JsonPropertiesFormatter();
        Config = config;
    }

    private MessageTemplateTextFormatter MessageFormatter { get; set; }
    // Serialize in json the properties non Arc4u standard.
    private JsonPropertiesFormatter PropertyFormatter { get; set; }

    private RealmConfiguration Config { get; set; }

    public async Task EmitBatchAsync(IEnumerable<LogEvent> events)
    {
        Realm DB = await Realm.GetInstanceAsync(Config).ConfigureAwait(false);

        foreach (var _event in events)
        {
            var (Category, Application, Identity, ClassType, MethodName, ActivityId, ProcessId, ThreadId, Stacktrace, Properties) = Helper.ExtractEventInfo(_event);

            try
            {
                using var properties = new StringWriter();
                using var messageText = new StringWriter();
                PropertyFormatter.Format(Properties, properties);
                MessageFormatter.Format(_event, messageText);

                var logMsg = new LogDBMessage
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
                    MessageCategory = (short)Category,
                    MessageType = (int)_event.Level.ToMessageType(),
                    Application = Application,
                    Identity = Identity,
                };

                await DB.WriteAsync(() =>
                {
                    DB.Add(logMsg);
                }).ConfigureAwait(false);
            }
            catch (Exception)
            {
            }

        }
    }

    public Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }
}
