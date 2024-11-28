using Serilog.Core;
using Serilog.Events;

namespace Arc4u.Diagnostics.Sinks;

public class CategoryFilterSink : ILogEventSink, IDisposable
{
    public CategoryFilterSink(MessageCategory categories, ILogEventSink sink)
    {
        Categories = categories;
        Sink = sink;
    }

    private MessageCategory Categories { get; set; }
    public ILogEventSink Sink { get; set; }

    public void Dispose()
    {
        if (Sink is IDisposable)
        {
            ((IDisposable)Sink).Dispose();
        }
    }

    /// <summary>
    /// Check if the message contains a category property!
    /// If no, the message is skipped.
    /// If yes, only messages with the registered categories are sent.
    /// </summary>
    /// <param name="logEvent"></param>
    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue(LoggingConstants.Category, out var propertyValue))
        {
            short iCategory = Helper.GetValue<short>(propertyValue, -1);
            if (typeof(MessageCategory).IsEnumDefined(iCategory))
            {
                MessageCategory category = (MessageCategory)iCategory;
                if (Categories.HasFlag(category))
                {
                    Sink?.Emit(logEvent);
                }
            }
        }
    }
}
