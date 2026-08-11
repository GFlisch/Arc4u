using Arc4u.Configuration;
using Arc4u.OAuth2.Events;
using Arc4u.OAuth2.Middleware;
using Arc4u.OAuth2.Options;
using Arc4u.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Net.Http.Headers;

namespace Arc4u.OAuth2.Extensions
{
    public static partial class AuthenticationExtensions
    {
        public static AuthenticationBuilder AddHybridAuthentication(this IServiceCollection services,
            Action<HybridAuthenticationOptions> authenticationOptions)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(authenticationOptions);

            var oidcOptions = new HybridAuthenticationOptions();
            authenticationOptions(oidcOptions);

            ValidateHybridOptions(oidcOptions);

            services.Configure<OidcAuthenticationOptions>(options =>
            {
                options.DefaultAuthority = oidcOptions.DefaultAuthority!;
                options.CookieName = oidcOptions.CookieName;
                options.AuthenticationMethod = oidcOptions.AuthenticationMethod;
                options.AuthenticationCacheTicketStoreOption = oidcOptions.AuthenticationCacheTicketStoreOption!;
                options.OpenIdSettingsKey = oidcOptions.OpenIdSettingsKey;
                options.OpenIdSettingsOptions = oidcOptions.OpenIdSettingsOptions!;
                options.DataProtectionCertificate = oidcOptions.DataProtectionCertificate;
                options.CallbackPath = oidcOptions.CallbackPath;
                options.DefaultKeyLifetime = oidcOptions.DefaultKeyLifetime;
                options.ApplicationName = oidcOptions.ApplicationName;
                options.ForceRefreshTimeoutTimeSpan = oidcOptions.ForceRefreshTimeoutTimeSpan;
                options.RefreshTokenLifetime = oidcOptions.RefreshTokenLifetime;
                options.CertSecurityKey = oidcOptions.CertSecurityKey;
                options.ResponseType = oidcOptions.ResponseType;
                options.AuthenticationTicketTtl = oidcOptions.AuthenticationTicketTtl;
                options.DataProtectionCacheStoreOption = oidcOptions.DataProtectionCacheStoreOption!;
                options.ClaimsIdentifierOptions = oidcOptions.ClaimsIdentifierOptions!;
                options.NameClaimType = oidcOptions.NameClaimType;
                options.RoleClaimType = oidcOptions.RoleClaimType;
            });
            var openIdOptions = ConfigureOidcServices(services, oidcOptions, out var securityKey);
            services.TryAddTransient<JwtBearerEvents, StandardBearerEvents>();

            services.ConfigureOAuth2Settings(oidcOptions.OAuth2SettingsOptions, oidcOptions.OAuth2SettingsKey);

            var oauth2Options = new OAuth2SettingsOption();
            oidcOptions.OAuth2SettingsOptions(oauth2Options);

            var authenticationBuilder = services
                .AddAuthentication(auth =>
                {
                    auth.DefaultAuthenticateScheme = Constants.ChallengePolicyScheme;
                    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    auth.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddPolicyScheme(Constants.ChallengePolicyScheme, "Authorization Bearer or OIDC", options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        var authHeader = context.Request.Headers[HeaderNames.Authorization].FirstOrDefault();
                        return authHeader?.StartsWith("Bearer ", StringComparison.Ordinal) == true ? JwtBearerDefaults.AuthenticationScheme : OpenIdConnectDefaults.AuthenticationScheme;
                    };
                })
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme,
                    options =>
                    {
                        ConfigureOpenIdConnectOptions(services, options, oidcOptions, openIdOptions, securityKey);
                    })
                .AddJwtBearer(option =>
                {
                    ConfigureJwtBearerOptions(services, option, oidcOptions, oauth2Options, securityKey);
                }).AddCookie();

            services.AddAuthenticationApiContext(_ => { });

            return authenticationBuilder;
        }

        public static AuthenticationBuilder AddHybridAuthentication(this IServiceCollection services,
            IConfiguration configuration, string authenticationSectionName = "Authentication",
            IX509CertificateLoader? certificateLoader = null)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(authenticationSectionName);

            var section = configuration.GetSection(authenticationSectionName);

            if (!section.Exists())
            {
                throw new ConfigurationException(
                    $"No section exists with name {authenticationSectionName} in the configuration providers for OpenId Connect authentication.");
            }

            var settings = new HybridAuthenticationSectionOptions();
            section.Bind(settings);

            var configErrors = ValidateHybridConfiguration(settings);

            if (configErrors is not null)
            {
                throw new ConfigurationException(configErrors);
            }

            var (dataProtectionCertificate, certSecurityKey, ticketStoreAction)  = PrepareOidcAuthenticationDependencies(services, configuration, certificateLoader, settings);

            services.AddBasicAuthenticationSettings(configuration, settings.BasicAuthenticationSectionPath,
                certificateLoader, throwExceptionIfSectionDoesntExist: false);

            void HybridAuthenticationFiller(HybridAuthenticationOptions options)
            {
                options.OAuth2SettingsKey = settings.OAuth2SettingsKey;
                options.OAuth2SettingsOptions =
                    OAuth2SettingsExtension.PrepareAction(configuration, settings.OAuth2SettingsSectionPath);

                OidcAuthenticationFiller(options);
            }

            void OidcAuthenticationFiller(OidcAuthenticationOptions options)
            {
                PopulateFromSection(options, settings, configuration, dataProtectionCertificate, certSecurityKey, ticketStoreAction);
            }

            services.AddAuthenticationApiContext(configuration);

            return services.AddHybridAuthentication(HybridAuthenticationFiller);
        }
        private static string? ValidateHybridConfiguration(HybridAuthenticationSectionOptions settings)
        {
            string? configErrors = null;

            if (string.IsNullOrWhiteSpace(settings.OAuth2SettingsSectionPath))
            {
                configErrors += "We need a setting section to configure OAuth2." + System.Environment.NewLine;
            }
            var oidcConfigErrors = ValidateOidcConfiguration(settings);

            return (configErrors is null && oidcConfigErrors is null) ? null : configErrors + oidcConfigErrors;
        }
        private static void ValidateHybridOptions(HybridAuthenticationOptions options)
        {
            ArgumentNullException.ThrowIfNull(options.OAuth2SettingsOptions);

            ValidateOidcOptions(options);
        }
    }
}
