using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Arc4u.Diagnostics;
public static class ILoggerExtensions
{
    public static ILoggerWrapper<T> Technical<T>(this ILogger<T> logger, [CallerMemberName] string methodName = "") =>
        logger is ILoggerWrapper<T> loggerWraper ? loggerWraper.SetContext(nameof(MessageCategory.Technical), methodName) : throw new InvalidOperationException("Bad Arc4u usage.");

    public static ILoggerWrapper<T> Business<T>(this ILogger<T> logger, [CallerMemberName] string methodName = "") =>
        logger is ILoggerWrapper<T> loggerWraper ? loggerWraper.SetContext(nameof(MessageCategory.Business), methodName) : throw new InvalidOperationException("Bad Arc4u usage.");

    public static ILoggerWrapper<T> Monitoring<T>(this ILogger<T> logger, [CallerMemberName] string methodName = "") =>
        logger is ILoggerWrapper<T> loggerWraper ? loggerWraper.SetContext(nameof(MessageCategory.Monitoring), methodName) : throw new InvalidOperationException("Bad Arc4u usage.");

    public static ILoggerWrapper<DefaultLogger> Technical(this ILogger logger, Type specificType, [CallerMemberName] string methodName = "") =>
        logger is ILoggerWrapper<DefaultLogger> loggerWraper ? loggerWraper.SetContext(nameof(MessageCategory.Technical), methodName, specificType) : throw new InvalidOperationException("Bad Arc4u usage.");

    public static ILoggerWrapper<DefaultLogger> Technical<T>(this ILogger logger, [CallerMemberName] string methodName = "") =>
        logger is ILoggerWrapper<DefaultLogger> loggerWraper ? loggerWraper.SetContext(nameof(MessageCategory.Technical), methodName, typeof(T)) : throw new InvalidOperationException("Bad Arc4u usage.");

    public static ILoggerWrapper<DefaultLogger> Monitoring(this ILogger logger, Type specificType, [CallerMemberName] string methodName = "") =>
        logger is ILoggerWrapper<DefaultLogger> loggerWraper ? loggerWraper.SetContext(nameof(MessageCategory.Monitoring), methodName, specificType) : throw new InvalidOperationException("Bad Arc4u usage.");

}
