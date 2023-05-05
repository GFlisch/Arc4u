using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Arc4u.Configuration;
using Arc4u.OAuth2.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Standard.OAuth2.Extensions;
public static class SecretBasicExtension
{
    public static void AddSecretAuthentication(this IServiceCollection services, [DisallowNull] Action<SecretBasicSettingsOptions> options, [DisallowNull] string optionKey)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(optionKey))
        {
            throw new ArgumentNullException(nameof(optionKey));
        }

        services.Configure<SimpleKeyValueSettings>(optionKey, BuildBasicSettings(options));
    }

    public static void AddSecretAuthentication(this IServiceCollection services, [DisallowNull] IConfiguration configuration, [DisallowNull] string sectionName = "Authentication.ClientSecrets")
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(sectionName);

        var section = configuration.GetSection(sectionName);

        if (section is null || !section.Exists())
        {
            throw new ConfigurationException($"No section in settings file with name {sectionName}.");
        }

        var basicSecrets = section.Get<Dictionary<string, SecretBasicSettingsOptions>>();

        if (basicSecrets is null || !basicSecrets.Any())
        {
            throw new ConfigurationException($"No Basic Secrets are defined in section in settings file with name {sectionName}.");
        }

        foreach (var secret in basicSecrets)
        {
            services.Configure<SimpleKeyValueSettings>(secret.Key, BuildBasicSettings(secret.Value));
        }
    }

    private static Action<SimpleKeyValueSettings> BuildBasicSettings(Action<SecretBasicSettingsOptions> action)
    {
        var options = new SecretBasicSettingsOptions();
        action(options);

        return BuildBasicSettings(options);

    }
    private static Action<SimpleKeyValueSettings> BuildBasicSettings(SecretBasicSettingsOptions options)
    {
        // Check the settings!
        // options mandatory fields!
        string? configErrors = null;
        if (string.IsNullOrWhiteSpace(options.ClientId))
        {
            configErrors += "ClientId in Secret Basic settings must be filled!" + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(options.Authority))
        {
            configErrors += "Authority in Secret Basic settings must be filled!" + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            configErrors += "Audience in Secret Basic settings must be filled!" + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(options.AuthenticationType))
        {
            configErrors += "AuthenticationType in Secret Basic settings must be filled!" + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(options.ProviderId))
        {
            configErrors += "ProviderId in Secret Basic settings must be filled!" + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(options.Scope))
        {
            configErrors += "Scope in Secret Basic settings must be filled!" + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(options.BasicProviderId))
        {
            configErrors += "BasicProviderId in Secret Basic settings must be filled!" + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(options.User) && string.IsNullOrWhiteSpace(options.Password) && string.IsNullOrWhiteSpace(options.Credential))
        {
            configErrors += "User/Password or Credential in Secret Basic settings must be filled!" + System.Environment.NewLine;
        }

        if (!string.IsNullOrWhiteSpace(options.Password) && !string.IsNullOrWhiteSpace(options.Credential))
        {
            configErrors += "Password and Credential in Secret Basic settings cannot be filled at the same time!" + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(options.User) && !string.IsNullOrWhiteSpace(options.Password) && string.IsNullOrWhiteSpace(options.Credential))
        {
            configErrors += "User in Secret Basic settings must be filled when password is used!" + System.Environment.NewLine;
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
            settings.Add(TokenKeys.AuthenticationTypeKey, options.AuthenticationType);
            settings.Add(TokenKeys.AuthorityKey, options.Authority);
            settings.Add(TokenKeys.ClientIdKey, options.ClientId);
            settings.Add(TokenKeys.Audience, options.Audience);
            settings.Add(TokenKeys.Scope, options.Scope);
            settings.Add("User", options.User);
            settings.Add("Password", options.Password);
            settings.Add("Credential", options.Credential);
            settings.Add("BasicProviderId", options.BasicProviderId);
        }

        return Settings;
    }


}
