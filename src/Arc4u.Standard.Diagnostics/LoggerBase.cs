using Arc4u.Threading;
using Microsoft.Extensions.Logging;

namespace Arc4u.Diagnostics;

/// <summary>
/// The class  is used to log any information on different media.
/// </summary>
public abstract class LoggerBase
{
    /// <summary>
    /// Gets the application name used to log.
    /// </summary>
    /// <value>The application.</value>
    public static string? Application { get; set; }

    private static ILogger? _loggerInstance;
    protected static ILogger LoggerInstance
    {
        get
        {
            if (null != Scope<ILogger>.Current)
            {
                return Scope<ILogger>.Current;
            }

            return _loggerInstance ?? throw new InvalidOperationException("LoggerInstance cannot be null");
        }

        set { _loggerInstance = value; }
    }

    public static CommonFromLogger Technical { get { return new CommonFromLogger(LoggerInstance, MessageCategory.Technical); } }

    public static CommonFromLogger Business { get { return new CommonFromLogger(LoggerInstance, MessageCategory.Business); } }

    public static MonitoringFromLogger Monitoring { get { return new MonitoringFromLogger(LoggerInstance); } }

    public static LogLevel FilterLevel { get; set; }
}
