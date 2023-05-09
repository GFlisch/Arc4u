namespace Arc4u.OAuth2.Options;

public class OnBehalfOfSettingsOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string Scope { get; set; } = "openid";
    public string ApplicationKey { get; set; } = string.Empty;
}
