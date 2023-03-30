using System;
using System.Security.Cryptography.X509Certificates;
using Arc4u.OAuth2.Events;
using Arc4u.OAuth2.TicketStore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Arc4u.OAuth2.Options;
public class OidcAuthenticationOptions
{
    public bool RequireHttpsMetadata { get; set; } = true;

    public string MetadataAddress { get; set; }

    public string CookieName { get; set; }

    public bool ValidateAuthority { get; set; } = true;

    public string OpenIdSettingsKey { get; set; } = "OpenId";

    public Action<OpenIdSettingsOption> OpenIdSettingsOptions { get; set; }

    public string OAuth2SettingsKey { get; set; } = "OAuth2";

    public Action<OAuth2SettingsOption> OAuth2SettingsOptions { get; set; }

    public X509Certificate2 Certificate { get; set; }

    public Action<CacheTicketStoreOption> AuthenticationCacheTicketStoreOption { get; set; }

    public TimeSpan DefaultKeyLifetime { get; set; } = TimeSpan.FromDays(365);

    public string ApplicationName { get; set; }

    public Type JwtBearerEventsType { get; set; } = typeof(StandardBearerEvents);

    public Type CookieAuthenticationEventsType { get; set; } = typeof(StandardCookieEvents);

    public Type OpenIdConnectEventsType { get; set; } = typeof(StandardOpenIdConnectEvents);

    public TimeSpan ForceRefreshTimeoutTimeSpan { get; set; } = TimeSpan.FromMinutes(60);

    public X509Certificate2? CertSecurityKey { get; set; }

    /// <summary>
    /// The <see cref="IPostConfigureOptions<CookieAuthenticationOptions"/> type used to configure the <see cref="CookieAuthenticationOptions"/>.
    /// </summary>
    public Type CookiesConfigureOptionsType { get; set; } = typeof(ConfigureCookieAuthenticationOptions);

    /// <summary>
    /// For the other OIDC => ResponseType = OpenIdConnectResponseType.CodeIdTokenToken;
    /// For AzureAD, AzureB2C and Adfs => ResponseType = OpenIdConnectResponseType.Code;
    /// </summary>
    public string ResponseType { get; set; } = OpenIdConnectResponseType.Code;

    /// <summary>
    /// Time to live of the authentication ticket.
    /// Default is 7 days.
    /// </summary>
    public TimeSpan AuthenticationTicketTTL { get; set; } = TimeSpan.FromDays(7);
}
