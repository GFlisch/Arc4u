using System.Diagnostics.CodeAnalysis;
using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Middleware;

public static class OpenIdBearerInjectorMiddlewareExtension
{
    public static void AddOpenIdBearerInjector(this IServiceCollection services, Action<OpenIdBearerInjectorOptions> options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        var validate = new OpenIdBearerInjectorOptions();
        options(validate);

        string? configErrors = null;
        if (string.IsNullOrEmpty(validate.OnBehalfOfOpenIdSettingsKey))
        {
            configErrors += "The on behalf of settings key must be defined." + System.Environment.NewLine;
        }

        if (string.IsNullOrEmpty(validate.OboProviderKey))
        {
            configErrors += "The token provider key to handle the on behalf of scenario must be defined." + System.Environment.NewLine;
        }

        if (configErrors is not null)
        {
            throw new ConfigurationException(configErrors);
        }

        services.Configure<OpenIdBearerInjectorOptions>(options);
        // Let a customer change the behavior by injecting his/her implementation.
        services.TryAddSingleton<IPostConfigureOptions<OpenIdBearerInjectorSettingsOptions>, PostConfigureOpenIdBearerInjectorSettings>();
    }

    public static void AddOpenIdBearerInjector(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var validate = new OpenIdBearerInjectorOptions();

        services.AddOpenIdBearerInjector(options =>
        {
            options.OboProviderKey = validate.OboProviderKey;
            options.OnBehalfOfOpenIdSettingsKey = validate.OnBehalfOfOpenIdSettingsKey;
            options.OpenIdSettingsKey = validate.OpenIdSettingsKey;
        });

    }
    public static IApplicationBuilder UseOpenIdBearerInjector([DisallowNull] this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<OpenIdBearerInjectorMiddleware>();
    }
}
