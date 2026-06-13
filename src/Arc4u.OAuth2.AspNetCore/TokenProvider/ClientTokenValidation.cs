using Arc4u.Configuration;

namespace Arc4u.OAuth2.TokenProvider;

/// <summary>
/// A small fluent validator for the scenario-specific <see cref="Arc4u.OAuth2.Options.ClientTokenSettingsOptions.Settings"/>
/// bag. Accumulates errors and throws a single <see cref="ConfigurationException"/> at the end.
/// </summary>
internal sealed class ClientTokenValidation(string optionKey)
{
    private string? _errors;

    /// <summary>The key must be present and non-empty.</summary>
    public ClientTokenValidation Require(IReadOnlyDictionary<string, string> settings, string key)
    {
        if (!Has(settings, key))
        {
            _errors += $"[{optionKey}] '{key}' must be filled." + System.Environment.NewLine;
        }
        return this;
    }

    /// <summary>At least one of the keys must be present and non-empty.</summary>
    public ClientTokenValidation AtLeastOne(IReadOnlyDictionary<string, string> settings, params string[] keys)
    {
        if (!keys.Any(k => Has(settings, k)))
        {
            _errors += $"[{optionKey}] One of [{string.Join(", ", keys)}] must be filled." + System.Environment.NewLine;
        }
        return this;
    }

    /// <summary>The two keys cannot both be present.</summary>
    public ClientTokenValidation MutuallyExclusive(IReadOnlyDictionary<string, string> settings, string first, string second)
    {
        if (Has(settings, first) && Has(settings, second))
        {
            _errors += $"[{optionKey}] '{first}' and '{second}' cannot be filled at the same time." + System.Environment.NewLine;
        }
        return this;
    }

    /// <summary>When <paramref name="whenKey"/> is present, <paramref name="requiredKey"/> must be present too.</summary>
    public ClientTokenValidation RequiredTogether(IReadOnlyDictionary<string, string> settings, string whenKey, string requiredKey)
    {
        if (Has(settings, whenKey) && !Has(settings, requiredKey))
        {
            _errors += $"[{optionKey}] '{requiredKey}' must be filled when '{whenKey}' is used." + System.Environment.NewLine;
        }
        return this;
    }

    /// <summary>Throws a <see cref="ConfigurationException"/> when any error was accumulated.</summary>
    public void ThrowIfInvalid()
    {
        if (_errors is not null)
        {
            throw new ConfigurationException(_errors);
        }
    }

    private static bool Has(IReadOnlyDictionary<string, string> settings, string key)
        => settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value);
}
