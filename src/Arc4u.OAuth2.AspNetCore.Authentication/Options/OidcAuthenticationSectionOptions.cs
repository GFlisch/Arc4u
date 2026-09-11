using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Arc4u.OAuth2.Options;

    public class OidcAuthenticationSectionOptions
    {
        public AuthorityOptions DefaultAuthority { get; set; } = default!;

        public string CookieName { get; set; } = default!;

        public bool ValidateAuthority { get; set; } = true;

        public string OpenIdSettingsSectionPath { get; set; } = "Authentication:OpenId.Settings";

        public string ClaimsIdentifierSectionPath { get; set; } = "Authentication:ClaimsIdentifier";

        public string CertificateSectionPath { get; set; } = "Authentication:DataProtection:EncryptionCertificate";

        public string AuthenticationCacheTicketStorePath { get; set; } = "Authentication:AuthenticationCacheTicketStore";

        public string DataProtectionSectionPath { get; set; } = "Authentication:DataProtection:CacheStore";

        public TimeSpan DefaultKeyLifetime { get; set; } = TimeSpan.FromDays(365);

        public string ApplicationNameSectionPath { get; set; } = "Application.configuration:ApplicationName";

        public string TokenCacheSectionPath { get; set; } = "Authentication:TokenCache";

        public string DomainMappingsSectionPath { get; set; } = "Authentication:DomainsMapping";

        public string ClaimsFillerSectionPath { get; set; } = "Authentication:ClaimsMiddleWare:ClaimsFiller";

        public TimeSpan ForceRefreshTimeoutTimeSpan { get; set; } = TimeSpan.FromMinutes(5);

        public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(90);

        public string CallbackPath { get; set; } = "/signin-oidc";

        public string? CertSecurityKeyPath { get; set; } = default!;

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

        public string AuthenticationMethod { get; set; } = nameof(OpenIdConnectRedirectBehavior.RedirectGet);
    }
