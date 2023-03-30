//using System;
//using System.Security.Cryptography.X509Certificates;
//using Arc4u.OAuth2.Events;
//using Microsoft.IdentityModel.Protocols.OpenIdConnect;

//namespace Arc4u.OAuth2.Options;

//public class OidcAuthenticationBuilderOptions
//{
//    public IKeyValueSettings OAuth2Settings { get; set; }

//    public IKeyValueSettings OpenIdSettings { get; set; }

//    public string? MetadataAddress { get; set; }

//    public string CookieName { get; set; } = ".Arc4u.Cookies";

//    public bool ValidateAuthority { get; set; }

//    public Type JwtBearerEventsType { get; set; } = typeof(StandardBearerEvents);

//    public Type CookieAuthenticationEventsType { get; set; } = typeof(StandardCookieEvents);

//    public Type OpenIdConnectEventsType { get; set; } = typeof(StandardOpenIdConnectEvents);

//    public TimeSpan ForceRefreshTimeoutTimeSpan { get; set; } = TimeSpan.FromMinutes(60);

//    public X509Certificate2? CertSecurityKey { get; set; }

//    /// <summary>
//    /// The <see cref="IPostConfigureOptions<CookieAuthenticationOptions"/> type used to configure the <see cref="CookieAuthenticationOptions"/>.
//    /// </summary>
//    public Type CookiesConfigureOptionsType { get; set; } = typeof(ConfigureCookieAuthenticationOptions);

//    /// <summary>
//    /// For the other OIDC => ResponseType = OpenIdConnectResponseType.CodeIdTokenToken;
//    /// For AzureAD, AzureB2C and Adfs => ResponseType = OpenIdConnectResponseType.Code;
//    /// </summary>
//    public string ResponseType { get; set; } = OpenIdConnectResponseType.Code;

//    /// <summary>
//    /// Time to live of the authentication ticket.
//    /// Default is 7 days.
//    /// </summary>
//    public TimeSpan AuthenticationTicketTTL { get; set; } = TimeSpan.FromDays(7);
//}
