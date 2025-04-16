
namespace Arc4u.OAuth2.Options;

public class OpenIdBearerInjectorOptions
{
    public string OnBehalfOfOpenIdSettingsKey { get; set; } = "Obo_for_OpenId";

    /// <summary>
    /// Which provider is used to create an On behal of token.
    /// </summary>
    public string OboProviderKey { get; set; } = "Obo";

    /// <summary>
    /// The OpenId KeyValues settings resolver name
    /// </summary>
    public string OpenIdSettingsKey { get; set; } = Constants.OpenIdOptionsName;
}

