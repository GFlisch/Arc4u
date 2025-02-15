using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Arc4u.Diagnostics;

public static partial class LoggerDefaults
{
    private static readonly Action<ILogger, string, Exception?> __LogExceptionCallback =
            LoggerMessage.Define<string>(LogLevel.Error, 
                                         new EventId(9000, nameof(LogException)), "Exception: {Message}" , 
                                         new LogDefineOptions() { SkipEnabledCheck = true });

    public static void LogException(this ILogger logger, Exception exception, [CallerMemberName] string caller = "")
    {
        if (logger.IsEnabled(LogLevel.Error))
        {
            if (logger is ILoggerCallerMember loggerTypeInfo)
            {
                loggerTypeInfo.CallerMemberName(caller);
            }
            __LogExceptionCallback(logger, exception.Message, exception);
        }
    }
}
