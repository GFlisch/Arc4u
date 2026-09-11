using System.Diagnostics.CodeAnalysis;
using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Extensions
{
    public static class OpenIdSettingsExtension
    {
        public static SimpleKeyValueSettings ConfigureOpenIdSettings(this IServiceCollection services, Action<OpenIdSettingsOption> option)
        {
            var validate = new OpenIdSettingsOption();
            option(validate);

            if (string.IsNullOrWhiteSpace(validate.ProviderId))
            {
                throw new MissingFieldException($"ProviderId field is not defined.");
            }

            // if we don't have to validate the audience, we don't need to have any.
            if (validate.ValidateAudience && !validate.Audiences.Any())
            {
                throw new MissingFieldException($"Audiences field is not defined.");
            }

            if (string.IsNullOrWhiteSpace(validate.ClientId))
            {
                throw new MissingFieldException($"ClientId field is not defined.");
            }

            if (!validate.Scopes.Any())
            {
                throw new MissingFieldException($"Scopes field is not defined.");
            }

            void SettingsFiller(SimpleKeyValueSettings keyOptions)
            {
                keyOptions.Add(TokenKeys.ProviderIdKey, validate!.ProviderId);
                keyOptions.Add(TokenKeys.AuthenticationTypeKey, Constants.CookiesAuthenticationType);
                //Optional => go to default.
                if (validate.Authority is not null)
                {
                    keyOptions.Add(TokenKeys.AuthorityKey, Constants.CookiesAuthenticationType);
                    services.AddAuthority(options =>
                    {
                        options.SetData(validate.Authority.Url, validate.Authority.TokenEndpoint, validate.Authority.Issuer, validate.Authority.MetaDataAddress);
                    }, Constants.CookiesAuthenticationType);
                }

                keyOptions.Add(TokenKeys.ClientIdKey, validate.ClientId);
                keyOptions.Add(TokenKeys.ClientSecret, validate.ClientSecret);
                if (validate.ValidateAudience)
                {
                    keyOptions.Add(TokenKeys.Audiences, string.Join(' ', validate.Audiences));
                }
                keyOptions.Add(TokenKeys.Scope, string.Join(' ', validate.Scopes));
            }

            services.Configure<SimpleKeyValueSettings>(Constants.CookiesAuthenticationType, SettingsFiller);

            var settings = new SimpleKeyValueSettings();

            SettingsFiller(settings);

            return settings;
        }

        public static SimpleKeyValueSettings ConfigureOpenIdSettings(this IServiceCollection services, IConfiguration configuration, [DisallowNull] string sectionName)
        {
            return ConfigureOpenIdSettings(services, PrepareAction(configuration, sectionName));
        }

        internal static Action<OpenIdSettingsOption> PrepareAction(IConfiguration configuration, [DisallowNull] string sectionName)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionName);
            ArgumentNullException.ThrowIfNull(configuration);

            var settings = new OpenIdSettingsOption();
            var defaulValidateAudience = settings.ValidateAudience;

            var section = configuration.GetSection(sectionName);

            if (section is not null && section.Exists())
            {
                settings = section.Get<OpenIdSettingsOption>() ?? settings;

                if (section.GetChildren().Any(c => c.Key == nameof(OpenIdSettingsOption.ValidateAudience)))
                {
                    settings.ValidateAudience = section.GetValue<bool>(nameof(OpenIdSettingsOption.ValidateAudience));
                }
                else
                {
                    settings.ValidateAudience = defaulValidateAudience;
                }
            }

            void OptionFiller(OpenIdSettingsOption option)
            {
                option.ClientSecret = settings.ClientSecret;
                option.Authority = settings.Authority;
                option.ClientId = settings.ClientId;
                option.Audiences = settings.Audiences;
                option.Scopes = settings.Scopes;
                option.ProviderId = settings.ProviderId;
                option.ValidateAudience = settings.ValidateAudience;
            }

            return OptionFiller;
        }
    }
}
