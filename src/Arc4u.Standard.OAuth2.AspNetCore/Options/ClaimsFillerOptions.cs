using System;
using System.Collections.Generic;
using Arc4u.Standard.OAuth2;

namespace Arc4u.OAuth2.Options;

/// <summary>
/// Inform if extra claims must be loaded during the authentication process.
/// </summary>
public class ClaimsFillerOptions
{
    public bool LoadClaimsFromClaimsFillerProvider { get; set; }

    /// <summary>
    /// The settings key to load the claims from. => registered as a named <see cref="SimpleKeyValueSettings>"/>
    /// </summary>
    public List<string> SettingsKeys { get; set; } = new() { Constants.OpenIdOptionsName, Constants.OAuth2OptionsName };
}

