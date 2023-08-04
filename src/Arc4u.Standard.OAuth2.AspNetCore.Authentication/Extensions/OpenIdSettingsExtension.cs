using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Extensions;
public static class OpenIdSettingsExtension
{
    public static SimpleKeyValueSettings ConfigureOpenIdSettings(this IServiceCollection services, Action<OpenIdSettingsOption> option, [DisallowNull] string sectionKey = "OpenId")
    {
        ArgumentNullException.ThrowIfNull(sectionKey);

        var validate = new OpenIdSettingsOption();
        option(validate);

        if (string.IsNullOrWhiteSpace(validate.ProviderId))
        {
            throw new MissingFieldException($"ProviderId field is not defined.");
        }

        if (!validate.Audiences.Any())
        {
            throw new MissingFieldException($"Audiences field is not defined.");
        }

        if (string.IsNullOrWhiteSpace(validate.ClientId))
        {
            throw new MissingFieldException($"ClientId field is not defined.");
        }

        if (string.IsNullOrWhiteSpace(validate.AuthenticationType))
        {
            throw new MissingFieldException($"AuthenticationType field is not defined.");
        }

        if (!validate.Scopes.Any())
        {
            throw new MissingFieldException($"Scopes field is not defined.");
        }

        void SettingsFiller(SimpleKeyValueSettings keyOptions)
        {
            keyOptions.Add(TokenKeys.ProviderIdKey, validate!.ProviderId);
            keyOptions.Add(TokenKeys.AuthenticationTypeKey, validate.AuthenticationType);

            //Optional => go to default.
            if (validate.Authority is not null)
            {
                keyOptions.Add(TokenKeys.AuthorityKey, Constants.OpenIdOptionsName);
                services.Configure<AuthorityOptions>(Constants.OpenIdOptionsName, options =>
                {
                    options.SetData(validate.Authority.Url, validate.Authority.TokenEndpoint, validate.Authority.MetaDataAddress);
                });
            }

            keyOptions.Add(TokenKeys.ClientIdKey, validate.ClientId);
            keyOptions.Add(TokenKeys.ClientSecret, validate.ClientSecret);
            keyOptions.Add(TokenKeys.Audiences, string.Join(' ', validate.Audiences));
            keyOptions.Add(TokenKeys.Scope, string.Join(' ',validate.Scopes));
        }

        services.Configure<SimpleKeyValueSettings>(sectionKey, SettingsFiller);

        var settings = new SimpleKeyValueSettings();

        SettingsFiller(settings);

        return settings;
    }

    public static SimpleKeyValueSettings ConfigureOpenIdSettings(this IServiceCollection services, IConfiguration configuration, [DisallowNull] string sectionName, [DisallowNull] string sectionKey = Constants.OpenIdOptionsName)
    {
        ArgumentNullException.ThrowIfNull(sectionKey);

        return ConfigureOpenIdSettings(services, PrepareAction(configuration, sectionName), sectionKey);
    }

    internal static Action<OpenIdSettingsOption> PrepareAction(IConfiguration configuration, [DisallowNull] string sectionName)
    {
        if (string.IsNullOrEmpty(sectionName))
        {
            throw new ArgumentNullException(nameof(sectionName));
        }

        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var section = configuration.GetSection(sectionName) ?? throw new NullReferenceException($"No section exists with name {sectionName}");

        var settings = section.Get<OpenIdSettingsOption>() ?? throw new NullReferenceException($"No section exists with name {sectionName}");

        void OptionFiller(OpenIdSettingsOption option)
        {
            option.ClientSecret = settings.ClientSecret;
            option.Authority = settings.Authority;
            option.ClientId = settings.ClientId;
            option.Audiences = settings.Audiences;
            option.Scopes = settings.Scopes;
            option.AuthenticationType = settings.AuthenticationType;
            option.ProviderId = settings.ProviderId;
        }

        return OptionFiller;
    }
}
