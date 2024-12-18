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
        if (null == options)
        {
            return;
        }

        foreach (var settingsOptions in options)
        {
            services.AddOnBehalfOfSettings(oboSettings =>
            {
                oboSettings.ClientId = settingsOptions.Value.ClientId;
                oboSettings.ClientSecret = settingsOptions.Value.ClientSecret;
                oboSettings.Scopes = settingsOptions.Value.Scopes;
                oboSettings.AuthenticationType = settingsOptions.Value.AuthenticationType;
                oboSettings.ProviderId = settingsOptions.Value.ProviderId;
            }, settingsOptions.Key);
        }

    }

    private static Action<SimpleKeyValueSettings> BuildSettings(Action<OnBehalfOfSettingsOptions> options)
    {
        var validated = Validate(options);

        void Settings(SimpleKeyValueSettings settings)
        {
            settings.Add(TokenKeys.ClientIdKey, validated.ClientId);
            settings.Add(TokenKeys.Scope, string.Join(' ', validated.Scopes));
            settings.Add(TokenKeys.ClientSecret, validated.ClientSecret);
            settings.Add(TokenKeys.AuthenticationTypeKey, validated.AuthenticationType);
            settings.Add(TokenKeys.ProviderIdKey, validated.ProviderId);
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

        if (string.IsNullOrWhiteSpace(extract.ClientSecret))
        {
            configErrors += "ClientSecret field is not defined." + System.Environment.NewLine;
        }

        if (!extract.Scopes.Any())
        {
            configErrors += "Scope field is not defined." + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(extract.AuthenticationType))
        {
            configErrors += "AuthenticationType field is not defined." + System.Environment.NewLine;
        }

        if (!string.IsNullOrWhiteSpace(configErrors))
        {
            throw new ConfigurationException(configErrors);
        }

        return extract;
    }
}
