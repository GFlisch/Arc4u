using NServiceBus.Logging;

namespace Arc4u.NServiceBus.Diagnostics;

public class LoggerFactory : ILoggerFactory
{

    public void SetILoggerFactory(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    private Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;
    public ILog GetLogger(Type type)
    {
        return GetLogger(type.FullName!);
    }

    public ILog GetLogger(string name)
    {
        return new LoggerBridge(_loggerFactory);
    }
}
