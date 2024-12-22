using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arc4u.Diagnostics.Logging;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseArc4uLogging(
        this IHostBuilder builder,
        Action<HostBuilderContext, ILoggingBuilder>? configure = null)
    {
        builder.ConfigureLogging((context, loggingBuilder) =>
        {
            var services = loggingBuilder.Services;
            configure?.Invoke(context, loggingBuilder);
        });

        return builder;
    }
}
