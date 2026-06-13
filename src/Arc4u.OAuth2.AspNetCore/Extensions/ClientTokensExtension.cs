using System.Diagnostics.CodeAnalysis;
using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using Arc4u.OAuth2.TokenProvider.Scenarios;
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
    /// Resolves an <see cref="IClientTokenScenario"/> from its discriminator (the
    /// <see cref="ClientTokenSettingsOptions.Scenario"/> value), or <c>null</c> when none matches.
    /// <para>
    /// Defaults to <see cref="DefaultScenario"/> (the built-in scenarios). A framework user can
    /// reconfigure it via <see cref="SetScenarioResolver"/> — typically by handling their own
    /// discriminators and delegating to <see cref="DefaultScenario"/> for the rest.
    /// </para>
    /// </summary>
    public static Func<string, IClientTokenScenario?> Scenario => discriminator => _scenario(discriminator);

    private static Func<string, IClientTokenScenario?> _scenario = DefaultScenario;

    /// <summary>
    /// Reconfigures the scenario resolver. Delegate to <see cref="DefaultScenario"/> to keep the built-ins.
    /// </summary>
    /// <example>
    /// <code>
    /// ClientTokensExtension.SetScenarioResolver(discriminator => discriminator switch
    /// {
    ///     "MyCustom" => new MyCustomScenario(),
    ///     _ => ClientTokensExtension.DefaultScenario(discriminator)
    /// });
    /// </code>
    /// </example>
    public static void SetScenarioResolver([DisallowNull] Func<string, IClientTokenScenario?> resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        _scenario = resolver;
    }

    /// <summary>
    /// The default resolver: maps the built-in discriminators
    /// (<see cref="UserPasswordScenario.Name"/>, <see cref="ClientCredentialsScenario.Name"/>) to their scenarios.
    /// </summary>
    public static IClientTokenScenario? DefaultScenario(string discriminator) => discriminator switch
    {
        UserPasswordScenario.Name => new UserPasswordScenario(),
        ClientCredentialsScenario.Name => new ClientCredentialsScenario(),
        _ => null
    };

    /// <summary>
    /// Reads the <paramref name="sectionName"/> section and registers a named
    /// <see cref="SimpleKeyValueSettings"/> per entry, selecting the matching
    /// <see cref="IClientTokenScenario"/> by the entry's discriminator through <see cref="Scenario"/>.
    /// </summary>
    /// <param name="sectionName">Section name to read the configuration from the builder.Configuration</param>
    public static void AddClientTokens(this IServiceCollection services,
        [DisallowNull] IConfiguration configuration,
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

        foreach (var (optionKey, options) in entries)
        {
            Register(services, optionKey, options);
        }
    }

    private static void Register(IServiceCollection services, string optionKey, ClientTokenSettingsOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Scenario))
        {
            throw new ConfigurationException($"[{optionKey}] The 'Scenario' discriminator must be filled.");
        }

        var scenario = Scenario(options.Scenario)
            ?? throw new ConfigurationException($"[{optionKey}] No client token scenario is registered for the discriminator '{options.Scenario}'.");

        if (string.IsNullOrWhiteSpace(options.AuthenticationType))
        {
            throw new ConfigurationException($"[{optionKey}] AuthenticationType must be filled.");
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
        services.Configure<SimpleKeyValueSettings>(optionKey, settings =>
        {
            scenario.WriteTo(optionKey, options, settings);

            // Any Settings key the scenario does not claim is forwarded verbatim as an extra request parameter.
            var extra = options.Settings.Where(kv => !scenario.KnownKeys.Contains(kv.Key)).ToList();
            if (extra.Count > 0)
            {
                settings.Add(TokenKeys.ExtraParameters, ExtraParametersEncoder.Encode(extra));
            }
        });
    }
}
