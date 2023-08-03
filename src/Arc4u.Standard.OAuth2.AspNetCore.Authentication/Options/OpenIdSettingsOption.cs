using System.Collections.Generic;
using Arc4u.OAuth2.TokenProviders;

namespace Arc4u.OAuth2.Options;
public class OpenIdSettingsOption
{
    public string ProviderId { get; set; } = OidcTokenProvider.ProviderName;

    public string AuthenticationType { get; set; } = Constants.CookiesAuthenticationType;

    /// <summary>
    /// If null default authority is used!
    /// </summary>
    public AuthorityOptions? Authority { get; set; }

    public string ClientId { get; set; }

    public string ClientSecret { get; set; }

    public List<string> Audiences { get; set; } = new List<string>();

    public string Scopes { get; set; }
}
