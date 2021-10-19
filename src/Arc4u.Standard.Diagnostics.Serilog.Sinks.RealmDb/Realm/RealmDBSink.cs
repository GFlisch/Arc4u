using Arc4u.Diagnostics.Formatter;
using Realms;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.PeriodicBatching;
using System;
using System.Collections.Generic;
using System.IO;

namespace Arc4u.Diagnostics.Serilog.Sinks.RealmDb
{
    public class RealmDBSink : PeriodicBatchingSink
    {
        public RealmDBSink(RealmConfiguration config) : base(50, TimeSpan.FromMilliseconds(500))
        {
            MessageFormatter = new MessageTemplateTextFormatter("{Message}");
            PropertyFormatter = new JsonPropertiesFormatter();
            Config = config;
        }

        private MessageTemplateTextFormatter MessageFormatter { get; set; }
        // Serialize in json the properties non Arc4u standard.
        private JsonPropertiesFormatter PropertyFormatter { get; set; }

        private Realm DB { get; set; }
        private RealmConfiguration Config { get; set; }


        protected override void EmitBatch(IEnumerable<LogEvent> events)
        {
            DB = Realm.GetInstance(Config);

            foreach (var _event in events)
            {
                var (Category, Application, Identity, ClassType, MethodName, ActivityId, ProcessId, ThreadId, Stacktrace, Properties) = Helper.ExtractEventInfo(_event);

                try
                {
                    using (var properties = new StringWriter())
                    using (var messageText = new StringWriter())
                    {
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

                        DB.Write(() =>
                        {
                            DB.Add(logMsg);
                        });
                    }
                }
                catch (Exception)
                {
                }

            }

        }
    }
}
