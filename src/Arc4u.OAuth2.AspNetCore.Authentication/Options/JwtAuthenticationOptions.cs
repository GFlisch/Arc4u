using System.Security.Cryptography.X509Certificates;

namespace Arc4u.OAuth2.Options
{
    public class JwtAuthenticationOptions
    {
        public AuthorityOptions DefaultAuthority { get; set; } = new AuthorityOptions();
        public Action<OAuth2SettingsOption> OAuth2SettingsOptions { get; set; } = default!;
        public Action<ClaimsIdentifierOption> ClaimsIdentifierOptions { get; set; } = default!;

        public bool ValidateAuthority { get; set; } = true;

        public X509Certificate2? CertSecurityKey { get; set; } = default!;
    }
}
