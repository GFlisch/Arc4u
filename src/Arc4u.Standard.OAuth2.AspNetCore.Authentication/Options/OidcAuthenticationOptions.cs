using Arc4u.OAuth2.Events;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Arc4u.OAuth2.Options;

public class OidcAuthenticationOptions
{
    public IKeyValueSettings OAuth2Settings { get; set; }

    public IKeyValueSettings OpenIdSettings { get; set; }

    public string MetadataAddress { get; set; }

    public string CookieName { get; set; } = ".Arc4u.Cookies";

    public bool ValidateAuthority { get; set; }

    public ITicketStore? TicketStore { get; set; }

    public Type JwtBearerEventsType { get; set; } = typeof(StandardBearerEvents);

    public Type CookieAuthenticationEventsType { get; set; } = typeof(StandardCookieEvents);

    public Type OpenIdConnectEventsType { get; set; } = typeof(StandardOpenIdConnectEvents);

    public TimeSpan ForceRefreshTimeoutTimeSpan { get; set; } = TimeSpan.FromMinutes(15);

    public X509Certificate2? CertSecurityKey { get; set; }
}
