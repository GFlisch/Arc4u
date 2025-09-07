using System.Globalization;
using Arc4u.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace Arc4u.UnitTest.Logging;

#region sinks and SerilogWriter

public sealed class SinkSpeedCategoryTest : ILogEventSink
{
    public const int Value = 101011;
    public bool Emited { get; set; }

    public uint HitCount { get; private set; }

    public void Emit(LogEvent logEvent)
    {
        if (Emited || HitCount == 0)
        {
            Emited = logEvent.Properties.TryGetValue("Speed", out var value) &&
                     value.ToString() == Value.ToString(CultureInfo.InvariantCulture);
        }

        HitCount++;
    }
}

public sealed class SinkTest : ILogEventSink
{
    public bool Emited { get; set; }

    public void Emit(LogEvent logEvent)
    {
        Emited = true;
    }
}

public sealed class FromSinkTest : ILogEventSink, IDisposable
{
    public string Class { get; set; } = default!;

    public string MethodName { get; set; } = default!;

    public string Application { get; set; } = default!;

    public MessageCategory Category { get; set; }

    public string? ActivityId { get; set; }

    public IReadOnlyDictionary<string, LogEventPropertyValue> Properties { get; set; } = default!;

    public void Dispose()
    {
    }

    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue(LoggingConstants.Class, out var classPropertyValue))
        {
            Class = GetValue(classPropertyValue, "")!;
        }

        if (logEvent.Properties.TryGetValue(LoggingConstants.MethodName, out var methodPropertyValue))
        {
            MethodName = GetValue(methodPropertyValue, "")!;
        }

        if (logEvent.Properties.TryGetValue(LoggingConstants.Category, out var categoryPropertyValue))
        {
            Category = (MessageCategory)Enum.Parse(typeof(MessageCategory),
                GetValue(categoryPropertyValue, MessageCategory.Technical.ToString())!);
        }

        if (logEvent.Properties.TryGetValue(LoggingConstants.Application, out var applicationName))
        {
            Application = GetValue(applicationName, "")!;
        }

        if (logEvent.Properties.TryGetValue(LoggingConstants.ActivityId, out var activityId))
        {
            ActivityId = GetValue(activityId, "");
        }

        Properties = logEvent.Properties;
    }

    public T? GetValue<T>(LogEventPropertyValue pv, T defaultValue)
    {
        return pv.GetType().Name switch
        {
            nameof(ScalarValue) => (T?)((ScalarValue)pv).Value,
            _ => defaultValue
        };
    }
}

#endregion sinks and SerilogWriter
