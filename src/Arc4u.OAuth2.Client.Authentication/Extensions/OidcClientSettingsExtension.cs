using System.Diagnostics.CodeAnalysis;
using Arc4u.OAuth2.Client.Authentication.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Extensions
{
    public static class OidcClientSettingsExtension
    {
        public static void ValidateOIdcClientSettings(this IServiceCollection services, Action<OidcClientSettingsOption> option)
        {
            var validate = new OidcClientSettingsOption();
            option(validate);

            if (string.IsNullOrWhiteSpace(validate.ProviderId))
            {
                throw new MissingFieldException($"ProviderId field is not defined.");
            }

            if (string.IsNullOrWhiteSpace(validate.ClientId))
            {
                throw new MissingFieldException($"ClientId field is not defined.");
            }

            if (!validate.Scopes.Any())
            {
                throw new MissingFieldException($"Scopes field is not defined.");
            }
        }

        internal static Action<OidcClientSettingsOption> PrepareAction(IConfiguration configuration, string sectionName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);
            ArgumentNullException.ThrowIfNull(configuration);

            var settings = new OidcClientSettingsOption();

            var section = configuration.GetSection(sectionName);

            if (section.Exists())
            {
                settings = section.Get<OidcClientSettingsOption>() ?? settings;
            }

            void OptionFiller(OidcClientSettingsOption option)
            {
                option.ClientId = settings.ClientId;
                option.Scopes = settings.Scopes;
                option.ProviderId = settings.ProviderId;
            }

            return OptionFiller;
        }
    }
}
