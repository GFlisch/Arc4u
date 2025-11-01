
using Arc4u.OAuth2.Options;

namespace Arc4u.OAuth2.Client.Authentication.Options;

    public class OidcClientAuthenticationSectionOptions
    {
        public AuthorityOptions DefaultAuthority { get; set; } = new AuthorityOptions();

        public bool ValidateAuthority { get; set; } = true;

        public string OidcClientIdSettingsSectionPath { get; set; } = "Authentication:OidcClient.Settings";

        public ApiExtraContextAuthenticationSectionOption ApiExtraContextAuthenticationSection { get; set; } = new ApiExtraContextAuthenticationSectionOption
        {
            AuthorizationEndpointSectionPath = "Authentication:OidcClient.Settings:AuthorizationEndpoint",
            TokenEndpointSectionPath = "Authentication:OidcClient.Settings:TokenEndpoint"
        };
        public string ClaimsIdentifierSectionPath { get; set; } = "Authentication:ClaimsIdentifier";

        public string ApplicationNameSectionPath { get; set; } = "Application.configuration:ApplicationName";

        public string DomainMappingsSectionPath { get; set; } = "Authentication:DomainsMapping";

        public string ClaimsFillerSectionPath { get; set; } = "Authentication:ClaimsMiddleWare:ClaimsFiller";

        public TimeSpan ForceRefreshTimeoutTimeSpan { get; set; } = TimeSpan.FromMinutes(5);

        public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(90);

        public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);

        public string CallbackPath { get; set; } = "/";
        public string PostLogoutRedirectUri { get; set; } = "/";

        public bool LoadProfile { get; set; }
        /// <summary>
        /// Define the claim type used to identify the name of the user.
        /// </summary>
        public string NameClaimType { get; set; } = "name";

        /// <summary>
        /// Define the claim type used to identify the role of the user.
        /// </summary>
        public string RoleClaimType { get; set; } = "role";

    }
