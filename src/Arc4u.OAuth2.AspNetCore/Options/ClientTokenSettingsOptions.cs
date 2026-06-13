using Arc4u.OAuth2.TokenProvider;

namespace Arc4u.OAuth2.Options;

/// <summary>
/// Unified configuration entry for the <c>Authentication:ClientTokens</c> section.
/// <para>
/// A single, bindable superset DTO covering every client token acquisition scenario. The
/// <see cref="Scenario"/> property is the discriminator: it selects the
/// <see cref="IClientTokenScenario"/> responsible for validating this entry and projecting it
/// into a <see cref="Arc4u.Configuration.SimpleKeyValueSettings"/>.
/// </para>
/// </summary>
public class ClientTokenSettingsOptions : BasicSettingsOptions
{
    /// <summary>
    /// Discriminator. The key under which an <see cref="IClientTokenScenario"/> is registered in the
    /// <see cref="ClientTokenScenarioRegistry"/> (e.g. "UserPassword", "ClientCredentials").
    /// </summary>
    public string Scenario { get; set; } = default!;

    /// <summary>
    /// The inner provider used by the user/password scenario to perform the real call to the STS.
    /// </summary>
    public string BasicProviderId { get; set; } = CredentialTokenCacheTokenProvider.ProviderName;

    /// <summary>
    /// The user name (user/password scenario).
    /// </summary>
    public string? User { get; set; }

    /// <summary>
    /// The password (user/password scenario).
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Base64("user:password") — an alternative, single-field encoding of <see cref="User"/>/<see cref="Password"/>.
    /// </summary>
    public string? Credential { get; set; }

    /// <summary>
    /// Open, future-proof bag of extra request parameters forwarded to the token endpoint
    /// (e.g. the Logto.io <c>resource</c> or <c>audience</c> parameter). Bound as a key/value map and
    /// surfaced through Arc4u's <see cref="Arc4u.Configuration.IKeyValueSettings"/> abstraction.
    /// </summary>
    public Dictionary<string, string> ExtraParameters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
