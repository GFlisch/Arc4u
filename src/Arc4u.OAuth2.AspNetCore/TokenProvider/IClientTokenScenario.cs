using Arc4u.Configuration;
using Arc4u.OAuth2.Options;

namespace Arc4u.OAuth2.TokenProvider;

/// <summary>
/// A client token acquisition scenario.
/// <para>
/// Each scenario owns the variability of one client token shape: its validation rules, the
/// <c>ProviderId</c> (token provider) that handles it, and how it projects the bound
/// <see cref="ClientTokenSettingsOptions"/> into a flat <see cref="SimpleKeyValueSettings"/>.
/// Scenarios are selected by the <see cref="ClientTokenSettingsOptions.Scenario"/> discriminator
/// through a <see cref="ClientTokenScenarioRegistry"/>.
/// </para>
/// </summary>
public interface IClientTokenScenario
{
    /// <summary>
    /// Validates the bound options. Implementations throw a
    /// <see cref="ConfigurationException"/> when the configuration is invalid. Invoked eagerly,
    /// before the service provider is built, so misconfiguration fails fast.
    /// </summary>
    void Validate(string optionKey, ClientTokenSettingsOptions options);

    /// <summary>
    /// Projects the bound options into the flat <see cref="SimpleKeyValueSettings"/> consumed by the token provider.
    /// </summary>
    void WriteTo(string optionKey, ClientTokenSettingsOptions options, SimpleKeyValueSettings settings);
}
