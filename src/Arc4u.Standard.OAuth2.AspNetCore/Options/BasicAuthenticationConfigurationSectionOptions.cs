namespace Arc4u.OAuth2.Options;
public class BasicAuthenticationConfigurationSectionOptions
{
    public string BasicSettingsPath { get; set; } = "Authentication:Basic:Settings";

    public string DefaultUpn { get; set; } = string.Empty;

    public string CertificateHeaderPath { get; set; } = "Authentication:Basic:Certificates";
}
