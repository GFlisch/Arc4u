using System.Globalization;
using System.Text;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace Arc4u.Diagnostics.Formatter;

public class SimpleTextFormatter : ITextFormatter
{
    public SimpleTextFormatter()
    {
        MessageFormatter = new MessageTemplateTextFormatter("{Message}");
        PropertyFormatter = new JsonPropertiesFormatter();

#if NET8_0_OR_GREATER
        MessageCategoryMap = Enum.GetValues<MessageCategory>().ToDictionary(x => x, x => x.ToString());
        MessageLogLevelMap = Enum.GetValues<LogEventLevel>().ToDictionary(x => x, x => x.ToMessageType().ToString());
#else
        MessageCategoryMap = Enum.GetValues(typeof(MessageCategory)).Cast<MessageCategory>()
            .ToDictionary(x => x, x => x.ToString());

        MessageLogLevelMap = Enum.GetValues(typeof(LogEventLevel)).Cast<LogEventLevel>()
            .ToDictionary(x => x, x => x.ToMessageType().ToString());
#endif
    }
    private Dictionary<MessageCategory, string> MessageCategoryMap { get; }

    private Dictionary<LogEventLevel, string> MessageLogLevelMap { get; }

    private MessageTemplateTextFormatter MessageFormatter { get; set; }

    // Serialize in json the properties non Arc4u standard.
    private JsonPropertiesFormatter PropertyFormatter { get; set; }

    public void Format(LogEvent logEvent, TextWriter output)
    {
        try
        {
            var (Category, _, Identity, ClassType, MethodName, ActivityId, ProcessId, ThreadId, Stacktrace, Properties) = Helper.ExtractEventInfo(logEvent);

            using var properties = new StringWriter();
            using var messageText = new StringWriter();

            MessageFormatter.Format(logEvent, messageText);
            PropertyFormatter.Format(Properties, properties);

            // Output the text.
            output.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3} {4} {5} {6} {7} - {8} - {9} - {10}",
                                           logEvent.Timestamp.ToString("dd/MM/yyyy HH:mm:ss,fff", CultureInfo.InvariantCulture).PadRight(24),
                                           MessageLogLevelMap[logEvent.Level].PadRight(13),
                                           MessageCategoryMap[Category].PadRight(14),
                                           Identity.PadRight(15),
                                           ProcessId.ToString(CultureInfo.InvariantCulture).PadRight(6),
                                           ThreadId.ToString(CultureInfo.InvariantCulture).PadRight(6),
                                           ActivityId.PadRight(40),
                                           ClassType,
                                           MethodName,
                                           messageText.ToString(),
                                           properties.ToString()));

            if (null != logEvent.Exception)
            {
                output.Write(logEvent.Exception.ToFormattedstring());
            }

            if (!string.IsNullOrWhiteSpace(Stacktrace))
            {
                output.WriteLine(Stacktrace);
            }
        }
        // We don't want to throw an exception and log this on the system who is assigned to log an information.
        catch (Exception) { }

    }
}

internal static class DumpException
{
    internal static string ToFormattedstring(this Exception exception)
    {
        var messages = exception
            .GetAllExceptions()
            .Where(e => !string.IsNullOrWhiteSpace(e.Message))
            .Select(e => e.GetType().FullName + " : " + e.Message.Trim());
        var sb = new StringBuilder();
        var i = 0;
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
            foreach (var innerEx in aggrEx.InnerExceptions.SelectMany(e => e.GetAllExceptions()))
            {
                yield return innerEx;
            }
        }
        else if (exception.InnerException != null)
        {
            foreach (var innerEx in exception.InnerException.GetAllExceptions())
            {
                yield return innerEx;
            }
        }
    }
}
