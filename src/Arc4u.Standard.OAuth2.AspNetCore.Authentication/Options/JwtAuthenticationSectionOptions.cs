using Arc4u.OAuth2.Events;

namespace Arc4u.OAuth2.Options;
public class JwtAuthenticationSectionOptions
{
    public AuthorityOptions DefaultAuthority { get; set; }
    public string OAuth2SettingsSectionPath { get; set; } = "Authentication:OAuth2.Settings";

    public string OAuth2SettingsKey { get; set; } = Constants.OAuth2OptionsName;

    public string MetadataAddress { get; set; } = null!;

    public bool RequireHttpsMetadata { get; set; } = true;

    public bool ValidateAuthority { get; set; } = true;

    public string JwtBearerEventsType { get; set; } = typeof(StandardBearerEvents).AssemblyQualifiedName!;

    public string? CertSecurityKeyPath { get; set; }

    public string ClaimsIdentifierSectionPath { get; set; } = "Authentication:ClaimsIdentifier";

    public string ClaimsFillerSectionPath { get; set; } = "Authentication:ClaimsMiddleWare:ClaimsFiller";

    public string TokenCacheSectionPath { get; set; } = "Authentication:TokenCache";

    public string ClientSecretSectionPath { get; set; } = "Authentication:ClientSecrets";
}
