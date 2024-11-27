using System.Security.Cryptography.X509Certificates;

namespace Arc4u.OAuth2.Options;
public class BasicAuthenticationConfigurationOptions
{
    public Action<BasicSettingsOptions> BasicOptions { get; set; } = default!;

    public string DefaultUpn { get; set; } = default!;

    public Action<Dictionary<string, X509Certificate2>> CertificateHeaderOptions { get; set; } = default!;
}
