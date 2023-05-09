using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Arc4u.Configuration;
using Arc4u.OAuth2.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Options;

public static class OnBehalfOfAuthenticationExtensions
{
    public static void AddOnBehalfOf(this IServiceCollection services, Action<OnBehalfOfAuthenticationOptions> options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        // validation.
        var extract = new OnBehalfOfAuthenticationOptions();
        options(extract);

        if (string.IsNullOrWhiteSpace(extract.Authority))
        {
            throw new ConfigurationException("Authority field is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(extract.TokenEndpoint))
        {
            throw new ConfigurationException("TokenEndpoint field is mandatory.");
        }

        services.Configure<OnBehalfOfAuthenticationOptions>(options);
    }

    public static void AddOnBehalfOfSettings(this IServiceCollection services, Action<OnBehalfOfSettingsOptions> options, [DisallowNull] string optionKey)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(optionKey))
        {
            throw new ArgumentNullException(optionKey);
        }

        services.Configure<SimpleKeyValueSettings>(optionKey, BuildSettings(options));
    }

    public static void AddOnBehalfOf(this IServiceCollection services, [DisallowNull] IConfiguration configuration, [DisallowNull] string sectionName = "Authentication:OnBehalfOf")
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(services);

        if (string.IsNullOrWhiteSpace(sectionName))
        {
            throw new ArgumentNullException(sectionName);
        }

        var section = configuration.GetSection(sectionName);

        if (section is null || !section.Exists())
        {
            throw new ConfigurationException($"Section {sectionName} doesn't exist");
        }

        var option = section.Get<OnBehalfOfAuthenticationSectionOptions>();

        if (option is null)
        {
            throw new ConfigurationException($"Section {sectionName} doesn't correspond to the expected format.");
        }

        services.AddOnBehalfOf(oboOptions =>
        {
            oboOptions.Authority = option.Authority;
            oboOptions.TokenEndpoint = option.TokenEndpoint;
        });

        section = configuration.GetSection(option.SettingsPath);

        if (section is null || !section.Exists())
        {
            throw new ConfigurationException($"Section {option.SettingsPath} doesn't exist");
        }

        var options = section.Get<Dictionary<string, OnBehalfOfSettingsOptions>>();

        foreach(var settingsOptions in  options)
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

        var configErrors = string.Empty;
        if (string.IsNullOrWhiteSpace(extract.ClientId))
        {
            configErrors += "ClientId field is not defined." + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(extract.ApplicationKey))
        {
            configErrors += "ApplicationKey field is not defined." + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(extract.Scope))
        {
            configErrors += "Scope field is not defined." + System.Environment.NewLine;
        }

        if (!string.IsNullOrWhiteSpace(configErrors))
        {
            throw new ConfigurationException(configErrors);
        }

        return extract;
    }
}
