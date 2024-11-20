using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Arc4u.Security.Principal;

public static class ApplicationContextServiceCollectionExtension
{
    /// <summary>
    /// Add the default logger and properties contexted with the scoped instance application context.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddApplicationContext(this IServiceCollection services)
    {
        services.RemoveAll(typeof(ILogger<>));

        // register the logger infrastructure as Scoped.
        services.AddILogger();

        services.TryAddScoped<IAddPropertiesToLog, DefaultLoggingProperties>();
        services.TryAddScoped<IApplicationContext, ApplicationInstanceContext>();
        services.TryAddSingleton<IActivitySourceFactory, DefaultActivitySourceFactory>();

        return services;
    }
}
