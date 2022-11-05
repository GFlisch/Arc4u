using Arc4u.Standard.OAuth2.Events;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using System;

namespace Arc4u.Standard.OAuth2.Options
{
    public class AuthenticationOidcOptions
    {
        public IKeyValueSettings OAuthSettings { get; set; }

        public IKeyValueSettings OpenIdSettings { get; set; }

        public string MetadataAddress { get; set; }

        public string CookieName { get; set; } = ".Arc4u.Cookies";

        public bool ValidateAuthority { get; set; }

        public ITicketStore TicketStore { get; set; }

        public IDataProtectionProvider  DataProtectionProvider { get; set; }

        public Type JwtBearerEventsType { get; set; } = typeof(CustomBearerEvents);

        public Type CookieAuthenticationEventsType { get; set; } = typeof(customCookieEvents);

        public Type OpenIdConnectEventsType { get; set; } = typeof(CustomOpenIdConnectEvents);

        public TimeSpan ForceRefreshTimeoutTimeSpan { get; set; } = TimeSpan.FromMinutes(15);
    }
}
