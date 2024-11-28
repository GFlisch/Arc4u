using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Arc4u.Diagnostics;

public class MonitoringFromLogger
{
    internal MonitoringFromLogger(ILogger logger)
    {
        _logger = logger;
    }

    private readonly ILogger _logger;

    public MonitoringMessageLogger From<T>([CallerMemberName] string methodName = "")
    {
        return new MonitoringMessageLogger(_logger, typeof(T).FullName ?? throw new InvalidOperationException("Type fullname doesn't exist"), methodName);
    }

    public MonitoringMessageLogger From(object This, [CallerMemberName] string methodName = "")
    {
        return new MonitoringMessageLogger(_logger, This.GetType().FullName ?? throw new InvalidOperationException("Type fullname doesn't exist"), methodName);
    }

    public MonitoringMessageLogger From(Type type, [CallerMemberName] string methodName = "")
    {
        return new MonitoringMessageLogger(_logger, type.FullName ?? throw new InvalidOperationException("Type fullname doesn't exist"), methodName);
    }
}
