using Arc4u.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Arc4u.Standard.OAuth2.Middleware;

public static class OpenIdBearerInjectorMiddlewareExtension
{

    public static void AddOpenIdBearerInjector(this IServiceCollection services, Action<OpenIdBearerInjectorOptions> options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        var validate = new OpenIdBearerInjectorOptions();
        options(validate);

        var configErrorsStringBuilder = new System.Text.StringBuilder();
        if (string.IsNullOrEmpty(validate.OnBehalfOfOpenIdSettingsKey))
        {
            configErrorsStringBuilder.AppendLine("The on behalf of settings key must be defined.");
        }

        if (string.IsNullOrEmpty(validate.OboProviderKey))
        {
            configErrorsStringBuilder.AppendLine("The token provider key to handle the on behalf of scenario must be defined.");
        }

        if (configErrorsStringBuilder.Length > 0)
        {
            throw new ConfigurationException(configErrorsStringBuilder.ToString());
        }

        services.Configure<OpenIdBearerInjectorOptions>(options);
    }

    public static void AddOpenIdBearerInjector(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var validate = new OpenIdBearerInjectorOptions();

        var configErrorsStringBuilder = new System.Text.StringBuilder();
        if (string.IsNullOrEmpty(validate.OnBehalfOfOpenIdSettingsKey))
        {
            configErrorsStringBuilder.AppendLine("The on behalf of settings key must be defined.");
        }

        if (string.IsNullOrEmpty(validate.OboProviderKey))
        {
            configErrorsStringBuilder.AppendLine("The token provider key to handle the on behalf of scenario must be defined.");
        }

        if (configErrorsStringBuilder.Length > 0)
        {
            throw new ConfigurationException(configErrorsStringBuilder.ToString());
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
