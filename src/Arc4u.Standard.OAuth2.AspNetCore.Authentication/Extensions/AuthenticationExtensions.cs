using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Arc4u.Configuration;
using Arc4u.OAuth2.DataProtection;
using Arc4u.OAuth2.Middleware;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.TicketStore;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace Arc4u.OAuth2.Extensions;

public static partial class AuthenticationExtensions
{
    public static AuthenticationBuilder AddOidcAuthentication(this IServiceCollection services, Action<OidcAuthenticationOptions> authenticationOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(authenticationOptions);

        var oidcOptions = new OidcAuthenticationOptions();
        authenticationOptions(oidcOptions);

        if (oidcOptions.OAuth2SettingsOptions is null)
        {
            throw new ArgumentNullException(nameof(authenticationOptions), $"{nameof(oidcOptions.OAuth2SettingsOptions)} is not defined");
        }
        if (oidcOptions.OpenIdSettingsOptions is null)
        {
            throw new ArgumentNullException(nameof(authenticationOptions), $"{nameof(oidcOptions.OpenIdSettingsOptions)} is not defined");
        }

        if (oidcOptions.AuthenticationCacheTicketStoreOption is not null)
        {
            services.AddCacheTicketStore(oidcOptions.AuthenticationCacheTicketStoreOption);
        }

        // We want to protect keys with a certificate, which can be either provided via configuration, or explicitly.
        // It is an error if no certificate was provided either way
        if (oidcOptions.Certificate is null)
        {
            throw new ConfigurationException("No certificate was provided for data protection");
        }

        // The Metadata address is retrieved from the DefaultAuthority!
        ArgumentNullException.ThrowIfNull(oidcOptions.DefaultAuthority.GetMetaDataAddress());
        ArgumentNullException.ThrowIfNull(oidcOptions.DefaultAuthority.MetaDataAddress); // should never be the case!

        services.AddDataProtection()
          .PersistKeysToCache(oidcOptions.DataProtectionCacheStoreOption)
          .ProtectKeysWithCertificate(oidcOptions.Certificate)
          .SetApplicationName(oidcOptions.ApplicationName)
          .SetDefaultKeyLifetime(oidcOptions.DefaultKeyLifetime);

        services.Configure(authenticationOptions);
        services.AddClaimsIdentifier(oidcOptions.ClaimsIdentifierOptions);
        // Will keep in memory the AccessToken and Refresh token for the time of the request...
        services.AddScoped<TokenRefreshInfo>();
        services.AddAuthorizationCore();
        services.AddHttpContextAccessor(); // give access to the HttpContext if requested by an external packages.
        services.AddTransient(oidcOptions.CookieAuthenticationEventsType);
        services.AddTransient(oidcOptions.JwtBearerEventsType);
        services.AddTransient(oidcOptions.OpenIdConnectEventsType);
        services.AddSingleton(typeof(IPostConfigureOptions<CookieAuthenticationOptions>), oidcOptions.CookiesConfigureOptionsType);

        services.AddDefaultAuthority(options =>
        {
            options.SetData(oidcOptions.DefaultAuthority.Url, oidcOptions.DefaultAuthority.TokenEndpoint, oidcOptions.DefaultAuthority.MetaDataAddress);
        });
        // store the configuration => this will be used by the AddCookies to define the ITicketStore implementation.
        services.Configure<OidcAuthenticationOptions>(authenticationOptions);

        services.ConfigureOAuth2Settings(oidcOptions.OAuth2SettingsOptions, oidcOptions.OAuth2SettingsKey);
        services.ConfigureOpenIdSettings(oidcOptions.OpenIdSettingsOptions, oidcOptions.OpenIdSettingsKey);

        var oauth2Options = new OAuth2SettingsOption();
        oidcOptions.OAuth2SettingsOptions(oauth2Options);

        var openIdOptions = new OpenIdSettingsOption();
        oidcOptions.OpenIdSettingsOptions(openIdOptions);

        // OAuth2.
        SecurityKey? securityKey = oidcOptions.CertSecurityKey is not null ? new X509SecurityKey(oidcOptions.CertSecurityKey) : null;

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
                        if (authHeader?.StartsWith("Bearer ", StringComparison.Ordinal) == true)
                        {
                            return JwtBearerDefaults.AuthenticationScheme;
                        }

                        return OpenIdConnectDefaults.AuthenticationScheme;

                    };
                })
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    // cookie will not be limited in time by the life time of the access token.
                    options.UsePkce = true; // Impact on the security. It is best to do this...
                    options.UseTokenLifetime = false;
                    options.SaveTokens = false;
                    options.Authority = openIdOptions.Authority is null ? oidcOptions.DefaultAuthority.Url.ToString() : openIdOptions.Authority.Url.ToString();
                    options.RequireHttpsMetadata = oidcOptions.DefaultAuthority.MetaDataAddress.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.InvariantCultureIgnoreCase);
                    options.MetadataAddress = oidcOptions.DefaultAuthority.MetaDataAddress.ToString();
                    options.ResponseType = oidcOptions.ResponseType;
                    options.CallbackPath = oidcOptions.CallbackPath;
                    options.Scope.Clear();
                    options.Scope.Add(OpenIdConnectScope.OpenIdProfile);
                    options.Scope.Add(OpenIdConnectScope.OfflineAccess);
                    openIdOptions.Scopes.ForEach((scope) => options.Scope.Add(scope));

                    options.ClientId = openIdOptions.ClientId;
                    options.ClientSecret = openIdOptions.ClientSecret;
                    // we don't call the user info endpoint => On AzureAd the user.read scope is needed.
                    options.GetClaimsFromUserInfoEndpoint = false;

                    options.TokenValidationParameters.SaveSigninToken = false;
                    options.TokenValidationParameters.AuthenticationType = openIdOptions.AuthenticationType;
                    options.TokenValidationParameters.ValidateAudience = true;
                    options.TokenValidationParameters.ValidAudiences = openIdOptions.Audiences;

                    // we will use the same key to generate and validate so we can use this also in the different services...
                    if (securityKey is not null)
                    {
                        options.TokenValidationParameters.IssuerSigningKey = securityKey;
                    }
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.SaveTokens = true;
                    options.AuthenticationMethod = OpenIdConnectRedirectBehavior.FormPost;
                    // AzureAD
                    options.ResponseMode = OpenIdConnectResponseMode.FormPost;

                    options.EventsType = oidcOptions.OpenIdConnectEventsType;
                })
                .AddJwtBearer(option =>
                {
                    option.RequireHttpsMetadata = oidcOptions.DefaultAuthority.MetaDataAddress.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.InvariantCultureIgnoreCase);
                    option.Authority = oauth2Options.Authority is null ? oidcOptions.DefaultAuthority.Url.ToString() : oauth2Options.Authority.Url.ToString();
                    option.MetadataAddress = oidcOptions.DefaultAuthority.MetaDataAddress.ToString();
                    option.SaveToken = true;
                    option.TokenValidationParameters.SaveSigninToken = false;
                    option.TokenValidationParameters.AuthenticationType = oauth2Options.AuthenticationType;
                    option.TokenValidationParameters.ValidateIssuer = false;
                    option.TokenValidationParameters.ValidateAudience = true;
                    option.TokenValidationParameters.ValidAudiences = oauth2Options.Audiences;
                    if (securityKey is not null)
                    {
                        option.TokenValidationParameters.IssuerSigningKey = securityKey;
                    }
                    option.EventsType = oidcOptions.JwtBearerEventsType;
                }).AddCookie(); // => by injection!

        return authenticationBuilder;

    }

    public static AuthenticationBuilder AddOidcAuthentication(this IServiceCollection services, IConfiguration configuration, [DisallowNull] string authenticationSectionName = "Authentication", IX509CertificateLoader? certificateLoader = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(authenticationSectionName);

        var section = configuration.GetSection(authenticationSectionName);

        if (section is null || !section.Exists())
        {
            throw new ConfigurationException($"No section exists with name {authenticationSectionName} in the configuration providers for OpenId Connect authentication.");
        }

        var settings = section.Get<OidcAuthenticationSectionOptions>() ?? throw new NullReferenceException($"No section exists with name {authenticationSectionName} in the configuration providers for OpenId Connect authentication.");

        string? configErrors = null;
        if (settings.DefaultAuthority is null)
        {
            configErrors += "DefaultAuthority must be filled!" + System.Environment.NewLine;
        }
        if (string.IsNullOrWhiteSpace(settings.CookieName))
        {
            configErrors += "We need a cookie name defined specifically for your services." + System.Environment.NewLine;
        }
        if (string.IsNullOrWhiteSpace(settings.OpenIdSettingsSectionPath))
        {
            configErrors += "We need a setting section to configure the OpenId Connect." + System.Environment.NewLine;
        }
        if (string.IsNullOrWhiteSpace(settings.OAuth2SettingsSectionPath))
        {
            configErrors += "We need a setting section to configure OAuth2." + System.Environment.NewLine;
        }
        if (string.IsNullOrWhiteSpace(settings.CertificateSectionPath))
        {
            configErrors += "We need a setting section to specify the certificate to protect your sensitive information." + System.Environment.NewLine;
        }
        if (string.IsNullOrWhiteSpace(settings.DataProtectionSectionPath))
        {
            configErrors += "We need a setting section to configure the DataProtection cache store." + System.Environment.NewLine;
        }
        if (string.IsNullOrWhiteSpace(settings.TokenCacheSectionPath))
        {
            configErrors += "We need a setting section to configure the TokenCacheOptions." + System.Environment.NewLine;
        }
        if (string.IsNullOrWhiteSpace(settings.JwtBearerEventsType))
        {
            configErrors += "The JwtBearerEventsType must be defined." + System.Environment.NewLine;
        }
        if (string.IsNullOrWhiteSpace(settings.ClaimsIdentifierSectionPath))
        {
            configErrors += "We need a setting section to specify the claims used to identify a user." + System.Environment.NewLine;
        }
        if (string.IsNullOrWhiteSpace(settings.ClaimsFillerSectionPath))
        {
            configErrors += "We need a setting section to configure the ClaimsFillerOptions." + System.Environment.NewLine;
        }
        if (string.IsNullOrWhiteSpace(settings.DomainMappingsSectionPath))
        {
            configErrors += "We need a setting section to configure the DomainsMapping." + System.Environment.NewLine;
        }

        if (configErrors is not null)
        {
            throw new ConfigurationException(configErrors);
        }

        var jwtBearerEventsType = Type.GetType(settings.JwtBearerEventsType, false);

        if (string.IsNullOrWhiteSpace(settings.CookieAuthenticationEventsType))
        {
            throw new MissingFieldException("The CookieAuthenticationEventsType must be defined.");
        }
        var cookieAuthenticationEventsType = Type.GetType(settings.CookieAuthenticationEventsType, true);

        if (string.IsNullOrWhiteSpace(settings.OpenIdConnectEventsType))
        {
            throw new MissingFieldException("The OpenIdConnectEventsType must be defined.");
        }
        var openIdConnectEventsType = Type.GetType(settings.OpenIdConnectEventsType, false);

        certificateLoader ??= new X509CertificateLoader(null);
        var certSecurityKey = string.IsNullOrWhiteSpace(settings.CertSecurityKeyPath) ? null : certificateLoader.FindCertificate(configuration, settings.CertSecurityKeyPath) ?? throw new MissingFieldException($"No certificate was found based on the configuration section: {settings.CertSecurityKeyPath}.");

        var cert = certificateLoader.FindCertificate(configuration, settings.CertificateSectionPath) ?? throw new MissingFieldException($"No certificate was found based on the configuration section: {settings.CertificateSectionPath}.");

        var ticketStoreAction = CacheTicketStoreExtension.PrepareAction(configuration, settings.AuthenticationCacheTicketStorePath);

        Type? cookiesConfigureOptionsType;
        if (string.IsNullOrWhiteSpace(settings.CookiesConfigureOptionsType))
        {
            cookiesConfigureOptionsType = ticketStoreAction is null ? typeof(ConfigureStandardCookieAuthenticationOptions) : typeof(ConfigureCookieWithTicketStoreAuthenticationOptions);
        }
        else
        {
            cookiesConfigureOptionsType = Type.GetType(settings.CookiesConfigureOptionsType, true);
        }

        if (string.IsNullOrWhiteSpace(settings.ResponseType))
        {
            throw new MissingFieldException("A ResponseType is mandatory to define the OpenId Connect protocol.");
        }

        void OidcAuthenticationFiller(OidcAuthenticationOptions options)
        {
            options.DefaultAuthority = settings.DefaultAuthority!;
            options.CookieName = settings.CookieName;
            options.ValidateAuthority = settings.ValidateAuthority;
            options.AuthenticationCacheTicketStoreOption = ticketStoreAction;
            options.OpenIdSettingsKey = settings.OpenIdSettingsKey;
            options.OpenIdSettingsOptions = OpenIdSettingsExtension.PrepareAction(configuration, settings.OpenIdSettingsSectionPath);
            options.OAuth2SettingsKey = settings.OAuth2SettingsKey;
            options.OAuth2SettingsOptions = OAuth2SettingsExtension.PrepareAction(configuration, settings.OAuth2SettingsSectionPath);
            options.Certificate = cert;
            options.CallbackPath = settings.CallbackPath;
            options.DefaultKeyLifetime = settings.DefaultKeyLifetime;
            options.ApplicationName = configuration[settings.ApplicationNameSectionPath];
            options.JwtBearerEventsType = jwtBearerEventsType;
            options.CookieAuthenticationEventsType = cookieAuthenticationEventsType;
            options.OpenIdConnectEventsType = openIdConnectEventsType;
            options.ForceRefreshTimeoutTimeSpan = settings.ForceRefreshTimeoutTimeSpan;
            options.CertSecurityKey = certSecurityKey;
            options.CookiesConfigureOptionsType = cookiesConfigureOptionsType;
            options.ResponseType = settings.ResponseType;
            options.AuthenticationTicketTTL = settings.AuthenticationTicketTTL;
            options.DataProtectionCacheStoreOption = CacheStoreExtension.PrepareAction(configuration, settings.DataProtectionSectionPath);
            options.ClaimsIdentifierOptions = ClaimsidentifierExtension.PrepareAction(configuration, settings.ClaimsIdentifierSectionPath);
        }

        services.AddDomainMapping(configuration, settings.DomainMappingsSectionPath);
        services.AddOnBehalfOf(configuration);
        services.AddTokenCache(configuration, settings.TokenCacheSectionPath);
        services.AddClaimsFiller(configuration, settings.ClaimsFillerSectionPath);
        services.AddBasicAuthenticationSettings(configuration, settings.BasicAuthenticationSectionPath, certificateLoader, throwExceptionIfSectionDoesntExist: false);
        services.AddOpenIdBearerInjector();

        return services.AddOidcAuthentication(OidcAuthenticationFiller);
    }

    /// <summary>
    /// This extension is used on a API only scenario.
    /// Until the yarp is there and support the Oidc scenario. We don't use this on the Yarp project.
    /// </summary>
    /// <param name="services">The collection ued to define the dependencies</param>
    /// <param name="authenticationOptions"><see cref="JwtAuthenticationBuilderOptions"/></param>
    /// <returns></returns>
    public static AuthenticationBuilder AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration, Action<JwtAuthenticationOptions> authenticationOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(authenticationOptions);

        var options = new JwtAuthenticationOptions();
        authenticationOptions(options);

        if (options.OAuth2SettingsOptions is null)
        {
            throw new ArgumentNullException(nameof(authenticationOptions), $"{nameof(options.OAuth2SettingsOptions)} is empty");
        }

        var oauth2Options = new OAuth2SettingsOption();
        options.OAuth2SettingsOptions(oauth2Options);

        ArgumentNullException.ThrowIfNull(options.JwtBearerEventsType);
        ArgumentNullException.ThrowIfNull(options.DefaultAuthority.GetMetaDataAddress());
        ArgumentNullException.ThrowIfNull(options.DefaultAuthority.MetaDataAddress);

        services.ConfigureOAuth2Settings(options.OAuth2SettingsOptions, options.OAuth2SettingsKey);
        services.AddClaimsIdentifier(options.ClaimsIdentifierOptions);
        services.AddTransient(options.JwtBearerEventsType);
        services.AddAuthorizationCore();
        services.AddHttpContextAccessor();
        services.AddDefaultAuthority(auth =>
        {
            auth.SetData(options.DefaultAuthority.Url, options.DefaultAuthority.TokenEndpoint, options.DefaultAuthority.MetaDataAddress);
        });

        var authenticationBuilder =
        services.AddAuthentication(auth =>
        {
            auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            auth.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
            auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
                .AddJwtBearer(option =>
                {
                    SecurityKey? securityKey = options.CertSecurityKey is not null ? new X509SecurityKey(options.CertSecurityKey) : null;

                    option.RequireHttpsMetadata = options.DefaultAuthority.MetaDataAddress.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.InvariantCultureIgnoreCase);
                    option.Authority = oauth2Options.Authority is null ? options.DefaultAuthority.Url.ToString() : oauth2Options.Authority.Url.ToString();
                    option.MetadataAddress = options.DefaultAuthority.MetaDataAddress.ToString();
                    option.SaveToken = true;
                    option.TokenValidationParameters.SaveSigninToken = false;
                    option.TokenValidationParameters.AuthenticationType = Constants.BearerAuthenticationType;
                    option.TokenValidationParameters.ValidateIssuer = false;
                    option.TokenValidationParameters.ValidateAudience = true;
                    option.TokenValidationParameters.ValidAudiences = oauth2Options.Audiences;
                    if (securityKey is not null)
                    {
                        option.TokenValidationParameters.IssuerSigningKey = securityKey;
                    }
                    option.EventsType = options.JwtBearerEventsType;
                });

        return authenticationBuilder;
    }

    public static AuthenticationBuilder AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration, [DisallowNull] string authenticationSectionName = "Authentication", IX509CertificateLoader? certificateLoader = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(authenticationSectionName);

        var section = configuration.GetSection(authenticationSectionName);

        if (section is null)
        {
            throw new NullReferenceException($"No section exists with name {authenticationSectionName} in the configuration providers for OAuth2 Connect authentication.");
        }

        var settings = section.Get<JwtAuthenticationSectionOptions>();

        if (settings is null)
        {
            throw new NullReferenceException($"No section exists with name {authenticationSectionName} in the configuration providers for OAuth2 Connect authentication.");
        }

        if (settings.DefaultAuthority is null)
        {
            throw new MissingFieldException("DefaultAuthority must be filled!");
        }

        if (string.IsNullOrWhiteSpace(settings.OAuth2SettingsSectionPath))
        {
            throw new MissingFieldException("We need a setting section to configure OAuth2.");
        }
        var jwtBearerEventsType = Type.GetType(settings.JwtBearerEventsType, false);
        if (string.IsNullOrWhiteSpace(settings.JwtBearerEventsType) || jwtBearerEventsType is null)
        {
            throw new MissingFieldException("The JwtBearerEventsType must be defined.");
        }

        X509Certificate2? certSecurityKey;

        if (string.IsNullOrWhiteSpace(settings.CertSecurityKeyPath))
            certSecurityKey = null;
        else
        {
            // we only need a non-null loader in this case
            certificateLoader ??= new X509CertificateLoader(null);
            certSecurityKey = string.IsNullOrWhiteSpace(settings.CertSecurityKeyPath) ? null : certificateLoader.FindCertificate(configuration, settings.CertSecurityKeyPath) ?? throw new MissingFieldException($"No certificate was found based on the configuration section: {settings.CertSecurityKeyPath}.");
        }

        void JwtAuthenticationFiller(JwtAuthenticationOptions options)
        {
            options.DefaultAuthority = settings.DefaultAuthority;
            options.ValidateAuthority = settings.ValidateAuthority;
            options.OAuth2SettingsKey = settings.OAuth2SettingsKey;
            options.OAuth2SettingsOptions = OAuth2SettingsExtension.PrepareAction(configuration, settings.OAuth2SettingsSectionPath);
            options.CertSecurityKey = certSecurityKey;
            options.JwtBearerEventsType = jwtBearerEventsType!;
            options.ClaimsIdentifierOptions = ClaimsidentifierExtension.PrepareAction(configuration, settings.ClaimsIdentifierSectionPath);
        }

        services.AddDomainMapping(configuration, settings.DomainMappingsSectionPath);
        services.AddTokenCache(configuration, settings.TokenCacheSectionPath);
        services.AddClaimsFiller(configuration, settings.ClaimsFillerSectionPath);
        services.AddSecretAuthentication(configuration, settings.ClientSecretSectionPath);
        services.AddRemoteSecretsAuthentication(configuration, settings.RemoteSecretSectionPath);

        return services.AddJwtAuthentication(configuration, JwtAuthenticationFiller);
    }
}
