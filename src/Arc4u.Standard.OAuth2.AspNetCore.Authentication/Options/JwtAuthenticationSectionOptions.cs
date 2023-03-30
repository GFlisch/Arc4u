using Arc4u.OAuth2.Events;

namespace Arc4u.OAuth2.Options;
public class JwtAuthenticationSectionOptions
{
    public string OAuth2SettingsSectionPath { get; set; } = "Authentication:OAuth2.Settings";

    public string OAuth2SettingsKey { get; set; } = "OAuth2";

    public string MetadataAddress { get; set; }

    public bool RequireHttpsMetadata { get; set; } = true;

    public bool ValidateAuthority { get; set; } = true;

    public string JwtBearerEventsType { get; set; } = typeof(StandardBearerEvents).AssemblyQualifiedName!;

    public string? CertSecurityKeyPath { get; set; } = null;
}
