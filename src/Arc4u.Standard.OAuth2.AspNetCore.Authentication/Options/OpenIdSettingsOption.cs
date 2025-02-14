using Arc4u.OAuth2.TokenProviders;

namespace Arc4u.OAuth2.Options;
public class OpenIdSettingsOption
{
    public string ProviderId { get; set; } = OidcTokenProvider.ProviderName;

    public string AuthenticationType { get; set; } = Constants.CookiesAuthenticationType;

    /// <summary>
    /// If null default authority is used!
    /// </summary>
    public AuthorityOptions? Authority { get; set; } = default!;

    public string ClientId { get; set; } = default!;

    public string ClientSecret { get; set; } = default!;

    public List<string> Audiences { get; set; } = [];

    public List<string> Scopes { get; set; } = [];

    public bool ValidateAudience { get; set; } = true;
}
