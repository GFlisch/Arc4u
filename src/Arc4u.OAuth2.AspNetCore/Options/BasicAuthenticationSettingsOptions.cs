using System.Security.Cryptography.X509Certificates;
using Arc4u.Configuration;

namespace Arc4u.OAuth2.Options;
public class BasicAuthenticationSettingsOptions
{
    public SimpleKeyValueSettings BasicSettings { get; set; } = default!;

    public string DefaultUpn { get; set; } = default!;

    public Dictionary<string, X509Certificate2> CertificateHeaderOptions { get; set; } = default!;
}
