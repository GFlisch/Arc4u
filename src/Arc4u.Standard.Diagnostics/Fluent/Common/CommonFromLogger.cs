using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Arc4u.Diagnostics;

public class CommonFromLogger
{
    internal CommonFromLogger(ILogger logger, MessageCategory category)
    {
        _logger = logger;
        Category = category;
    }

    private readonly ILogger _logger;
    private MessageCategory Category { get; set; }

    public CommonMessageLogger From<T>([CallerMemberName] string methodName = "")
    {
        return new CommonMessageLogger(_logger, Category, typeof(T).FullName, methodName);
    }

    public CommonMessageLogger From(object This, [CallerMemberName] string methodName = "")
    {
        return new CommonMessageLogger(_logger, Category, This?.GetType()?.FullName, methodName);
    }

    public CommonMessageLogger From(Type type, [CallerMemberName] string methodName = "")
    {
        return new CommonMessageLogger(_logger, Category, type?.FullName, methodName);
    }
}
