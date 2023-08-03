using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Arc4u.Configuration;
using Arc4u.OAuth2.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Extensions;
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
            return;
        }

        var basicSecrets = section.Get<Dictionary<string, SecretBasicSettingsOptions>>();

        if (basicSecrets is null || !basicSecrets.Any())
        {
            return;
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

        // Add default value only for Basic Authentication scenario.
        if (!options.Scopes.Any())
        {
            options.Scopes.Add("openid");
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

        var authorityKey = string.Empty;
        if (options.Authority is not null)
        {
            services.AddAuthority(authOptions =>
            {
                authOptions.SetData(options.Authority.Url, options.Authority.TokenEndpoint, options.Authority.MetaDataAddress);
            }, optionKey);
            authorityKey = optionKey;
        }

        // We map this to a IKeyValuesSettings dictionary.
        // The TokenProviders are based on this.

        void Settings(SimpleKeyValueSettings settings)
        {
            settings.Add(TokenKeys.ProviderIdKey, options!.ProviderId);
            settings.Add(TokenKeys.AuthenticationTypeKey, options.AuthenticationType);
            settings.Add(TokenKeys.ClientIdKey, options.ClientId);
            settings.Add(TokenKeys.Scope, string.Join(' ', options.Scopes));
            settings.AddifNotNullOrEmpty("User", options.User);
            settings.AddifNotNullOrEmpty("Password", options.Password);
            settings.AddifNotNullOrEmpty("Credential", options.Credential);
            settings.AddifNotNullOrEmpty(TokenKeys.AuthorityKey, authorityKey);
            settings.Add("BasicProviderId", options.BasicProviderId);
        }

        services.Configure<SimpleKeyValueSettings>(optionKey, Settings);
    }


}
