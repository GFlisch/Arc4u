using System.Security.Cryptography.X509Certificates;

namespace Arc4u.OAuth2.Options;
public class BasicAuthenticationConfigurationOptions
{
    public Action<BasicSettingsOptions> BasicOptions { get; set; }

    public string DefaultUpn { get; set; } = string.Empty;

    public Action<Dictionary<string, X509Certificate2>> CertificateHeaderOptions { get; set; }
}
