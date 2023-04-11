using System;
using System.Diagnostics.CodeAnalysis;
using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Extensions;
public static class OAuth2SettingsExtension
{
    public static SimpleKeyValueSettings ConfigureOAuth2Settings(this IServiceCollection services, Action<OAuth2SettingsOption> option, [DisallowNull] string sectionKey = "OAuth2")
    {
        ArgumentNullException.ThrowIfNull(sectionKey, nameof(sectionKey));

        var validate = new OAuth2SettingsOption();
        option(validate);

        if (string.IsNullOrWhiteSpace(validate.ProviderId))
        {
            throw new MissingFieldException($"ProviderId field is not defined.");
        }

        if (string.IsNullOrWhiteSpace(validate.Audiences))
        {
            throw new MissingFieldException($"Audiences field is not defined.");
        }

        if (string.IsNullOrWhiteSpace(validate.Authority))
        {
            throw new MissingFieldException($"Authorithy field is not defined.");
        }

        if (string.IsNullOrWhiteSpace(validate.ClientId))
        {
            throw new MissingFieldException($"ClientId field is not defined.");
        }

        if (string.IsNullOrWhiteSpace(validate.AuthenticationType))
        {
            throw new MissingFieldException($"AuthenticationType field is not defined.");
        }

        // We map this to a IKeyValuesSettings dictionary.
        // The TokenProviders are based on this.

        void SettingsFiller(SimpleKeyValueSettings keyOptions)
        {
            keyOptions.Add(TokenKeys.ProviderIdKey, validate!.ProviderId);
            keyOptions.Add(TokenKeys.AuthenticationTypeKey, validate.AuthenticationType);
            keyOptions.Add(TokenKeys.AuthorityKey, validate.Authority);
            keyOptions.Add(TokenKeys.ClientIdKey, validate.ClientId);
            keyOptions.Add(TokenKeys.Audiences, validate.Audiences);
            if (!string.IsNullOrWhiteSpace(validate.Scopes))
            {
                keyOptions.Add(TokenKeys.Scopes, validate.Scopes);
            }
        }


        services.Configure<SimpleKeyValueSettings>(sectionKey, SettingsFiller);

        var settings = new SimpleKeyValueSettings();

        SettingsFiller(settings);

        return settings;

    }

    public static SimpleKeyValueSettings ConfigureOAuth2Settings(this IServiceCollection services, IConfiguration configuration, [DisallowNull] string sectionName, [DisallowNull] string sectionKey = "OAuth2")
    {
        ArgumentNullException.ThrowIfNull(sectionKey, nameof(sectionKey));

        return ConfigureOAuth2Settings(services, PrepareAction(configuration, sectionName), sectionKey);
    }

    internal static Action<OAuth2SettingsOption> PrepareAction(IConfiguration configuration, [DisallowNull] string sectionName)
    {
        if (string.IsNullOrEmpty(sectionName))
        {
            throw new ArgumentNullException(nameof(sectionName));
        }

        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var section = configuration.GetSection(sectionName);

        if (section is null)
        {
            throw new NullReferenceException($"No section exists with name {sectionName}");
        }

        var settings = section.Get<OAuth2SettingsOption>();

        if (settings is null)
        {
            throw new NullReferenceException($"No section exists with name {sectionName}");
        }

        void OptionFiller(OAuth2SettingsOption option)
        {
            option.Authority = settings.Authority;
            option.ClientId = settings.ClientId;
            option.Audiences = settings.Audiences;
            option.AuthenticationType = settings.AuthenticationType;
            option.ProviderId = settings.ProviderId;
            option.Scopes = settings.Scopes;
        }

        return OptionFiller;
    }
}
