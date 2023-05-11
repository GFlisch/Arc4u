using Arc4u.OAuth2.Options;

namespace Arc4u.Standard.OAuth2;
public class BasicSettingsOptions
{
    public string ProviderId { get; set; } = "Credential";

    public string AuthenticationType { get; set; } = "Password";

    public AuthorityOptions? Authority { get; set; }

    public string ClientId { get; set; }

    public string Scope { get; set; } = "openid";
}
