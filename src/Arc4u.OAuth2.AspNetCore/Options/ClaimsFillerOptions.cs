namespace Arc4u.OAuth2.Options;

/// <summary>
/// Inform if extra claims must be loaded during the authentication process.
/// </summary>
public class ClaimsFillerOptions
{
    // By default, a specific claims filler is needed to manage the right.
    public bool LoadClaimsFromClaimsFillerProvider { get; set; } = true;

    /// <summary>
    /// The settings key to load the claims from.
    /// By default no on behalf of scenario is defined => OAuth2 must be added to perform the on behalf of scenario.
    /// </summary>
    public List<string> SettingsKeys { get; set; } = [Constants.OpenIdOptionsName];
}

