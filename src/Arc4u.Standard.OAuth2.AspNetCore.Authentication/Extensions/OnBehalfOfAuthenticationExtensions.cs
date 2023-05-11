using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Extensions;

public static class OnBehalfOfAuthenticationExtensions
{
    public static void AddOnBehalfOfSettings(this IServiceCollection services, Action<OnBehalfOfSettingsOptions> options, [DisallowNull] string optionKey)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(optionKey))
        {
            throw new ArgumentNullException(optionKey);
        }

        services.Configure(optionKey, BuildSettings(options));
    }

    public static void AddOnBehalfOf(this IServiceCollection services, [DisallowNull] IConfiguration configuration, [DisallowNull] string sectionName = "Authentication:OnBehalfOf")
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(services);

        if (string.IsNullOrWhiteSpace(sectionName))
        {
            // No OnBehalfOf are needed into the application!
            return;
        }

        var section = configuration.GetSection(sectionName);

        if (section is null || !section.Exists())
        {
            // No OnBehalfOf are needed into the application!
            return;
        }

        var options = section.Get<Dictionary<string, OnBehalfOfSettingsOptions>>();

        foreach (var settingsOptions in options)
        {
            services.AddOnBehalfOfSettings(oboSettings =>
            {
                oboSettings.ClientId = settingsOptions.Value.ClientId;
                oboSettings.ApplicationKey = settingsOptions.Value.ApplicationKey;
                oboSettings.Scope = settingsOptions.Value.Scope;

            }, settingsOptions.Key);
        }

    }

    private static Action<SimpleKeyValueSettings> BuildSettings(Action<OnBehalfOfSettingsOptions> options)
    {
        var validated = Validate(options);

        void Settings(SimpleKeyValueSettings settings)
        {
            settings.Add(TokenKeys.ClientIdKey, validated.ClientId);
            settings.Add(TokenKeys.Scope, validated.Scope);
            settings.Add(TokenKeys.ApplicationKey, validated.ApplicationKey);
        }

        return Settings;
    }

    private static OnBehalfOfSettingsOptions Validate(Action<OnBehalfOfSettingsOptions> options)
    {
        // validation.
        var extract = new OnBehalfOfSettingsOptions();
        options(extract);

        var configErrorsStringBuilder = new System.Text.StringBuilder();
        if (string.IsNullOrWhiteSpace(extract.ClientId))
        {
            configErrorsStringBuilder.AppendLine("ClientId field is not defined.");
        }

        if (string.IsNullOrWhiteSpace(extract.ApplicationKey))
        {
            configErrorsStringBuilder.AppendLine("ApplicationKey field is not defined.");
        }

        if (string.IsNullOrWhiteSpace(extract.Scope))
        {
            configErrorsStringBuilder.AppendLine("Scope field is not defined.");
        }

        if (configErrorsStringBuilder.Length > 0)
        {
            throw new ConfigurationException(configErrorsStringBuilder.ToString());
        }

        return extract;
    }
}
