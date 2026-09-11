namespace Arc4u.OAuth2.Options
{
    public class JwtAuthenticationSectionOptions
    {
        public AuthorityOptions DefaultAuthority { get; set; } = default!;
        public string OAuth2SettingsSectionPath { get; set; } = "Authentication:OAuth2.Settings";

        public bool ValidateAuthority { get; set; } = true;

        public string? CertSecurityKeyPath { get; set; } = default!;

        public string ClaimsIdentifierSectionPath { get; set; } = "Authentication:ClaimsIdentifier";

        public string ClaimsFillerSectionPath { get; set; } = "Authentication:ClaimsMiddleWare:ClaimsFiller";

        public string TokenCacheSectionPath { get; set; } = "Authentication:TokenCache";

        public string ClientTokensSectionPath { get; set; } = "Authentication:ClientTokens";

        public string RemoteSecretSectionPath { get; set; } = "Authentication:RemoteSecrets";

        public string DomainMappingsSectionPath { get; set; } = "Authentication:DomainsMapping";

        /// <summary>
        /// By default the audience is validated. It is always better to do
        /// On Keycloak audience doesn't exist by default, so it is needed to disable it.
        /// </summary>
        public bool ValidateAudience { get; set; } = true;

    }
}
