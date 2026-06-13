namespace Arc4u.OAuth2.TokenProvider.Scenarios;

/// <summary>
/// Serializes the open <c>ExtraParameters</c> bag into a single form-url-encoded value so it
/// survives the flat <see cref="Arc4u.Configuration.SimpleKeyValueSettings"/> (a string/string
/// dictionary), and decodes it back on the token provider side when building the request to the
/// token endpoint.
/// </summary>
public static class ExtraParametersEncoder
{
    /// <summary>
    /// Encodes the parameters as a form-url-encoded fragment, e.g. <c>resource=https%3A%2F%2F...&amp;audience=logto</c>.
    /// </summary>
    public static string Encode(IEnumerable<KeyValuePair<string, string>> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        return string.Join('&', parameters.Select(p =>
            $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value ?? string.Empty)}"));
    }

    /// <summary>
    /// Decodes a fragment produced by <see cref="Encode"/> back into its key/value pairs.
    /// </summary>
    public static IEnumerable<KeyValuePair<string, string>> Decode(string? encoded)
    {
        if (string.IsNullOrEmpty(encoded))
        {
            yield break;
        }

        foreach (var pair in encoded.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = pair.Split('=', 2);
            yield return new KeyValuePair<string, string>(
                Uri.UnescapeDataString(kv[0]),
                kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : string.Empty);
        }
    }
}
