namespace Arc4u.Configuration;

/// <summary>
/// Note that this implements equality, even though it is never used.
/// </summary>
public class SimpleKeyValueSettings : IKeyValueSettings, IEquatable<SimpleKeyValueSettings>
{
    public SimpleKeyValueSettings()
    {
        _keyValues = new(StringComparer.OrdinalIgnoreCase);
    }

    public SimpleKeyValueSettings(Dictionary<string, string> keyValues)
    {
        _keyValues = keyValues;
    }

    public static SimpleKeyValueSettings CreateFrom(IKeyValueSettings source)
    {
        var keyValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var keyPair in source.Values)
        {
            keyValues.Add(keyPair.Key, keyPair.Value);
        }

        return new SimpleKeyValueSettings(keyValues);
    }

    public void Add(string key, string value)
    {
        _keyValues.Add(key, value);
    }

    public void AddifNotNullOrEmpty(string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            _keyValues.Add(key, value!);
        }
    }
    private readonly Dictionary<string, string> _keyValues;

    public IReadOnlyDictionary<string, string> Values => _keyValues;

    public override int GetHashCode()
    {
#if NETSTANDARD2_1_OR_GREATER
        var hashCode = new HashCode();
        foreach (var value in Values)
            hashCode.Add(value);
        return hashCode.ToHashCode();
#else
        int hash = 0;
        foreach (var value in Values)
        {
            hash ^= value.GetHashCode();
        }

        return hash;
#endif
    }

    public bool Equals(SimpleKeyValueSettings? other)
    {
        if (other != null && other._keyValues.Count == _keyValues.Count)
        {
            foreach (var keyValue in _keyValues)
            {
                if (!other._keyValues.TryGetValue(keyValue.Key, out var value) || value != keyValue.Value)
                {
                    return false;
                }
            }

            return true;
        }
        return false;
    }

    public override bool Equals(object? obj)
    {
        return obj is SimpleKeyValueSettings other && Equals(other);
    }
}
