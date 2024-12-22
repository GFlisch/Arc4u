namespace Arc4u.Diagnostics;

public static class MessagePropertyEx
{
    public static void AddIfNotExist(this IDictionary<string, object?> properties, string key, object? value)
    {
        if (value == null || properties.ContainsKey(key))
        {
            return;
        }

        properties[key] = value;
    }

    public static void AddOrReplace(this IDictionary<string, object?> properties, string key, object? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        properties[key] = value;
    }
}
