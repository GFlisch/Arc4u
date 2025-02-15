using System.Globalization;
using Arc4u.Diagnostics;
using NServiceBus.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using Microsoft.Extensions.Logging;

namespace Arc4u.NServiceBus.Diagnostics;

public class LoggerBridge : ILog
{
    public LoggerBridge(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<LoggerBridge>();
    }

    private readonly ILogger<LoggerBridge> _logger;

    public bool IsDebugEnabled => _logger.IsEnabled(LogLevel.Debug);

    public bool IsInfoEnabled => _logger.IsEnabled(LogLevel.Information);

    public bool IsWarnEnabled => _logger.IsEnabled(LogLevel.Warning);

    public bool IsErrorEnabled => _logger.IsEnabled(LogLevel.Error);

    public bool IsFatalEnabled => _logger.IsEnabled(LogLevel.Critical);

    public void Debug(string message)
    {
       _logger.Technical().LogDebug(message);
    }

    public void Debug(string message, Exception exception)
    {
        _logger.Technical().LogDebug(message);
        _logger.Technical().LogException(exception);
    }

    public void DebugFormat(string format, params object[] args)
    {
        _logger.Technical().LogDebug(string.Format(CultureInfo.InvariantCulture, format, args));
    }

    public void Error(string message)
    {
        _logger.Technical().LogError(message);
    }

    public void Error(string message, Exception exception)
    {
        _logger.Technical().LogError(message);
        _logger.Technical().LogException(exception);
    }

    public void ErrorFormat(string format, params object[] args)
    {
        _logger.Technical().LogError(string.Format(CultureInfo.InvariantCulture, format, args));
    }

    public void Fatal(string message)
    {
        _logger.Technical().LogCritical(message);
    }

    public void Fatal(string message, Exception exception)
    {
        _logger.Technical().LogCritical(message);
        _logger.Technical().LogException(exception);
    }

    public void FatalFormat(string format, params object[] args)
    {
        _logger.Technical().LogCritical(string.Format(CultureInfo.InvariantCulture, format, args));
    }

    public void Info(string message)
    {
        _logger.Technical().LogInformation(message);
    }

    public void Info(string message, Exception exception)
    {
        _logger.Technical().LogInformation(message);
        _logger.Technical().LogException(exception);
    }

    public void InfoFormat(string format, params object[] args)
    {
        _logger.Technical().LogInformation(string.Format(CultureInfo.InvariantCulture, format, args));
    }

    public void Warn(string message)
    {
        _logger.Technical().LogWarning(message);
    }

    public void Warn(string message, Exception exception)
    {
        _logger.Technical().LogWarning(message);
        _logger.Technical().LogException(exception);
    }

    public void WarnFormat(string format, params object[] args)
    {
        _logger.Technical().LogWarning(string.Format(CultureInfo.InvariantCulture, format, args));
    }
}
