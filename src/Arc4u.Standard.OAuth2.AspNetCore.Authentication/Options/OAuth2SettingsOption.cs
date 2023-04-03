using Arc4u.Standard.OAuth2;

namespace Arc4u.OAuth2.Options;
public class OAuth2SettingsOption
{
    public string ProviderId { get; set; } = "Bootstrap";

    public string AuthenticationType { get; set; } = Constants.BearerAuthenticationType;

    public string Authority { get; set; }

    public string ClientId { get; set; }

    public string Audiences { get; set; }
}
