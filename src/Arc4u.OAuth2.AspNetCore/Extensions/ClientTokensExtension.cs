using System.Diagnostics.CodeAnalysis;
using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.TokenProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Extensions;

/// <summary>
/// Registers the unified <c>Authentication:ClientTokens</c> section: a single section where
/// heterogeneous client token scenarios coexist, each entry discriminated by its
/// <see cref="ClientTokenSettingsOptions.Scenario"/> property and projected into a named
/// <see cref="SimpleKeyValueSettings"/>.
/// </summary>
public static class ClientTokensExtension
{
    /// <summary>
    /// Reads the <paramref name="sectionName"/> section and registers a named
    /// <see cref="SimpleKeyValueSettings"/> per entry, selecting the matching
    /// <see cref="IClientTokenScenario"/> by the entry's discriminator.
    /// </summary>
    /// <param name="configureScenarios">
    /// Optional hook to register custom scenarios or override the built-in ones
    /// (<see cref="UserPasswordScenario"/>, <see cref="ClientCredentialsScenario"/>).
    /// </param>
    public static void AddClientTokens(this IServiceCollection services,
        [DisallowNull] IConfiguration configuration,
        Action<ClientTokenScenarioRegistry>? configureScenarios = null,
        [DisallowNull] string sectionName = "Authentication:ClientTokens")
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(sectionName);

        var section = configuration.GetSection(sectionName);

        if (section is null || !section.Exists())
        {
            return;
        }

        var entries = section.Get<Dictionary<string, ClientTokenSettingsOptions>>();

        if (entries is null || entries.Count == 0)
        {
            return;
        }

        var registry = new ClientTokenScenarioRegistry()
            .Add(UserPasswordScenario.Name, new UserPasswordScenario())
            .Add(ClientCredentialsScenario.Name, new ClientCredentialsScenario());

        configureScenarios?.Invoke(registry);

        foreach (var (optionKey, options) in entries)
        {
            Register(services, registry, optionKey, options);
        }
    }

    private static void Register(IServiceCollection services, ClientTokenScenarioRegistry registry, string optionKey, ClientTokenSettingsOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Scenario))
        {
            throw new ConfigurationException($"[{optionKey}] The 'Scenario' discriminator must be filled.");
        }

        if (!registry.TryGet(options.Scenario, out var scenario))
        {
            throw new ConfigurationException($"[{optionKey}] No client token scenario is registered for the discriminator '{options.Scenario}'.");
        }

        // Eager validation: fail fast, before the service provider is built.
        scenario.Validate(optionKey, options);

        if (options.Authority is not null)
        {
            services.AddAuthority(authOptions =>
            {
                authOptions.SetData(options.Authority.Url, options.Authority.TokenEndpoint, options.Authority.Issuer, options.Authority.MetaDataAddress);
            }, optionKey);
        }

        // Projection is deferred into the named options callback; it only writes already-validated data.
        services.Configure<SimpleKeyValueSettings>(optionKey, settings => scenario.WriteTo(optionKey, options, settings));
    }
}
