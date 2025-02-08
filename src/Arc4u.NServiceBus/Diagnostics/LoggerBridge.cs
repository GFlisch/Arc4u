using System.Globalization;
using Arc4u.Diagnostics;
using NServiceBus.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Arc4u.NServiceBus.Diagnostics;

public class LoggerBridge : ILog
{
    public bool IsDebugEnabled => LoggerBase.FilterLevel >= LogLevel.Debug;

    public bool IsInfoEnabled => LoggerBase.FilterLevel >= LogLevel.Information;

    public bool IsWarnEnabled => LoggerBase.FilterLevel >= LogLevel.Warning;

    public bool IsErrorEnabled => LoggerBase.FilterLevel >= LogLevel.Error;

    public bool IsFatalEnabled => LoggerBase.FilterLevel >= LogLevel.Critical;

    public void Debug(string message)
    {
        LoggerBase.Technical.From<LoggerBridge>().Debug(message);
    }

    public void Debug(string message, Exception exception)
    {
        LoggerBase.Technical.From<LoggerBridge>().Debug(message);
        LoggerBase.Technical.From<LoggerBridge>().Exception(exception);
    }

    public void DebugFormat(string format, params object[] args)
    {
        LoggerBase.Technical.From<LoggerBridge>().Debug(string.Format(CultureInfo.InvariantCulture, format, args));
    }

    public void Error(string message)
    {
        LoggerBase.Technical.From<LoggerBridge>().Error(message);
    }

    public void Error(string message, Exception exception)
    {
        LoggerBase.Technical.From<LoggerBridge>().Error(message);
        LoggerBase.Technical.From<LoggerBridge>().Exception(exception);
    }

    public void ErrorFormat(string format, params object[] args)
    {
        LoggerBase.Technical.From<LoggerBridge>().Error(string.Format(CultureInfo.InvariantCulture, format, args));
    }

    public void Fatal(string message)
    {
        LoggerBase.Technical.From<LoggerBridge>().Fatal(message);
    }

    public void Fatal(string message, Exception exception)
    {
        LoggerBase.Technical.From<LoggerBridge>().Fatal(message);
        LoggerBase.Technical.From<LoggerBridge>().Exception(exception);
    }

    public void FatalFormat(string format, params object[] args)
    {
        LoggerBase.Technical.From<LoggerBridge>().Fatal(string.Format(CultureInfo.InvariantCulture, format, args));
    }

    public void Info(string message)
    {
        LoggerBase.Technical.From<LoggerBridge>().Information(message);
    }

    public void Info(string message, Exception exception)
    {
        LoggerBase.Technical.From<LoggerBridge>().Information(message);
        LoggerBase.Technical.From<LoggerBridge>().Exception(exception);
    }

    public void InfoFormat(string format, params object[] args)
    {
        LoggerBase.Technical.From<LoggerBridge>().Information(string.Format(CultureInfo.InvariantCulture, format, args));
    }

    public void Warn(string message)
    {
        LoggerBase.Technical.From<LoggerBridge>().Warning(message);
    }

    public void Warn(string message, Exception exception)
    {
        LoggerBase.Technical.From<LoggerBridge>().Warning(message);
        LoggerBase.Technical.From<LoggerBridge>().Exception(exception);
    }

    public void WarnFormat(string format, params object[] args)
    {
        LoggerBase.Technical.From<LoggerBridge>().Warning(string.Format(CultureInfo.InvariantCulture, format, args));
    }
}
