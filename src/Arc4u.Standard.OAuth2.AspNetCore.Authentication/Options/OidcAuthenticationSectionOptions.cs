using System;
using Arc4u.OAuth2.Events;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Arc4u.OAuth2.Options;
public class OidcAuthenticationSectionOptions
{
    public bool RequireHttpsMetadata { get; set; } = true;

    public string MetadataAddress { get; set; }

    public string CookieName { get; set; }

    public bool ValidateAuthority { get; set; } = true;

    public string OpenIdSettingsSectionPath { get; set; } = "Authentication:OpenId.Settings";

    public string OpenIdSettingsKey { get; set; } = "OpenId";

    public string OAuth2SettingsSectionPath { get; set; } = "Authentication:OAuth2.Settings";

    public string OAuth2SettingsKey { get; set; } = "OAuth2";

    public string ClaimsIdentifierSectionPath { get; set; } = "Authentication:ClaimsIdentifier";

    public string CertificateSectionPath { get; set; } = "Authentication:DataProtection:EncryptionCertificate";

    public string AuthenticationCacheTicketStorePath { get; set; } = "Authentication:AuthenticationCacheTicketStore";

    public string DataProtectionSectionPath { get; set; } = "Authentication:DataProtection:CacheStore";

    public TimeSpan DefaultKeyLifetime { get; set; } = TimeSpan.FromDays(365);

    public string ApplicationNameSectionPath { get; set; } = "Application.configuration:ApplicationName";

    public string JwtBearerEventsType { get; set; } = typeof(StandardBearerEvents).AssemblyQualifiedName!;
           
    public string CookieAuthenticationEventsType { get; set; } = typeof(StandardCookieEvents).AssemblyQualifiedName!;

    public string OpenIdConnectEventsType { get; set; } = typeof(StandardOpenIdConnectEvents).AssemblyQualifiedName!;

    public TimeSpan ForceRefreshTimeoutTimeSpan { get; set; } = TimeSpan.FromMinutes(5);

    public string CallbackPath { get; set; } = "/signin-oidc";

    public string? CertSecurityKeyPath { get; set; } = null;

    /// <summary>
    /// The <see cref="IPostConfigureOptions<CookieAuthenticationOptions"/> type used to configure the <see cref="CookieAuthenticationOptions"/>.
    /// </summary>
    public string? CookiesConfigureOptionsType { get; set; } 

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

