using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Arc4u.Diagnostics
{
    public static class ILoggerExtensions
    {
        public static CommonFromLogger Technical(this ILogger logger)
        {
            return new CommonFromLogger(logger, MessageCategory.Technical);
        }

        public static CommonFromLogger Business(this ILogger logger)
        {
            return new CommonFromLogger(logger, MessageCategory.Business);
        }

        public static MonitoringFromLogger Monitoring(this ILogger logger)
        {
            return new MonitoringFromLogger(logger);
        }
        public static CommonMessageLogger Technical<T>(this ILogger<T> logger, [CallerMemberName] string methodName = "")
        {
            return new CommonFromLogger(logger, MessageCategory.Technical).From<T>(methodName);
        }

        public static CommonMessageLogger Business<T>(this ILogger<T> logger, [CallerMemberName] string methodName = "")
        {
            return new CommonFromLogger(logger, MessageCategory.Business).From<T>(methodName);
        }

        public static MonitoringMessageLogger Monitoring<T>(this ILogger<T> logger, [CallerMemberName] string methodName = "")
        {
            return new MonitoringFromLogger(logger).From<T>(methodName);
        }
    }
}
