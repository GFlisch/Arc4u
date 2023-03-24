using System;
using System.Security.Cryptography.X509Certificates;
using Arc4u.OAuth2.Events;

namespace Arc4u.OAuth2.Options;

public class JwtAuthenticationOptions
{
    public string OAuth2SettingsSectionName { get; set; } = "OAuth2.Settings";

    public string MetadataAddress { get; set; }

    public bool ValidateAuthority { get; set; }

    public Type JwtBearerEventsType { get; set; } = typeof(StandardBearerEvents);

    public X509Certificate2? CertSecurityKey { get; set; }
}
