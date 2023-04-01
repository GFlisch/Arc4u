using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Arc4u.OAuth2.DataProtection;
using Arc4u.OAuth2.Extensions;
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

namespace Arc4u.Standard.OAuth2.Extensions;

public static partial class AuthenticationExtensions
{
    public static AuthenticationBuilder AddOidcAuthentication(this IServiceCollection services, IConfiguration configuration, Action<OidcAuthenticationOptions> authenticationOptions)
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

        ArgumentNullException.ThrowIfNull(oidcOptions.MetadataAddress);

        services.AddDataProtection()
          .PersistKeysToCache(oidcOptions.DataProtectionCacheStoreOption)
          .ProtectKeysWithCertificate(oidcOptions.Certificate)
          .SetApplicationName(oidcOptions.ApplicationName)
          .SetDefaultKeyLifetime(oidcOptions.DefaultKeyLifetime);

        services.AddCacheTicketStore(oidcOptions.AuthenticationCacheTicketStoreOption);


        // Will keep in memory the AccessToken and Refresh token for the time of the request...
        services.Configure(authenticationOptions);
        services.AddScoped<TokenRefreshInfo>();
        services.AddAuthorization();
        services.AddHttpContextAccessor(); // give access to the HttpContext if requested by an external packages.
        services.AddTransient(oidcOptions.CookieAuthenticationEventsType);
        services.AddTransient(oidcOptions.JwtBearerEventsType);
        services.AddTransient(oidcOptions.OpenIdConnectEventsType);
        services.AddSingleton(typeof(IPostConfigureOptions<CookieAuthenticationOptions>), oidcOptions.CookiesConfigureOptionsType);

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
                    options.UsePkce = true; // Impact on th security. It is best to do this...
                    options.UseTokenLifetime = false;
                    options.SaveTokens = false;
                    options.Authority = openIdOptions.Authority;
                    options.RequireHttpsMetadata = oidcOptions.RequireHttpsMetadata;
                    options.MetadataAddress = oidcOptions.MetadataAddress;
                    options.ResponseType = oidcOptions.ResponseType;

                    options.Scope.Clear();
                    options.Scope.Add(OpenIdConnectScope.OpenIdProfile);
                    options.Scope.Add(OpenIdConnectScope.OfflineAccess);
                    foreach (var scope in SplitString(openIdOptions.Scopes))
                    {
                        options.Scope.Add(scope);
                    }

                    options.ClientId = openIdOptions.ClientId;
                    options.ClientSecret = openIdOptions.ApplicationKey;
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
                    option.Authority = oauth2Options.Authority;
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
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        ArgumentNullException.ThrowIfNull(authenticationSectionName, nameof(authenticationSectionName));

        var section = configuration.GetSection(authenticationSectionName);

        if (section is null || !section.Exists())
        {
            throw new NullReferenceException($"No section exists with name {authenticationSectionName} in the configuration providers for OpenId Connect authentication.");
        }

        var settings = section.Get<OidcAuthenticationSectionOptions>() ?? throw new NullReferenceException($"No section exists with name {authenticationSectionName} in the configuration providers for OpenId Connect authentication.");

        if (string.IsNullOrWhiteSpace(settings.MetadataAddress))
        {
            throw new MissingFieldException("MetadataAddress must be filled!");
        }
        if (string.IsNullOrWhiteSpace(settings.CookieName))
        {
            throw new MissingFieldException("We need a cookie name defined specifically for your services.");
        }
        if (string.IsNullOrWhiteSpace(settings.OpenIdSettingsSectionPath))
        {
            throw new MissingFieldException("We need a setting section to configure the OpenId Connect.");
        }
        if (string.IsNullOrWhiteSpace(settings.OAuth2SettingsSectionPath))
        {
            throw new MissingFieldException("We need a setting section to configure OAuth2.");
        }
        if (string.IsNullOrWhiteSpace(settings.CertificateSectionPath))
        {
            throw new MissingFieldException("We need a cookie name defined specifically for your services.");
        }
        if (string.IsNullOrWhiteSpace(settings.DataProtectionSectionPath))
        {
            throw new MissingFieldException("We need a setting section to configure the DataProtection cache store.");
        }

        var jwtBearerEventsType = Type.GetType(settings.JwtBearerEventsType, false);
        if (string.IsNullOrWhiteSpace(settings.JwtBearerEventsType) || jwtBearerEventsType is null)
        {
            throw new MissingFieldException("The JwtBearerEventsType must be defined.");
        }

        var cookieAuthenticationEventsType = Type.GetType(settings.CookieAuthenticationEventsType, false);
        if (string.IsNullOrWhiteSpace(settings.CookieAuthenticationEventsType) || cookieAuthenticationEventsType is null)
        {
            throw new MissingFieldException("The CookieAuthenticationEventsType must be defined.");
        }

        var openIdConnectEventsType = Type.GetType(settings.OpenIdConnectEventsType, false);
        if (string.IsNullOrWhiteSpace(settings.OpenIdConnectEventsType) || openIdConnectEventsType is null)
        {
            throw new MissingFieldException("The OpenIdConnectEventsType must be defined.");
        }

        var certSecurityKey = string.IsNullOrWhiteSpace(settings.CertSecurityKeyPath) ? null : new X509CertificateLoader(null).FindCertificate(configuration, settings.CertSecurityKeyPath) ?? throw new MissingFieldException($"No certificate was found based on the configuration section: {settings.CertSecurityKeyPath}.");

        var cert = new X509CertificateLoader(null).FindCertificate(configuration, settings.CertificateSectionPath) ?? throw new MissingFieldException($"No certificate was found based on the configuration section: {settings.CertificateSectionPath}.");

