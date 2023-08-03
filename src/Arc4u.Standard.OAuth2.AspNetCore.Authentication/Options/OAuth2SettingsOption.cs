using System.Collections.Generic;

namespace Arc4u.OAuth2.Options;
public class OAuth2SettingsOption
{
    public string ProviderId { get; set; } = "Bootstrap";

    public string AuthenticationType { get; set; } = Constants.BearerAuthenticationType;

    public AuthorityOptions? Authority { get; set; }

    public List<string> Audiences { get; set; } = new List<string>();

    // use for Obo scenario.
    public List<string> Scopes { get; set; } = new List<string>();

}
