using Arc4u.Standard.OAuth2;

namespace Arc4u.OAuth2.Options;
public class OAuth2SettingsOption
{
    public string ProviderId { get; set; } = "Bootstrap";

    public string AuthenticationType { get; set; } = Constants.BearerAuthenticationType;

    /// <summary>
    /// If null default authority is used!
    /// </summary>
    public string? Authority { get; set; }

    public string Audiences { get; set; }

    // use for Obo scenario.
    public string? Scopes { get; set; }

}
