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
        LoggerBase.Technical.From<LoggerBridge>().Debug(message).Log();
    }

    public void Debug(string message, Exception exception)
    {
        LoggerBase.Technical.From<LoggerBridge>().Debug(message).Log();
        LoggerBase.Technical.From<LoggerBridge>().Exception(exception).Log();
    }

    public void DebugFormat(string format, params object[] args)
    {
        LoggerBase.Technical.From<LoggerBridge>().Debug(string.Format(CultureInfo.InvariantCulture ,format, args)).Log();
    }

    public void Error(string message)
    {
        LoggerBase.Technical.From<LoggerBridge>().Error(message).Log();
    }

    public void Error(string message, Exception exception)
    {
        LoggerBase.Technical.From<LoggerBridge>().Error(message).Log();
        LoggerBase.Technical.From<LoggerBridge>().Exception(exception).Log();
    }

    public void ErrorFormat(string format, params object[] args)
    {
        LoggerBase.Technical.From<LoggerBridge>().Error(string.Format(CultureInfo.InvariantCulture, format, args)).Log();
    }

    public void Fatal(string message)
    {
        LoggerBase.Technical.From<LoggerBridge>().Fatal(message).Log();
    }

    public void Fatal(string message, Exception exception)
    {
        LoggerBase.Technical.From<LoggerBridge>().Fatal(message).Log();
        LoggerBase.Technical.From<LoggerBridge>().Exception(exception).Log();
    }

    public void FatalFormat(string format, params object[] args)
    {
        LoggerBase.Technical.From<LoggerBridge>().Fatal(string.Format(CultureInfo.InvariantCulture, format, args)).Log();
    }

    public void Info(string message)
    {
        LoggerBase.Technical.From<LoggerBridge>().Information(message).Log();
    }

    public void Info(string message, Exception exception)
    {
        LoggerBase.Technical.From<LoggerBridge>().Information(message).Log();
        LoggerBase.Technical.From<LoggerBridge>().Exception(exception).Log();
    }

    public void InfoFormat(string format, params object[] args)
    {
        LoggerBase.Technical.From<LoggerBridge>().Information(string.Format(CultureInfo.InvariantCulture, format, args)).Log();
    }

    public void Warn(string message)
    {
        LoggerBase.Technical.From<LoggerBridge>().Warning(message).Log();
    }

    public void Warn(string message, Exception exception)
    {
        LoggerBase.Technical.From<LoggerBridge>().Warning(message).Log();
        LoggerBase.Technical.From<LoggerBridge>().Exception(exception).Log();
    }

    public void WarnFormat(string format, params object[] args)
    {
        LoggerBase.Technical.From<LoggerBridge>().Warning(string.Format(CultureInfo.InvariantCulture, format, args)).Log();
    }
}
