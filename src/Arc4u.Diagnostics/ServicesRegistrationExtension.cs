using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using LoggerMessage = Arc4u.Diagnostics.LoggerMessage;

namespace Arc4u.Dependency;

public static class ServicesRegistrationExtension
{
    /// <summary>
    /// Add to the <see cref="IServiceCollection"/> a scoped instance of <see cref="ILogger{LoggerMessage}"/> as ILogger.
    /// This allows for static classes to resolve based on ILogger and via the fluent API, the from() method replaces
    /// the class name.
    /// </summary>
    /// <param name="services"></param>
    /// <returns><see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddILogger(this IServiceCollection services)
    {
        // Add the Arc4u Logger<T> implementation.
        // Scoped is used because the logger now is linked to the authenticated user which is set per scope in the backend.
        services.TryAddScoped(typeof(ILogger<>), typeof(Diagnostics.Logger<>));

        // this injection is to have a ILogger<T> and we will erase the className via the fluent API.
        services.AddScoped<ILogger>((serviceProvider) => serviceProvider.GetRequiredService<ILogger<LoggerMessage>>());

        return services;
    }
}
