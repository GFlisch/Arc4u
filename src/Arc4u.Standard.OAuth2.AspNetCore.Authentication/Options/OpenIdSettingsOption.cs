using Arc4u.OAuth2.TokenProviders;
using Arc4u.Standard.OAuth2;

namespace Arc4u.OAuth2.Options;
public class OpenIdSettingsOption
{
    public string ProviderId { get; set; } = OidcTokenProvider.ProviderName;

    public string AuthenticationType { get; set; } = Constants.CookiesAuthenticationType;

    /// <summary>
    /// If null default authority is used!
    /// </summary>
    public string? Authority { get; set; }

    public string ClientId { get; set; }

    public string ClientSecret { get; set; }

    public string Audiences { get; set; }

    public string Scopes { get; set; }
}
