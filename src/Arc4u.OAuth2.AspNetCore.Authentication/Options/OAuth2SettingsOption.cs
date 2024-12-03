namespace Arc4u.OAuth2.Options;
public class OAuth2SettingsOption
{
    public string ProviderId { get; set; } = "Bootstrap";

    public string AuthenticationType { get; set; } = Constants.BearerAuthenticationType;

    public AuthorityOptions? Authority { get; set; } = default!;

    public List<string> Audiences { get; set; } = [];

    // use for Obo scenario.
    public List<string> Scopes { get; set; } = [];

    public bool ValidateAudience { get; set; } = true;

}
