using System.Security.Cryptography.X509Certificates;
using Arc4u.OAuth2.Configuration;
using Arc4u.OAuth2.DataProtection;
using Arc4u.OAuth2.Events;
using Arc4u.OAuth2.TicketStore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Arc4u.OAuth2.Options;
public class OidcAuthenticationOptions
{
    public AuthorityOptions DefaultAuthority { get; set; }

    public string CookieName { get; set; }

    public bool ValidateAuthority { get; set; } = true;

    public string OpenIdSettingsKey { get; set; } = Constants.OpenIdOptionsName;

    public Action<OpenIdSettingsOption> OpenIdSettingsOptions { get; set; }

    public string OAuth2SettingsKey { get; set; } = Constants.OAuth2OptionsName;

    public Action<OAuth2SettingsOption> OAuth2SettingsOptions { get; set; }

    public Action<ClaimsIdentifierOption> ClaimsIdentifierOptions { get; set; }

    public X509Certificate2 Certificate { get; set; }

    public Action<CacheTicketStoreOptions> AuthenticationCacheTicketStoreOption { get; set; }

    public Action<CacheStoreOption> DataProtectionCacheStoreOption { get; set; }

    public TimeSpan DefaultKeyLifetime { get; set; } = TimeSpan.FromDays(365);

    public string CallbackPath { get; set; } = "/signin-oidc";

    public string ApplicationName { get; set; }

    public Type JwtBearerEventsType { get; set; } = typeof(StandardBearerEvents);

    public Type CookieAuthenticationEventsType { get; set; } = typeof(StandardCookieEvents);

    public Type OpenIdConnectEventsType { get; set; } = typeof(StandardOpenIdConnectEvents);

    public TimeSpan ForceRefreshTimeoutTimeSpan { get; set; } = TimeSpan.FromMinutes(5);

    public X509Certificate2? CertSecurityKey { get; set; }

    /// <summary>
    /// The <see cref="IPostConfigureOptions<CookieAuthenticationOptions"/> type used to configure the <see cref="CookieAuthenticationOptions"/>.
    /// </summary>
    public Type? CookiesConfigureOptionsType { get; set; } = typeof(ConfigureCookieWithTicketStoreAuthenticationOptions);

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

    /// <summary>
    /// By default the audience is validated. It is always better to do 
    /// On Keycloak audience doesn't exist by default, so it is needed to disable it.
    /// </summary>
    public bool ValidateAudience { get; set; } = true;
}
