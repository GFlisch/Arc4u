using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Arc4u.Diagnostics;

/// <summary>
/// Helper class to unit test the Arc4u logging.
/// </summary>
/// <typeparam name="T"></typeparam>
public class NullLoggerWrapper<T> : ILoggerWrapper<T>
{
    public static readonly NullLoggerWrapper<T> Instance = new();

    private readonly ILogger<T> _logger = NullLogger<T>.Instance;


    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _logger.Log(logLevel, eventId, state, exception, formatter);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _logger.IsEnabled(logLevel);
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return _logger.BeginScope(state);
    }

    public void CallerMemberName(string caller)
    {
        // No implementation.
    }

    public Dictionary<string, object?> AdditionalFields { get; } = new();
    public bool IncludeStackTrace { get; set; }
    public ILoggerWrapper<T> SetContext(string category, string caller = "", Type? realType = null)
    {
        return this;
    }
}
