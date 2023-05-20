using System;
using System.Diagnostics.CodeAnalysis;
using Arc4u.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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

    }

    public static void AddOpenIdBearerInjector(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var validate = new OpenIdBearerInjectorOptions();

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

        var options = app.ApplicationServices.GetRequiredService<IOptions<OpenIdBearerInjectorOptions>>().Value;
        var settings = app.ApplicationServices.GetRequiredService<IOptionsMonitor<SimpleKeyValueSettings>>();

        var middlewareOptions = new OpenIdBearerInjectorSettingsOptions();

        middlewareOptions.OnBehalfOfOpenIdSettings = settings.Get(options.OnBehalfOfOpenIdSettingsKey);
        middlewareOptions.OboProviderKey = options.OboProviderKey;
        middlewareOptions.OpenIdSettings = settings.Get(options.OpenIdSettingsKey);

        return app.UseMiddleware<OpenIdBearerInjectorMiddleware>(middlewareOptions);
    }
}
