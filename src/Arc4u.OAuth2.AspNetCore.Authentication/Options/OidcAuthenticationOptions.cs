using System.Security.Cryptography.X509Certificates;
using Arc4u.OAuth2.DataProtection;
using Arc4u.OAuth2.TicketStore;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Arc4u.OAuth2.Options;

    public class OidcAuthenticationOptions
    {
        public AuthorityOptions DefaultAuthority { get; set; } = default!;

        public string CookieName { get; set; } = default!;

        public bool ValidateAuthority { get; set; } = true;

        public string OpenIdSettingsKey { get; set; } = Constants.OpenIdOptionsName;

        public Action<OpenIdSettingsOption> OpenIdSettingsOptions { get; set; } = default!;

        public Action<ClaimsIdentifierOption> ClaimsIdentifierOptions { get; set; } = default!;

        public X509Certificate2 DataProtectionCertificate { get; set; } = default!;

        public Action<CacheTicketStoreOptions> AuthenticationCacheTicketStoreOption { get; set; } = default!;

        public Action<CacheStoreOption> DataProtectionCacheStoreOption { get; set; } = default!;

        public TimeSpan DefaultKeyLifetime { get; set; } = TimeSpan.FromDays(365);

        public string CallbackPath { get; set; } = "/signin-oidc";

        public string ApplicationName { get; set; } = default!;

        public TimeSpan ForceRefreshTimeoutTimeSpan { get; set; } = TimeSpan.FromMinutes(5);

        public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(90);

        public X509Certificate2? CertSecurityKey { get; set; } = default!;

        /// <summary>
        /// For the other OIDC => ResponseType = OpenIdConnectResponseType.CodeIdTokenToken;
        /// For AzureAD, AzureB2C and Adfs => ResponseType = OpenIdConnectResponseType.Code;
        /// </summary>
        public string ResponseType { get; set; } = OpenIdConnectResponseType.Code;

        /// <summary>
        /// Time to live of the authentication ticket.
        /// Default is 7 days.
        /// </summary>
        public TimeSpan AuthenticationTicketTtl { get; set; } = TimeSpan.FromDays(7);

        /// <summary>
        /// By default the audience is validated. It is always better to do
        /// On Keycloak audience doesn't exist by default, so it is needed to disable it or add it.
        /// </summary>
        public bool ValidateAudience { get; set; } = true;

        /// <summary>
        /// Define the claim type used to identify the name of the user.
        /// </summary>
        public string NameClaimType { get; set; } = "name";

        /// <summary>
        /// Define the claim type used to identify the role of the user.
        /// </summary>
        public string RoleClaimType { get; set; } = "role";

        public OpenIdConnectRedirectBehavior AuthenticationMethod { get; set; } = OpenIdConnectRedirectBehavior.FormPost;
    }
