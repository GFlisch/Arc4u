using NServiceBus.Logging;

namespace Arc4u.NServiceBus.Diagnostics;

/// <summary>
/// Bring Arc4u logger to NServiceBus.
/// </summary>
public class Arc4uLogger : LoggingFactoryDefinition
{
    protected override ILoggerFactory GetLoggingFactory()
    {
        return new LoggerFactory();
    }
}
