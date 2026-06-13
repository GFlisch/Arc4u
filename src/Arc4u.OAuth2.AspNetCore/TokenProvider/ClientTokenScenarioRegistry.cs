namespace Arc4u.OAuth2.TokenProvider;

/// <summary>
/// A keyed registry of <see cref="IClientTokenScenario"/> instances, keyed by the
/// <see cref="Arc4u.OAuth2.Options.ClientTokenSettingsOptions.Scenario"/> discriminator.
/// <para>
/// Resolved eagerly at registration time (before the service provider is built), so scenario
/// validation can fail fast. Consumers can register their own scenarios or override the built-in
/// ones by adding the same discriminator.
/// </para>
/// </summary>
public sealed class ClientTokenScenarioRegistry
{
    private readonly Dictionary<string, IClientTokenScenario> _scenarios = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers (or overrides) the scenario for the given discriminator.
    /// </summary>
    public ClientTokenScenarioRegistry Add(string discriminator, IClientTokenScenario scenario)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(discriminator);
        ArgumentNullException.ThrowIfNull(scenario);

        _scenarios[discriminator] = scenario;
        return this;
    }

    /// <summary>
    /// Attempts to resolve the scenario for the given discriminator.
    /// </summary>
    public bool TryGet(string discriminator, out IClientTokenScenario scenario) => _scenarios.TryGetValue(discriminator, out scenario!);
}
