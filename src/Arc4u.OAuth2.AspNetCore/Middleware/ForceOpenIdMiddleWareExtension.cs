using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Middleware;
public static class ForceOpenIdMiddleWareOptionsExtension
{
    public static IServiceCollection AddForceOpenId(this IServiceCollection services, Action<ForceOpenIdMiddleWareOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        return services;
    }

    public static IServiceCollection AddForceOpenId(this IServiceCollection services, IConfiguration configuration, string sectionName = "Authentication:ClaimsMiddleWare:ForceOpenId")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<ForceOpenIdMiddleWareOptions>(configuration.GetSection(sectionName));
        return services;
    }

    public static IApplicationBuilder UseForceOpenId(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<ForceOpenIdMiddleWare>(); // Now uses IOptions<T>
    }
}
