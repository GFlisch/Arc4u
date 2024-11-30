using Microsoft.Extensions.Logging;
#if NETSTANDARD2_0
using System.Diagnostics;
#endif
namespace Arc4u.Diagnostics;

public class LoggerMessage
{
    static LoggerMessage()
    {
        try
        {
#if NET8_0_OR_GREATER
            ProcessId = Environment.ProcessId;
#else
            ProcessId = Process.GetCurrentProcess().Id;
#endif
        }
        catch (PlatformNotSupportedException)
        {
            ProcessId = -1;
        }
    }

    private static int ProcessId { get; }
    private readonly ILogger _logger;

    internal LoggerMessage(ILogger logger, MessageCategory category, string methodName, string typeClass)
    {
        _logger = logger;
        MethodName = methodName;
        TypeClass = typeClass;
        Category = category;
        Properties = [];
        Text = string.Empty;
        StackTrace = string.Empty;
        Args = [];
        Exception = default!;
    }

    public MessageCategory Category { get; }
    public LogLevel LogLevel { get; set; }
    public string Text { get; set; }
    public string StackTrace { get; set; }

    public string MethodName { get; }
    public string TypeClass { get; }
    public object[] Args { get; set; }

    internal Dictionary<string, object> Properties { get; }
    internal Exception? Exception { get; set; }

    internal void Log()
    {
        if (LogLevel < LoggerBase.FilterLevel)
        {
            return;
        }

        if (null == _logger)
        {
            return;
        }

        if (null != LoggerContext.Current?.All())
        {
            foreach (var property in LoggerContext.Current.All())
            {
                Properties.AddIfNotExist(property.Key, property.Value);
            }
        }

        Properties.AddIfNotExist(LoggingConstants.Application, LoggerBase.Application ?? "Unknown App");
        Properties.AddIfNotExist(LoggingConstants.ThreadId, Environment.CurrentManagedThreadId);
        Properties.AddIfNotExist(LoggingConstants.Class, TypeClass);
        Properties.AddIfNotExist(LoggingConstants.MethodName, MethodName);
        Properties.AddIfNotExist(LoggingConstants.ProcessId, ProcessId);
        Properties.AddIfNotExist(LoggingConstants.Category, (short)Category);
        Properties.AddIfNotExist(LoggingConstants.Stacktrace, StackTrace);

        // Add the internal Arc4u properties to whatever the TState provides already before logging
        var stateLogger = new StateLogger(Properties, _logger);
        stateLogger.Log(LogLevel, 0, Exception, Text, Args);

        // if this was an aggregate exception (a common occurrence in async programming), we also log the individual innner exceptions, which is better than just "One or more errors occurred".
        if (Exception is AggregateException aggregateException)
        {
            foreach (var innerException in aggregateException.Flatten().InnerExceptions)
            {
                // we know that if we have an exception, the Text property is the message of the exception so we call the state logger with the message of the inner exception instead.
                // we also replace the stack trace with the exception's stack trace. Strictly speaking, this changes the state of the LoggerMessage but since Log() is supposed to
                // be the last method called, we don't mind.
                if (string.IsNullOrEmpty(innerException.StackTrace))
                {
                    Properties.Remove(LoggingConstants.Stacktrace);
                }
                else
                {
                    Properties[LoggingConstants.Stacktrace] = CommonLoggerProperties.CleanupStackTrace(innerException.StackTrace);
                }

                stateLogger.Log(LogLevel, 0, innerException, innerException.Message, Args);
            }
        }
    }
}

class StateLogger : ILogger
{
    private readonly IReadOnlyDictionary<string, object> _properties;
    private readonly ILogger _logger;

    public StateLogger(IReadOnlyDictionary<string, object> properties, ILogger logger)
    {
        _properties = properties;
        _logger = logger;
    }

    IDisposable? ILogger.BeginScope<TState>(TState state)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(state);
#else
        if (null == state)
        {
            throw new ArgumentNullException(nameof(state));
        }
#endif
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        throw new NotImplementedException();
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (state is IEnumerable<KeyValuePair<string, object>> pairs)
        {
            Dictionary<string, object> mergedState = [];
            foreach (var pair in pairs)
            {
                mergedState[pair.Key] = pair.Value;
            }

            foreach (var property in _properties)
            {
                mergedState.AddIfNotExist(property.Key, property.Value);
            }

            _logger.Log(logLevel, eventId, mergedState, exception, LocalFormatter);

            // we can do this since the template is not aware of the internal Arc4u properties.
            string LocalFormatter(Dictionary<string, object> extendedState, Exception? e) => formatter(state, exception);
        }
        else
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}
