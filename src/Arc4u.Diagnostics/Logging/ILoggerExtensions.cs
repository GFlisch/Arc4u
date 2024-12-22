using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arc4u.Diagnostics.Logging;
public static class ILoggerExtensions
{
    public static LoggerWrapper<T> Technical<T>(this ILogger logger, [CallerMemberName] string methodName = "") =>
        new(logger, MessageCategory.Technical, typeof(T), methodName);

    public static LoggerWrapper<T> Business<T>(this ILogger logger, [CallerMemberName] string methodName = "") =>
        new(logger, MessageCategory.Business, typeof(T), methodName);

    public static LoggerWrapper<T> Monitoring<T>(this ILogger logger, [CallerMemberName] string methodName = "") =>
        new(logger, MessageCategory.Monitoring, typeof(T), methodName);

    public static LoggerWrapper<T> Technical<T>(this ILogger<T> logger, [CallerMemberName] string methodName = "") =>
        new(logger, MessageCategory.Technical, typeof(T), methodName);

    public static LoggerWrapper<T> Business<T>(this ILogger<T> logger, [CallerMemberName] string methodName = "") =>
        new(logger, MessageCategory.Business, typeof(T), methodName);

    public static LoggerWrapper<T> Monitoring<T>(this ILogger<T> logger, [CallerMemberName] string methodName = "") =>
        new(logger, MessageCategory.Monitoring, typeof(T), methodName);
}
