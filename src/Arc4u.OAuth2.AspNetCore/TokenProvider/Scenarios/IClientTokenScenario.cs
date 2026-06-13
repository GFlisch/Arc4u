using Arc4u.Configuration;
using Arc4u.OAuth2.Options;

namespace Arc4u.OAuth2.TokenProvider.Scenarios;

/// <summary>
/// A client token acquisition scenario.
/// <para>
/// Each scenario owns the variability of one client token shape: its validation rules, the
/// <c>ProviderId</c> (token provider) that handles it, and how it projects the bound
/// <see cref="ClientTokenSettingsOptions"/> into a flat <see cref="SimpleKeyValueSettings"/>.
/// Scenarios are selected by the <see cref="ClientTokenSettingsOptions.Scenario"/> discriminator
/// through the <see cref="Arc4u.OAuth2.Extensions.ClientTokensExtension.Scenario"/> resolver function.
/// </para>
/// </summary>
public interface IClientTokenScenario
{
    /// <summary>
    /// The keys in <see cref="ClientTokenSettingsOptions.Settings"/> this scenario consumes.
    /// Any other key present in <c>Settings</c> is forwarded verbatim to the token endpoint as an
    /// extra request parameter. Should use a case-insensitive comparer.
    /// </summary>
    IReadOnlyCollection<string> KnownKeys { get; }

    /// <summary>
    /// Validates the bound options. Implementations throw a <see cref="ConfigurationException"/>
    /// when the configuration is invalid. Invoked eagerly, before the service provider is built.
    /// </summary>
    void Validate(string optionKey, ClientTokenSettingsOptions options);

    /// <summary>
    /// Projects the bound options into the flat <see cref="SimpleKeyValueSettings"/> consumed by the token provider.
    /// </summary>
    void WriteTo(string optionKey, ClientTokenSettingsOptions options, SimpleKeyValueSettings settings);
}
