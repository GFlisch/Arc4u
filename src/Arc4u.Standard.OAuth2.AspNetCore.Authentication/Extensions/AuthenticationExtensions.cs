using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Arc4u.Configuration;
using Arc4u.OAuth2.DataProtection;
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

        ArgumentNullException.ThrowIfNull(oidcOptions.MetadataAddress);

        services.AddDataProtection()
          .PersistKeysToCache(oidcOptions.DataProtectionCacheStoreOption)
          .ProtectKeysWithCertificate(oidcOptions.Certificate)
          .SetApplicationName(oidcOptions.ApplicationName)
          .SetDefaultKeyLifetime(oidcOptions.DefaultKeyLifetime);

        services.Configure(authenticationOptions);
        services.AddClaimsIdentifier(oidcOptions.ClaimsIdentifierOptions);
        // Will keep in memory the AccessToken and Refresh token for the time of the request...
        services.AddScoped<TokenRefreshInfo>();
        services.AddAuthorization();
        services.AddHttpContextAccessor(); // give access to the HttpContext if requested by an external packages.
        services.AddTransient(oidcOptions.CookieAuthenticationEventsType);
        services.AddTransient(oidcOptions.JwtBearerEventsType);
        services.AddTransient(oidcOptions.OpenIdConnectEventsType);
        services.AddSingleton(typeof(IPostConfigureOptions<CookieAuthenticationOptions>), oidcOptions.CookiesConfigureOptionsType);

        services.AddDefaultAuthority(options =>
        {
            options.Url = oidcOptions.DefaultAuthority.Url;
            options.TokenEndpoint = oidcOptions.DefaultAuthority.TokenEndpoint;
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
                        if (authHeader?.StartsWith("Bearer ") == true)
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
                    options.Authority = openIdOptions.Authority is null ? oidcOptions.DefaultAuthority.Url : openIdOptions.Authority.Url;
                    options.RequireHttpsMetadata = oidcOptions.RequireHttpsMetadata;
                    options.MetadataAddress = oidcOptions.MetadataAddress;
                    options.ResponseType = oidcOptions.ResponseType;
                    options.CallbackPath = oidcOptions.CallbackPath;
                    options.Scope.Clear();
                    options.Scope.Add(OpenIdConnectScope.OpenIdProfile);
                    options.Scope.Add(OpenIdConnectScope.OfflineAccess);
                    foreach (var scope in SplitString(openIdOptions.Scopes))
                    {
                        options.Scope.Add(scope);
                    }

                    options.ClientId = openIdOptions.ClientId;
                    options.ClientSecret = openIdOptions.ClientSecret;
                    // we don't call the user info endpoint => On AzureAd the user.read scope is needed.
                    options.GetClaimsFromUserInfoEndpoint = false;

                    options.TokenValidationParameters.SaveSigninToken = false;
                    options.TokenValidationParameters.AuthenticationType = openIdOptions.AuthenticationType;
                    options.TokenValidationParameters.ValidateAudience = true;
                    options.TokenValidationParameters.ValidAudiences = SplitString(openIdOptions.Audiences);

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
                    option.RequireHttpsMetadata = oidcOptions.RequireHttpsMetadata;
                    option.Authority = oauth2Options.Authority is null ? oidcOptions.DefaultAuthority.Url : oauth2Options.Authority.Url;
                    option.MetadataAddress = oidcOptions.MetadataAddress;
                    option.SaveToken = true;
                    option.TokenValidationParameters.SaveSigninToken = false;
                    option.TokenValidationParameters.AuthenticationType = oauth2Options.AuthenticationType;
                    option.TokenValidationParameters.ValidateIssuer = false;
                    option.TokenValidationParameters.ValidateAudience = true;
                    option.TokenValidationParameters.ValidAudiences = SplitString(oauth2Options.Audiences);
                    if (securityKey is not null)
                    {
                        option.TokenValidationParameters.IssuerSigningKey = securityKey;
                    }
                    option.EventsType = oidcOptions.JwtBearerEventsType;
                }).AddCookie(); // => by injection!

        return authenticationBuilder;

    }

    public static AuthenticationBuilder AddOidcAuthentication(this IServiceCollection services, IConfiguration configuration, [DisallowNull] string authenticationSectionName = "Authentication")
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
        if (string.IsNullOrWhiteSpace(settings.MetadataAddress))
        {
            configErrors += "MetadataAddress must be filled!" + System.Environment.NewLine;
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

        var certSecurityKey = string.IsNullOrWhiteSpace(settings.CertSecurityKeyPath) ? null : new X509CertificateLoader(null).FindCertificate(configuration, settings.CertSecurityKeyPath) ?? throw new MissingFieldException($"No certificate was found based on the configuration section: {settings.CertSecurityKeyPath}.");

        var cert = new X509CertificateLoader(null).FindCertificate(configuration, settings.CertificateSectionPath) ?? throw new MissingFieldException($"No certificate was found based on the configuration section: {settings.CertificateSectionPath}.");

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
            options.DefaultAuthority = settings.DefaultAuthority;
            options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
            options.MetadataAddress = settings!.MetadataAddress;
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
            options.ClaimsIdentifierOptions = ClaimsIdentiferExtension.PrepareAction(configuration, settings.ClaimsIdentifierSectionPath);
        }

        services.AddDomainMapping(configuration, settings.DomainMappingsSectionPath);
        services.AddOnBehalfOf(configuration);
        services.AddTokenCache(configuration, settings.TokenCacheSectionPath);

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
        ArgumentNullException.ThrowIfNull(options.MetadataAddress);

        services.ConfigureOAuth2Settings(options.OAuth2SettingsOptions, options.OAuth2SettingsKey);
        services.AddClaimsIdentifier(options.ClaimsIdentifierOptions);
        services.AddTransient(options.JwtBearerEventsType);
        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddDefaultAuthority(auth =>
        {
            auth.Url = options.DefaultAuthority.Url;
            auth.TokenEndpoint = options.DefaultAuthority.TokenEndpoint;
        });

        var authenticationBuilder =
        services.AddAuthentication(auth => auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(option =>
                {
                    SecurityKey? securityKey = options.CertSecurityKey is not null ? new X509SecurityKey(options.CertSecurityKey) : null;

                    option.RequireHttpsMetadata = options.RequireHttpsMetadata;
                    option.Authority = oauth2Options.Authority is null ? options.DefaultAuthority.Url : oauth2Options.Authority.Url;
                    option.MetadataAddress = options.MetadataAddress;
                    option.SaveToken = true;
                    option.TokenValidationParameters.SaveSigninToken = false;
                    option.TokenValidationParameters.AuthenticationType = Constants.BearerAuthenticationType;
                    option.TokenValidationParameters.ValidateIssuer = false;
                    option.TokenValidationParameters.ValidateAudience = true;
                    option.TokenValidationParameters.ValidAudiences = SplitString(oauth2Options.Audiences);
                    if (securityKey is not null)
                    {
                        option.TokenValidationParameters.IssuerSigningKey = securityKey;
                    }
                    option.EventsType = options.JwtBearerEventsType;
                });

        return authenticationBuilder;
    }

    public static AuthenticationBuilder AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration, [DisallowNull] string authenticationSectionName = "Authentication")
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

        if (string.IsNullOrWhiteSpace(settings.MetadataAddress))
        {
            throw new MissingFieldException("MetadataAddress must be filled!");
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

        var certSecurityKey = string.IsNullOrWhiteSpace(settings.CertSecurityKeyPath) ? null : new X509CertificateLoader(null).FindCertificate(configuration, settings.CertSecurityKeyPath) ?? throw new MissingFieldException($"No certificate was found based on the configuration section: {settings.CertSecurityKeyPath}.");

        void JwtAuthenticationFiller(JwtAuthenticationOptions options)
        {
            options.DefaultAuthority = settings.DefaultAuthority;
            options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
            options.MetadataAddress = settings!.MetadataAddress;
            options.ValidateAuthority = settings.ValidateAuthority;
            options.OAuth2SettingsKey = settings.OAuth2SettingsKey;
            options.OAuth2SettingsOptions = OAuth2SettingsExtension.PrepareAction(configuration, settings.OAuth2SettingsSectionPath);
            options.CertSecurityKey = certSecurityKey;
            options.JwtBearerEventsType = jwtBearerEventsType!;
            options.ClaimsIdentifierOptions = ClaimsIdentiferExtension.PrepareAction(configuration, settings.ClaimsIdentifierSectionPath);
        }

        return services.AddJwtAuthentication(configuration, JwtAuthenticationFiller);

    }

    static string[] SplitString(string value) => value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
}
