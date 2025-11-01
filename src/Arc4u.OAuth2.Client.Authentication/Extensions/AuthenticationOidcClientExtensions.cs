using Arc4u.Configuration;
using Arc4u.OAuth2.Client.Authentication.Options;
using Arc4u.OAuth2.Token;
using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient.Browser;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.Extensions
{
    public static class AuthenticationExtensions
    {
        public static void AddOidcClientAuthentication(this IServiceCollection services, IBrowser browser, ILoggerFactory loggerFactory,
            Action<OidcClientAuthenticationOptions> authenticationOptions, string settingsKey = "OidcClient")
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(authenticationOptions);
            ArgumentNullException.ThrowIfNull(browser);
            ArgumentNullException.ThrowIfNull(loggerFactory);

            var oidcOptions = new OidcClientAuthenticationOptions();
            authenticationOptions(oidcOptions);

            ValidateOidcOptions(oidcOptions);
            ConfigureOidcServices(services, oidcOptions);

            services.Configure(authenticationOptions);
            ConfigureOidcServices(services, oidcOptions);

            var oidcClientSettings = new OidcClientSettingsOption();
            oidcOptions.OidcClientSettingsOption?.Invoke(oidcClientSettings);

            services.ConfigureOidcServices(oidcClientSettings, settingsKey);

            services.AddSingleton(new OidcClientOptions
            {
                Authority = oidcOptions.DefaultAuthority.Url.ToString(),
                Policy = new Policy
                {
                    Discovery = new DiscoveryPolicy
                    {
                        ValidateIssuerName = false,
                        DiscoveryDocumentPath = oidcOptions.DefaultAuthority.GetRelativeMetaDataAddress(),
                        RequireHttps = !oidcOptions.DefaultAuthority.IsLocalHost
                    }
                },
                ClientId = oidcClientSettings.ClientId,
                Scope = string.Join(' ', oidcClientSettings.Scopes),
                RedirectUri = oidcOptions.CallbackPath,
                PostLogoutRedirectUri = oidcOptions.PostLogoutRedirectUri,
                LoadProfile = oidcOptions.LoadProfile,
                Browser = browser,
                ClockSkew = oidcOptions.ClockSkew,
                FilterClaims = false,
                LoggerFactory = loggerFactory,
                TokenClientCredentialStyle = ClientCredentialStyle.PostBody,
                DisablePushedAuthorization = true
            });

            services.AddSingleton<OidcClient>();
        }

        private static void ConfigureOidcServices(this IServiceCollection services, OidcClientSettingsOption oidcClientOptions, string sectionKey = "OidcClient")
        {
            void SettingsFiller(SimpleKeyValueSettings keyOptions)
            {
                keyOptions.Add(TokenKeys.ProviderIdKey, oidcClientOptions.ProviderId);
                keyOptions.Add(TokenKeys.ClientIdKey, oidcClientOptions.ClientId);
                keyOptions.Add(TokenKeys.Scope, string.Join(' ', oidcClientOptions.Scopes));
            }

            services.Configure<SimpleKeyValueSettings>(sectionKey, SettingsFiller);
        }

        private static void ConfigureOidcServices(IServiceCollection services,
            OidcClientAuthenticationOptions oidcClientOptions)
        {
            services.AddClaimsIdentifier(oidcClientOptions.ClaimsIdentifierOptions);
            services.AddSingleton<TokenRefreshInfo>();

            services.AddDefaultAuthority(options =>
            {
                options.SetData(oidcClientOptions.DefaultAuthority.Url,
                                oidcClientOptions.DefaultAuthority.TokenEndpoint,
                                oidcClientOptions.DefaultAuthority.Issuer,
                                oidcClientOptions.DefaultAuthority.MetaDataAddress);
            });
        }

        public static void AddOidcClientAuthentication(this IServiceCollection services, IBrowser browser, ILoggerFactory loggerFactory,
            IConfiguration configuration, string authenticationSectionName = "Authentication", string settingsKey = "OidcClient")
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(authenticationSectionName);

            var section = configuration.GetSection(authenticationSectionName);

            if (!section.Exists())
            {
                throw new ConfigurationException(
                    $"No section exists with name {authenticationSectionName} in the configuration providers for OpenId Connect authentication.");
            }

            // Read the configuration.
            var settings = new OidcClientAuthenticationSectionOptions();
            section.Bind(settings);

            var configErrors = ValidateOidcConfiguration(settings);

            if (configErrors is not null)
            {
                throw new ConfigurationException(configErrors);
            }

            PrepareOidcAuthenticationDependencies(services, configuration, settings);

            void OidcAuthenticationFiller(OidcClientAuthenticationOptions options)
            {
                PopulateFromSection(options, settings, configuration);
            }

            services.AddOidcClientAuthentication(browser, loggerFactory, OidcAuthenticationFiller, settingsKey);
        }

        private static void PrepareOidcAuthenticationDependencies(IServiceCollection services, IConfiguration configuration,
            OidcClientAuthenticationSectionOptions settings)
        {
            services.AddDomainMapping(configuration, settings.DomainMappingsSectionPath);
            services.AddClaimsFiller(configuration, settings.ClaimsFillerSectionPath);
            services.AddAuthenticationApiContext(configuration, settings.ApiExtraContextAuthenticationSection);
        }

        private static void PopulateFromSection(
            OidcClientAuthenticationOptions options,
            OidcClientAuthenticationSectionOptions settings,
            IConfiguration configuration)
        {
            options.DefaultAuthority = settings.DefaultAuthority;
            options.OidcClientSettingsOption =
                OidcClientSettingsExtension.PrepareAction(configuration, settings.OidcClientIdSettingsSectionPath);
            options.CallbackPath = settings.CallbackPath;
            options.PostLogoutRedirectUri = settings.PostLogoutRedirectUri;
            options.LoadProfile = settings.LoadProfile;
            options.ForceRefreshTimeoutTimeSpan = settings.ForceRefreshTimeoutTimeSpan;
            options.RefreshTokenLifetime = settings.RefreshTokenLifetime;
            options.ClaimsIdentifierOptions =
                ClaimsidentifierExtension.PrepareAction(configuration, settings.ClaimsIdentifierSectionPath);
            options.NameClaimType = settings.NameClaimType;
            options.RoleClaimType = settings.RoleClaimType;
        }

        private static string? ValidateOidcConfiguration(OidcClientAuthenticationSectionOptions settings)
        {
            string? configErrors = null;

            if (settings.DefaultAuthority is null)
            {
                configErrors += "DefaultAuthority must be filled!" + System.Environment.NewLine;
            }

            if (string.IsNullOrWhiteSpace(settings.NameClaimType))
            {
                configErrors += "We need a name claim type defined specifically for your services." +
                                System.Environment.NewLine;
            }

            if (string.IsNullOrWhiteSpace(settings.RoleClaimType))
            {
                configErrors += "We need a role claim type defined specifically for your services." +
                                System.Environment.NewLine;
            }

            if (string.IsNullOrWhiteSpace(settings.OidcClientIdSettingsSectionPath))
            {
                configErrors += "We need a setting section to configure the OpenId Connect." +
                                System.Environment.NewLine;
            }

            if (string.IsNullOrWhiteSpace(settings.ClaimsIdentifierSectionPath))
            {
                configErrors += "We need a setting section to specify the claims used to identify a user." +
                                System.Environment.NewLine;
            }

            if (string.IsNullOrWhiteSpace(settings.ClaimsFillerSectionPath))
            {
                configErrors += "We need a setting section to configure the ClaimsFillerOptions." +
                                System.Environment.NewLine;
            }

            if (string.IsNullOrWhiteSpace(settings.DomainMappingsSectionPath))
            {
                configErrors += "We need a setting section to configure the DomainsMapping." +
                                System.Environment.NewLine;
            }

            return configErrors;
        }

        private static void ValidateOidcOptions(OidcClientAuthenticationOptions options)
        {
            ArgumentNullException.ThrowIfNull(options.OidcClientSettingsOption);
            ArgumentNullException.ThrowIfNull(options.DefaultAuthority.GetMetaDataAddress());
        }

    }
}
