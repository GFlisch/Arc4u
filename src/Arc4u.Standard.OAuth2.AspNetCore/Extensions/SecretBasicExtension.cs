using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Arc4u.Configuration;
using Arc4u.OAuth2.Extensions;
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

        Register(services, options, optionKey);
    }

    public static void AddSecretAuthentication(this IServiceCollection services, [DisallowNull] IConfiguration configuration, [DisallowNull] string sectionName = "Authentication:ClientSecrets")
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
            Register(services, secret.Value, secret.Key);
        }
    }

    private static void Register(IServiceCollection services, Action<SecretBasicSettingsOptions> action, [DisallowNull] string optionKey)
    {
        var options = new SecretBasicSettingsOptions();
        action(options);

        Register(services, options, optionKey);

    }


    private static void Register(IServiceCollection services, SecretBasicSettingsOptions options, [DisallowNull] string optionKey)
    {
        // Check the settings!
        // options mandatory fields!
        string? configErrors = null;
        if (string.IsNullOrWhiteSpace(options.ClientId))
        {
            configErrors += "ClientId in Secret Basic settings must be filled!" + System.Environment.NewLine;
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

        if (options.Authority is not null)
        {
            services.AddAuthority(authOptions =>
            {
                authOptions.Url = options.Authority.Url;
                authOptions.TokenEndpoint = options.Authority.TokenEndpoint;
                authOptions.MetaDataAddress = options.Authority.MetaDataAddress;
            }, optionKey);
        }

        // We map this to a IKeyValuesSettings dictionary.
        // The TokenProviders are based on this.

        void Settings(SimpleKeyValueSettings settings)
        {
            settings.Add(TokenKeys.ProviderIdKey, options!.ProviderId);
            settings.Add(TokenKeys.AuthenticationTypeKey, options.AuthenticationType);
            settings.Add(TokenKeys.ClientIdKey, options.ClientId);
            settings.Add(TokenKeys.Scope, options.Scope);
            if (options.Authority is not null)
            {
                // info to retrieve the authority!
                settings.Add(TokenKeys.AuthorityKey, optionKey);
            }
            settings.Add("User", options.User);
            settings.Add("Password", options.Password);
            settings.Add("Credential", options.Credential);
            settings.Add("BasicProviderId", options.BasicProviderId);
        }

        services.Configure<SimpleKeyValueSettings>(optionKey, Settings);
    }


}
