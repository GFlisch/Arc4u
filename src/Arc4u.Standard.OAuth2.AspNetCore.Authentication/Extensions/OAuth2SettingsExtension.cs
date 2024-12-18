using System.Diagnostics.CodeAnalysis;
using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Extensions;
public static class OAuth2SettingsExtension
{
    public static SimpleKeyValueSettings ConfigureOAuth2Settings(this IServiceCollection services, Action<OAuth2SettingsOption> option, [DisallowNull] string sectionKey = Constants.OAuth2OptionsName)
    {
        ArgumentNullException.ThrowIfNull(sectionKey);

        var validate = new OAuth2SettingsOption();
        option(validate);
        string? configErrors = null;

        if (string.IsNullOrWhiteSpace(validate.ProviderId))
        {
            configErrors += "ProviderId field is not defined." + System.Environment.NewLine;
        }

        if (validate.ValidateAudience && !validate.Audiences.Any())
        {
            configErrors += "Audiences field is not defined." + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(validate.AuthenticationType))
        {
            configErrors += "AuthenticationType field is not defined." + System.Environment.NewLine;
        }

        if (configErrors is not null)
        {
            throw new ConfigurationException(configErrors);
        }

        // We map this to a IKeyValuesSettings dictionary.
        // The TokenProviders are based on 

        void SettingsFiller(SimpleKeyValueSettings keyOptions)
        {
            keyOptions.Add(TokenKeys.ProviderIdKey, validate!.ProviderId);
            keyOptions.Add(TokenKeys.AuthenticationTypeKey, validate.AuthenticationType);

            //Optional => go to default.
            if (validate.Authority is not null)
            {
                keyOptions.Add(TokenKeys.AuthorityKey, Constants.OAuth2OptionsName);
                services.Configure<AuthorityOptions>(Constants.OAuth2OptionsName, options =>
                {
                    options.SetData(validate.Authority.Url, validate.Authority.TokenEndpoint, validate.Authority.MetaDataAddress);
                });
            }
            // Build the list of Audiences as a string.
            keyOptions.Add(TokenKeys.Audiences, string.Join(' ', validate.Audiences));
            keyOptions.Add(TokenKeys.Scope, string.Join(' ', validate.Scopes));

        }

        services.Configure<SimpleKeyValueSettings>(sectionKey, SettingsFiller);

        var settings = new SimpleKeyValueSettings();

        SettingsFiller(settings);

        return settings;

    }

    public static SimpleKeyValueSettings ConfigureOAuth2Settings(this IServiceCollection services, IConfiguration configuration, [DisallowNull] string sectionName, [DisallowNull] string sectionKey = "OAuth2")
    {
        ArgumentNullException.ThrowIfNull(sectionKey);

        return ConfigureOAuth2Settings(services, PrepareAction(configuration, sectionName), sectionKey);
    }

    internal static Action<OAuth2SettingsOption> PrepareAction(IConfiguration configuration, [DisallowNull] string sectionName)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionName);
        ArgumentNullException.ThrowIfNull(configuration);

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
            option.Audiences = settings.Audiences;
            option.AuthenticationType = settings.AuthenticationType;
            option.ProviderId = settings.ProviderId;
            option.Scopes = settings.Scopes;
            option.ValidateAudience = settings.ValidateAudience;
        }

        return OptionFiller;
    }
}
