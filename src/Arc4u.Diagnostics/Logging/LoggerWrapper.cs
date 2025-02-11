using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arc4u.Diagnostics;

public interface ILoggerWrapper<T> : ILogger<T>, ILoggerCallerMember
{
    public Dictionary<string, object?> AdditionalFields { get; }

    public bool IncludeStackTrace { set; }

    ILoggerWrapper<T> SetContext(string category, string caller = "", Type? realType = null);

}

public interface IScopedLogger<T> : ILoggerWrapper<T>
{

}

public sealed class LoggerWrapper<T> : LoggerBaseWrapper<T>
{
    public LoggerWrapper(ILoggerFactory loggerFactory, [FromKeyedServices("Transient")] IAddPropertiesToLog addPropertiesToLog): base(loggerFactory, addPropertiesToLog)   
    {
        
    }
}

public sealed class ScopedLoggerWrapper<T> : LoggerBaseWrapper<T>
{
    public ScopedLoggerWrapper(ILoggerFactory loggerFactory, [FromKeyedServices("Scoped")] IAddPropertiesToLog addPropertiesToLog) : base(loggerFactory, addPropertiesToLog)
    {

    }
}

public abstract class LoggerBaseWrapper<T> : IScopedLogger<T>
{
    internal readonly ILogger _logger;
    private string _category;
    private readonly Dictionary<string, object?> _additionalFields = [];
    private Type _contextType;
    private string _caller = string.Empty;
    private bool _disposed;
    private readonly IAddPropertiesToLog? _addPropertiesToLog;

    public Dictionary<string, object?> AdditionalFields => _additionalFields;
    public bool IncludeStackTrace { private get; set; }

    internal static int ProcessId
    {
        get
        {
            try
            {
                return Environment.ProcessId;
            }
            catch (PlatformNotSupportedException)
            {
                return -1;
            }
        }
    }

    /// <summary>
    /// Created by the IServiceProvider.
    /// </summary>
    /// <param name="loggerFactory"></param>
    public LoggerBaseWrapper(ILoggerFactory loggerFactory, IAddPropertiesToLog addPropertiesToLog)
    {
        _logger = loggerFactory.CreateLogger<T>();
        _category = nameof(MessageCategory.Technical);
        _contextType = typeof(T);
        _addPropertiesToLog = addPropertiesToLog;
    }

    public  ILoggerWrapper<T> SetContext(string category, string caller = "", Type? realType = null)
    {
        _caller = caller;
        _category = category;
        if (realType != null)
        {
            _contextType = realType;
        }
        return this;
    }
    private void Log(LogLevel level, string? message, Exception? exception = null)
    {
        ThrowIfDisposed();

        if (!IsEnabled(level))
        {
            return;
        }

        try
        {
            var properties = AddAdditionalProperties();

            if (null != LoggerContext.Current?.All())
            {
                foreach (var property in LoggerContext.Current.All())
                {
                    properties.AddIfNotExist(property.Key, property.Value);
                }
            }

            if (IncludeStackTrace)
            {
                properties.AddIfNotExist(LoggingConstants.Stacktrace, exception?.StackTrace ?? Environment.StackTrace);
            }

            properties.AddIfNotExist(LoggingConstants.MethodName, _caller);
            properties.AddIfNotExist(LoggingConstants.Class, _contextType?.FullName ?? nameof(_contextType));
            properties.AddIfNotExist(LoggingConstants.Category, _category);
            properties.AddIfNotExist(LoggingConstants.Application, Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown App");
            properties.AddIfNotExist(LoggingConstants.ThreadId, Environment.CurrentManagedThreadId);
            properties.AddIfNotExist(LoggingConstants.ProcessId, ProcessId);

            _logger.Log(level, 0, properties, exception, (state, ex) => message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log message with property providers and eventId");
        }
    }

    internal void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
    }

    private Dictionary<string, object?> AddAdditionalProperties()
    {
        var properties = new Dictionary<string, object?>(AdditionalFields);
        try
        { 
            var definedProperties = _addPropertiesToLog?.GetProperties();
            if (definedProperties != null)
            {
                foreach (var property in definedProperties)
                {
                    if (property.Value != null)
                    {
                        properties.AddIfNotExist(property.Key, property.Value);
                    }
                }
            }
            return properties;
        }
        catch (Exception ex)
        {
            Log(LogLevel.Error, "Error getting property providers. Logging without additional properties.", ex);
            return new Dictionary<string, object?>(AdditionalFields);
        }
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (state is IEnumerable<KeyValuePair<string, object>> pairs)
        {
            foreach (var pair in pairs)
            {
                AdditionalFields.AddOrReplace(pair.Key, pair.Value);
            }
        }

        Log(logLevel, formatter(state, exception), exception);
    }

    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    /// <summary>
    /// Create a disposable scope that can be used to push properties into the scope.
    /// This is not really intended to be used by the end user.
    /// As LoggerWrapper is a wrapper around ILogger, this method must be implemented.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="state"></param>
    /// <returns></returns>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _logger.BeginScope(state);

    /// <summary>
    /// If 
    /// </summary>
    /// <param name="caller"></param>
    public void CallerMemberName(string caller)
    {
        _caller = caller;
    }
}