        var cookiesConfigureOptionsType = Type.GetType(settings.CookiesConfigureOptionsType, false);
        if (string.IsNullOrWhiteSpace(settings.CookiesConfigureOptionsType) || cookiesConfigureOptionsType is null)
        {
            throw new MissingFieldException("The CookiesConfigureOptionsType must be defined.");
        }

        if (string.IsNullOrWhiteSpace(settings.ResponseType))
        {
            throw new MissingFieldException("A ResponseType is mandatory to define the OpenId Connect protocol.");
        }


        // Call to prepare the Cache ticket store...

        void OidcAuthenticationFiller(OidcAuthenticationOptions options)
        {
            options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
            options.MetadataAddress = settings!.MetadataAddress;
            options.CookieName = settings.CookieName;
            options.ValidateAuthority = settings.ValidateAuthority;
            options.AuthenticationCacheTicketStoreOption = CacheTicketStoreExtension.PrepareAction(configuration, settings.AuthenticationCacheTicketStorePath);
            options.OpenIdSettingsKey = settings.OpenIdSettingsKey;
            options.OpenIdSettingsOptions = OpenIdSettingsExtension.PreapreAction(configuration, settings.OpenIdSettingsSectionPath);
            options.OAuth2SettingsKey = settings.OAuth2SettingsKey;
            options.OAuth2SettingsOptions = OAuth2SettingsExtension.PrepareAction(configuration, settings.OAuth2SettingsSectionPath);
            options.Certificate = cert;
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
        }

        return services.AddOidcAuthentication(configuration, OidcAuthenticationFiller);
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
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(authenticationOptions, nameof(authenticationOptions));

        var options = new JwtAuthenticationOptions();
        authenticationOptions(options);

        if (options.OAuth2SettingsOptions is null)
        {
            throw new ArgumentNullException(nameof(authenticationOptions), $"{nameof(options.OAuth2SettingsOptions)} is empty");
        }

        var oauth2Options = new OAuth2SettingsOption();
        options.OAuth2SettingsOptions(oauth2Options);

        ArgumentNullException.ThrowIfNull(options.JwtBearerEventsType, nameof(options.JwtBearerEventsType));
        ArgumentNullException.ThrowIfNull(options.MetadataAddress, nameof(options.MetadataAddress));

        services.ConfigureOAuth2Settings(options.OAuth2SettingsOptions, options.OAuth2SettingsKey);

        services.AddTransient(options.JwtBearerEventsType);
        services.AddAuthorization();
        services.AddHttpContextAccessor();
        var authenticationBuilder =
        services.AddAuthentication(auth => auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(option =>
                {
                    SecurityKey? securityKey = options.CertSecurityKey is not null ? new X509SecurityKey(options.CertSecurityKey) : null;

                    option.RequireHttpsMetadata = options.RequireHttpsMetadata;
                    option.Authority = oauth2Options.Authority;
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
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        ArgumentNullException.ThrowIfNull(authenticationSectionName, nameof(authenticationSectionName));

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
            options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
            options.MetadataAddress = settings!.MetadataAddress;
            options.ValidateAuthority = settings.ValidateAuthority;
            options.OAuth2SettingsKey = settings.OAuth2SettingsKey;
            options.OAuth2SettingsOptions = OAuth2SettingsExtension.PrepareAction(configuration, settings.OAuth2SettingsSectionPath);
            options.CertSecurityKey = certSecurityKey;
            options.JwtBearerEventsType = jwtBearerEventsType!;
        }

        return services.AddJwtAuthentication(configuration, JwtAuthenticationFiller);

    }
    //public static void ConfigureOpenIdAuthentication(this IServiceCollection services, IConfiguration configuration, Action<OidcAuthenticationOptions> options)
    //{
    //    ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
    //    ArgumentNullException.ThrowIfNull(options, nameof(options));

    //    var configData = new OidcAuthenticationOptions();
    //    options(configData);

    //    // validation.

    //    // is this used by the cookieManager?
    //    services.AddDataProtection()
    //            .PersistKeysToCache(configuration)
    //            .ProtectKeysWithCertificate(configData.Certificate)
    //            .SetApplicationName(configData.ApplicationName)
    //            .SetDefaultKeyLifetime(configData.DefaultKeyLifetime);

    //    services.AddCacheTicketStore(configData.AuthenticationCacheTicketStoreOption);

    //    var openIdSettings = services.ConfigureOpenIdSettings(configData.OpenIdSettingsOptions, configData.OpenIdSettingsKey);
    //    var oauth2Settings = services.ConfigureOAuth2Settings(configData.OAuth2SettingsOptions, configData.OAuth2SettingsKey);

    //    services.AddOidcAuthentication(configuration, options =>
    //    {
    //        options.CookieName = configData.CookieName;
    //        options.ValidateAuthority = configData.ValidateAuthority;
    //        options.OpenIdSettings = openIdSettings;
    //        options.OAuth2Settings = oauth2Settings;
    //        options.MetadataAddress = configData.MetadataAddress;
    //        options.CertSecurityKey = configData.CertSecurityKey;
    //        options.CookiesConfigureOptionsType = configData.CookiesConfigureOptionsType;
    //        options.JwtBearerEventsType = configData.JwtBearerEventsType;
    //        options.OpenIdConnectEventsType = configData.OpenIdConnectEventsType;
    //        options.ResponseType = configData.ResponseType;
    //        options.CookieAuthenticationEventsType = configData.CookieAuthenticationEventsType;
    //        options.ForceRefreshTimeoutTimeSpan = configData.ForceRefreshTimeoutTimeSpan;
    //        options.AuthenticationTicketTTL = configData.AuthenticationTicketTTL;
    //    });
    //}

    static string[] SplitString(string value) => value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
}
