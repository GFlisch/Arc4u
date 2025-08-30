using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using Arc4u.Configuration;
using Arc4u.Dependency;
using Arc4u.OAuth2.DataProtection;
using Arc4u.OAuth2.Events;
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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using X509CertificateLoader = Arc4u.Security.Cryptography.X509CertificateLoader;

namespace Arc4u.OAuth2.Extensions;

public static partial class AuthenticationExtensions
{
    public static AuthenticationBuilder AddOidcAuthentication(this IServiceCollection services, Action<OidcAuthenticationOptions> authenticationOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(authenticationOptions);

        var oidcOptions = new OidcAuthenticationOptions();
        authenticationOptions(oidcOptions);

        ValidateOidcOptions(oidcOptions);

        if (oidcOptions.AuthenticationCacheTicketStoreOption is not null)
        {
            services.AddCacheTicketStore(oidcOptions.AuthenticationCacheTicketStoreOption);
        }

        services.AddDataProtection()
                .PersistKeysToCache(oidcOptions.DataProtectionCacheStoreOption)
                .ProtectKeysWithCertificate(oidcOptions.DataProtectionCertificate)
                .SetApplicationName(oidcOptions.ApplicationName)
                .SetDefaultKeyLifetime(oidcOptions.DefaultKeyLifetime);

        services.Configure(authenticationOptions);
        services.AddClaimsIdentifier(oidcOptions.ClaimsIdentifierOptions);
        services.AddScoped<TokenRefreshInfo>();
        services.AddAuthorizationCore();
        services.AddHttpContextAccessor();
        services.TryAddTransient<CookieAuthenticationEvents, StandardCookieEvents>();
        services.TryAddTransient<JwtBearerEvents, StandardBearerEvents>();
        services.TryAddTransient<OpenIdConnectEvents, StandardOpenIdConnectEvents>();
        services.TryAddSingleton(typeof(IPostConfigureOptions<CookieAuthenticationOptions>), typeof(ConfigureCookieWithTicketStoreAuthenticationOptions));

        services.AddDefaultAuthority(options =>
        {
            options.SetData(oidcOptions.DefaultAuthority.Url, oidcOptions.DefaultAuthority.TokenEndpoint, oidcOptions.DefaultAuthority.Issuer, oidcOptions.DefaultAuthority.MetaDataAddress);
        });

        services.Configure<OidcAuthenticationOptions>(authenticationOptions);
        services.ConfigureOAuth2Settings(oidcOptions.OAuth2SettingsOptions, oidcOptions.OAuth2SettingsKey);
        services.ConfigureOpenIdSettings(oidcOptions.OpenIdSettingsOptions, oidcOptions.OpenIdSettingsKey);

        var oauth2Options = new OAuth2SettingsOption();
        oidcOptions.OAuth2SettingsOptions(oauth2Options);

        var openIdOptions = new OpenIdSettingsOption();
        oidcOptions.OpenIdSettingsOptions(openIdOptions);

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
                    ConfigureOpenIdConnectOptions(services, options, oidcOptions, openIdOptions, securityKey);
                })
                .AddJwtBearer(option =>
                {
                    ConfigureJwtBearerOptions(services, option, oidcOptions, oauth2Options, securityKey);
                }).AddCookie();

        return authenticationBuilder;
    }

    private static void ValidateOidcOptions(OidcAuthenticationOptions oidcOptions)
    {
        ArgumentNullException.ThrowIfNull(oidcOptions.OAuth2SettingsOptions);
        ArgumentNullException.ThrowIfNull(oidcOptions.OpenIdSettingsOptions);
        ArgumentNullException.ThrowIfNull(oidcOptions.DataProtectionCertificate);
        ArgumentNullException.ThrowIfNull(oidcOptions.DefaultAuthority.GetMetaDataAddress());
    }

    private static void ConfigureOpenIdConnectOptions(IServiceCollection services, OpenIdConnectOptions options, OidcAuthenticationOptions oidcOptions, OpenIdSettingsOption openIdOptions, SecurityKey? securityKey)
    {
        ArgumentNullException.ThrowIfNull(oidcOptions.DefaultAuthority.GetMetaDataAddress());

        options.UsePkce = true;
        options.UseTokenLifetime = false;
        options.SaveTokens = false;
        options.Authority = openIdOptions.Authority is null ? oidcOptions.DefaultAuthority.Url.ToString() : openIdOptions.Authority.Url.ToString();
        options.RequireHttpsMetadata = oidcOptions.DefaultAuthority.MetaDataAddress.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.InvariantCultureIgnoreCase);
        options.MetadataAddress = oidcOptions.DefaultAuthority.MetaDataAddress.ToString();
        options.ResponseType = oidcOptions.ResponseType;
        options.CallbackPath = oidcOptions.CallbackPath;
        options.Scope.Clear();
        openIdOptions.Scopes.ForEach(options.Scope.Add);
        options.ClientId = openIdOptions.ClientId;
        options.ClientSecret = openIdOptions.ClientSecret;
        options.GetClaimsFromUserInfoEndpoint = false;
        options.MapInboundClaims = false;
        options.TokenValidationParameters.NameClaimType = oidcOptions.NameClaimType;
        options.TokenValidationParameters.RoleClaimType = oidcOptions.RoleClaimType;
        options.TokenValidationParameters.SaveSigninToken = false;
        options.TokenValidationParameters.AuthenticationType = openIdOptions.AuthenticationType;
        options.TokenValidationParameters.ValidateAudience = openIdOptions.ValidateAudience;
        options.TokenValidationParameters.ValidAudiences = openIdOptions.Audiences;
        if (securityKey is not null)
        {
            options.TokenValidationParameters.IssuerSigningKey = securityKey;
        }
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.SaveTokens = true;
        options.AuthenticationMethod = OpenIdConnectRedirectBehavior.FormPost;
        options.ResponseMode = OpenIdConnectResponseMode.FormPost;
        options.EventsType = typeof(OpenIdConnectEvents); //services.GetImplementationType<OpenIdConnectEvents>();
    }

    private static void ConfigureJwtBearerOptions(IServiceCollection services, JwtBearerOptions option, OidcAuthenticationOptions oidcOptions, OAuth2SettingsOption oauth2Options, SecurityKey? securityKey)
    {
        ArgumentNullException.ThrowIfNull(oidcOptions.DefaultAuthority.MetaDataAddress);

        option.RequireHttpsMetadata = oidcOptions.DefaultAuthority.MetaDataAddress.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.InvariantCultureIgnoreCase);
        option.Authority = oauth2Options.Authority is null ? oidcOptions.DefaultAuthority.Url.ToString() : oauth2Options.Authority.Url.ToString();
        option.MetadataAddress = oidcOptions.DefaultAuthority.MetaDataAddress.ToString();
        option.SaveToken = true;
        option.MapInboundClaims = false;
        option.TokenValidationParameters.NameClaimType = oidcOptions.NameClaimType;
        option.TokenValidationParameters.RoleClaimType = oidcOptions.RoleClaimType;
        option.TokenValidationParameters.SaveSigninToken = false;
        option.TokenValidationParameters.AuthenticationType = oauth2Options.AuthenticationType;
        option.TokenValidationParameters.ValidateIssuer = false;
        option.TokenValidationParameters.ValidateAudience = oauth2Options.ValidateAudience;
        option.TokenValidationParameters.ValidAudiences = oauth2Options.Audiences;
        if (securityKey is not null)
        {
            option.TokenValidationParameters.IssuerSigningKey = securityKey;
        }
        option.EventsType = typeof(JwtBearerEvents);
    }

    public static AuthenticationBuilder AddOidcAuthentication(this IServiceCollection services, IConfiguration configuration, [DisallowNull] string authenticationSectionName = "Authentication", IX509CertificateLoader? certificateLoader = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(authenticationSectionName);

        var section = configuration.GetSection(authenticationSectionName);

        if (!section.Exists())
        {
            throw new ConfigurationException($"No section exists with name {authenticationSectionName} in the configuration providers for OpenId Connect authentication.");
        }

        var settings = new OidcAuthenticationSectionOptions();
        section.Bind(settings);

        string? configErrors = null;
        if (settings.DefaultAuthority is null)
        {
            configErrors += "DefaultAuthority must be filled!" + System.Environment.NewLine;
        }
        if (string.IsNullOrWhiteSpace(settings.CookieName))
        {
            configErrors += "We need a cookie name defined specifically for your services." + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(settings.NameClaimType))
        {
            configErrors += "We need a name claim type defined specifically for your services." + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(settings.RoleClaimType))
        {
            configErrors += "We need a role claim type defined specifically for your services." + System.Environment.NewLine;
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

        certificateLoader ??= new X509CertificateLoader(null);
        var certSecurityKey = string.IsNullOrWhiteSpace(settings.CertSecurityKeyPath) ? null : certificateLoader.FindCertificate(configuration, settings.CertSecurityKeyPath) ?? throw new MissingFieldException($"No certificate was found based on the configuration section: {settings.CertSecurityKeyPath}.");

        var cert = certificateLoader.FindCertificate(configuration, settings.CertificateSectionPath) ?? throw new MissingFieldException($"No certificate was found based on the configuration section: {settings.CertificateSectionPath}.");

        var ticketStoreAction = CacheTicketStoreExtension.PrepareAction(configuration, settings.AuthenticationCacheTicketStorePath);

        if (string.IsNullOrWhiteSpace(settings.ResponseType))
        {
            throw new MissingFieldException("A ResponseType is mandatory to define the OpenId Connect protocol.");
        }

        // // Map default settings if not defined (only for non string values).
        // if (!section.GetChildren().Any(c => c.Key == nameof(OidcAuthenticationOptions.AuthenticationTicketTTL)))
        // {
        //     settings.AuthenticationTicketTTL = defaultSettings.AuthenticationTicketTTL;
        // }
        //
        // if (!section.GetChildren().Any(c => c.Key == nameof(OidcAuthenticationOptions.ForceRefreshTimeoutTimeSpan)))
        // {
        //     settings.ForceRefreshTimeoutTimeSpan = defaultSettings.ForceRefreshTimeoutTimeSpan;
        // }
        //
        // if (!section.GetChildren().Any(c => c.Key == nameof(OidcAuthenticationOptions.DefaultKeyLifetime)))
        // {
        //     settings.DefaultKeyLifetime = defaultSettings.DefaultKeyLifetime;
        // }

        //if (!section.GetChildren().Any(c => c.Key == nameof(OidcAuthenticationOptions.ValidateAudience)))
        //{
        //    settings.ValidateAudience = defaultSettings.ValidateAudience;
        //}

        //if (!section.GetChildren().Any(c => c.Key == nameof(OidcAuthenticationOptions.ValidateAuthority)))
        //{
        //    settings.ValidateAuthority = defaultSettings.ValidateAuthority;
        //}

        void OidcAuthenticationFiller(OidcAuthenticationOptions options)
        {
            options.DefaultAuthority = settings.DefaultAuthority!;
            options.CookieName = settings.CookieName;
            options.AuthenticationCacheTicketStoreOption = ticketStoreAction!;
            options.OpenIdSettingsKey = settings.OpenIdSettingsKey;
            options.OpenIdSettingsOptions = OpenIdSettingsExtension.PrepareAction(configuration, settings.OpenIdSettingsSectionPath);
            options.OAuth2SettingsKey = settings.OAuth2SettingsKey;
            options.OAuth2SettingsOptions = OAuth2SettingsExtension.PrepareAction(configuration, settings.OAuth2SettingsSectionPath);
            options.DataProtectionCertificate = cert;
            options.CallbackPath = settings.CallbackPath;
            options.DefaultKeyLifetime = settings.DefaultKeyLifetime;
            options.ApplicationName = configuration[settings.ApplicationNameSectionPath]!;
            options.ForceRefreshTimeoutTimeSpan = settings.ForceRefreshTimeoutTimeSpan;
            options.RefreshTokenLifetime = settings.RefreshTokenLifetime;
            options.CertSecurityKey = certSecurityKey;
            options.ResponseType = settings.ResponseType;
            options.AuthenticationTicketTTL = settings.AuthenticationTicketTTL;
            options.DataProtectionCacheStoreOption = CacheStoreExtension.PrepareAction(configuration, settings.DataProtectionSectionPath);
            options.ClaimsIdentifierOptions = ClaimsidentifierExtension.PrepareAction(configuration, settings.ClaimsIdentifierSectionPath);
            options.NameClaimType = settings.NameClaimType;
            options.RoleClaimType = settings.RoleClaimType;
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
    /// <param name="configuration"></param>
    /// <param name="authenticationOptions">Custom options</param>
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

        ArgumentNullException.ThrowIfNull(options.DefaultAuthority.GetMetaDataAddress());
        ArgumentNullException.ThrowIfNull(options.DefaultAuthority.MetaDataAddress);

        services.ConfigureOAuth2Settings(options.OAuth2SettingsOptions, options.OAuth2SettingsKey);
        services.AddClaimsIdentifier(options.ClaimsIdentifierOptions);
        services.TryAddTransient<JwtBearerEvents, StandardBearerEvents>();
        services.AddAuthorizationCore();
        services.AddHttpContextAccessor();
        services.AddDefaultAuthority(auth =>
        {
            auth.SetData(options.DefaultAuthority.Url, options.DefaultAuthority.TokenEndpoint, options.DefaultAuthority.Issuer, options.DefaultAuthority.MetaDataAddress);
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
                    option.TokenValidationParameters.ValidateAudience = oauth2Options.ValidateAudience;
                    option.TokenValidationParameters.ValidAudiences = oauth2Options.Audiences;
                    option.EventsType = typeof(JwtBearerEvents);
                    if (securityKey is not null)
                    {
                        option.TokenValidationParameters.IssuerSigningKey = securityKey;
                    }
                });

        return authenticationBuilder;
    }

    public static AuthenticationBuilder AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration, [DisallowNull] string authenticationSectionName = "Authentication", IX509CertificateLoader? certificateLoader = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(authenticationSectionName);

        var section = configuration.GetSection(authenticationSectionName) ?? throw new InvalidOperationException($"No section exists with name {authenticationSectionName} in the configuration providers for OAuth2 Connect authentication.");
        var settings = section.Get<JwtAuthenticationSectionOptions>() ?? throw new InvalidOperationException($"No section exists with name {authenticationSectionName} in the configuration providers for OAuth2 Connect authentication.");

        if (settings.DefaultAuthority is null)
        {
            throw new MissingFieldException("DefaultAuthority must be filled!");
        }

        if (string.IsNullOrWhiteSpace(settings.OAuth2SettingsSectionPath))
        {
            throw new MissingFieldException("We need a setting section to configure OAuth2.");
        }

        X509Certificate2? certSecurityKey;

        if (string.IsNullOrWhiteSpace(settings.CertSecurityKeyPath))
        {
            certSecurityKey = null;
        }
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
