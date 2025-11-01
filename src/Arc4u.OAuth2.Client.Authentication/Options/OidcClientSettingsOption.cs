using Arc4u.OAuth2.Client.Authentication.TokenProvider;

namespace Arc4u.OAuth2.Client.Authentication.Options;

public class OidcClientSettingsOption
{
    public string ProviderId { get; set; } = OidcClientIdentityModelTokenProvider.TokenProviderName;

    public string ClientId { get; set; } = string.Empty;

    public List<string> Scopes { get; set; } = [];
}
