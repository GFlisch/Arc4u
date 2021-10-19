using Serilog.Core;
using Serilog.Events;
using System;
using System.Linq;

namespace Arc4u.Diagnostics.Sinks
{
    public class AnonymizerSink : ILogEventSink, IDisposable
    {
        public AnonymizerSink(ILogEventSink sink)
        {
            Sink = sink;
        }
        public ILogEventSink Sink { get; set; }

        public void Dispose()
        {
            if (Sink is IDisposable)
                ((IDisposable)Sink).Dispose();
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent.Properties.TryGetValue(LoggingConstants.Identity, out var propertyValue))
            {
                var _event = new LogEvent(logEvent.Timestamp, logEvent.Level, logEvent.Exception, logEvent.MessageTemplate, logEvent.Properties.Where(p => !p.Key.Equals(LoggingConstants.Identity)).Select(p => new LogEventProperty(p.Key, p.Value)).ToList());
                Sink?.Emit(_event);
            }
            else
                Sink?.Emit(logEvent);

        }
    }
}
