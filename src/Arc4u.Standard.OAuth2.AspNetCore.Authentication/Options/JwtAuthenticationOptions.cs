using System;
using System.Security.Cryptography.X509Certificates;
using Arc4u.OAuth2.Events;

namespace Arc4u.OAuth2.Options;
public class JwtAuthenticationOptions
{
    public Action<OAuth2SettingsOption> OAuth2SettingsOptions { get; set; }

    public string OAuth2SettingsKey { get; set; } = "OAuth2";

    public string MetadataAddress { get; set; }

    public bool RequireHttpsMetadata { get; set; } = true;

    public bool ValidateAuthority { get; set; } = true;

    public Type JwtBearerEventsType { get; set; } = typeof(StandardBearerEvents);

    public X509Certificate2? CertSecurityKey { get; set; }
}
