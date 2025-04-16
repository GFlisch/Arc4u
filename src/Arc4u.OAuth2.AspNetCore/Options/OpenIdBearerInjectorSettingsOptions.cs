using Arc4u.Configuration;

namespace Arc4u.OAuth2.Options;

public class OpenIdBearerInjectorSettingsOptions
{
    public IKeyValueSettings OnBehalfOfOpenIdSettings { get; set; } = new SimpleKeyValueSettings();

    /// <summary>
    /// Which provider is used to create an On behal of token.
    /// </summary>
    public string OboProviderKey { get; set; } = "Obo";

    /// <summary>
    /// The OpenId KeyValues settings resolver name
    /// </summary>
    public IKeyValueSettings OpenIdSettings { get; set; } = new SimpleKeyValueSettings();
}

