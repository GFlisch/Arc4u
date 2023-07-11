using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Arc4u.OAuth2.Middleware;

public static class ValidateResourcesRightMiddlewareExtension
{
    public static IApplicationBuilder UseResourcesRightValidationFor(this IApplicationBuilder app, Action<ValidateResourcesRightMiddlewareOptions> options)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(options);

        var validate = new ValidateResourcesRightMiddlewareOptions();
        options(validate);

        string? configErrors = null;

        if (validate.ResourcesPolicies is null || validate.ResourcesPolicies.Count == 0)
        {
            return app; // do not use the middleware.
        }
        else
        {
            foreach (var option in validate.ResourcesPolicies)
            {
                if (string.IsNullOrWhiteSpace(option.Key))
                {
                    configErrors += "Key must be filled!" + System.Environment.NewLine;
                }

                if (string.IsNullOrWhiteSpace(option.Value.Path))
                {
                    configErrors += "Path must be filled!" + System.Environment.NewLine;
                }

                if (string.IsNullOrWhiteSpace(option.Value.AuthorizationPolicy))
                {
                    configErrors += "AuthorizationPolicy must be filled!" + System.Environment.NewLine;
                }

                if (string.IsNullOrWhiteSpace(option.Value.ContentToDisplay))
                {
                    option.Value.ContentToDisplay = validate.DefaultContent;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(validate.DefaultContent))
        {
            configErrors = "Default message content must be defined!" + System.Environment.NewLine;
        }

        if (configErrors is not null)
        {
            throw new ConfigurationException(configErrors);
        }

        return app.UseMiddleware<ValidateResourcesRightMiddleware>(validate);
    }

    public static IApplicationBuilder UseResourcesRightValidationFor(this IApplicationBuilder app, string sectionName = "Authentication:ResourcesRights")
    {
        ArgumentNullException.ThrowIfNull(app);

        if (string.IsNullOrWhiteSpace(sectionName))
        {
            throw new ArgumentNullException(nameof(sectionName));
        }

        var section = app.ApplicationServices.GetRequiredService<IConfiguration>().GetSection(sectionName);

        if (section is null && !section.Exists())
        {
            throw new ConfigurationException($"Section {sectionName} does not exist!");
        }

        app.UseResourcesRightValidationFor(options =>
        {
            var resources = section.Get<ValidateResourcesRightMiddlewareOptions>();

            options.DefaultContent = resources.DefaultContent;
            options.ResourcesPolicies = resources.ResourcesPolicies;
        });

        return app;
    }
}

