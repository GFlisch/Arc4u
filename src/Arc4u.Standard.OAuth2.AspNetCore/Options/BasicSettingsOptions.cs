using Arc4u.OAuth2.Options;

namespace Arc4u.OAuth2;
public class BasicSettingsOptions
{
    public string ProviderId { get; set; } = "Credential";

    public string AuthenticationType { get; set; } = Constants.InjectAuthenticationType;

    public AuthorityOptions? Authority { get; set; } = default!;

    public string ClientId { get; set; } = default!;

    public List<string> Scopes { get; set; } = [];

    // Some STS are using user name - password in combination with a client secret (kind of 2 factor authentication).
    public string? ClientSecret { get; set; } = default!;
}
