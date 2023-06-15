using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Extensions;
public static class RemoteSecretsExtension
{
    public static void AddRemoteSecretsAuthentication(this IServiceCollection services, Action<RemoteSecretSettingsOptions> options, [DisallowNull] string optionKey)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(optionKey))
        {
            throw new ArgumentNullException(nameof(optionKey));
        }

        services.Configure<SimpleKeyValueSettings>(optionKey, BuildRemoteSecretsSettings(options));
    }

    public static void AddRemoteSecretsAuthentication(this IServiceCollection services, [DisallowNull] IConfiguration configuration, [DisallowNull] string sectionName = "Authentication:RemoteSecrets")
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(sectionName);

        var section = configuration.GetSection(sectionName);

        if (section is null || !section.Exists())
        {
            return;
        }

        var remoteSecrets = section.Get<Dictionary<string, RemoteSecretSettingsOptions>>();

        if (remoteSecrets is null || !remoteSecrets.Any())
        {
            return;
        }

        foreach (var secret in remoteSecrets)
        {
            services.Configure<SimpleKeyValueSettings>(secret.Key, BuildRemoteSecretsSettings(secret.Value));
        }
    }

    private static Action<SimpleKeyValueSettings> BuildRemoteSecretsSettings(Action<RemoteSecretSettingsOptions> action)
    {
        var options = new RemoteSecretSettingsOptions();
        action(options);

        return BuildRemoteSecretsSettings(options);

    }

    private static Action<SimpleKeyValueSettings> BuildRemoteSecretsSettings(RemoteSecretSettingsOptions options)
    {
        // Check the settings!
        // options mandatory fields!
        string? configErrors = null;
        if (string.IsNullOrWhiteSpace(options.HeaderKey))
        {
            configErrors += "HeaaderKey in Remote Secret settings must be filled!" + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(options.ClientSecret))
        {
            configErrors += "ClientSecret in Remote Secret settings must be filled!" + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(options.ProviderId))
        {
            configErrors += "ProviderId in Remote Secret settings must be filled!" + System.Environment.NewLine;
        }

        if (configErrors is not null)
        {
            throw new ConfigurationException(configErrors);
        }

        // We map this to a IKeyValuesSettings dictionary.
        // The TokenProviders are based on this.

        void Settings(SimpleKeyValueSettings settings)
        {
            settings.Add(TokenKeys.ProviderIdKey, options!.ProviderId);
            settings.Add(TokenKeys.ClientSecretHeader, options.HeaderKey);
            settings.Add(TokenKeys.ClientSecret, options.ClientSecret);
        }

        return Settings;
    }

}
