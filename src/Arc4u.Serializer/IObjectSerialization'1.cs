namespace Arc4u.Serializer;

/// <summary>
/// Specify the serialization protocol for cached objects of a specific type.
/// which allows us to write a serialization method tuned to that specific type. This may be more efficient than just passing it to a general serialization protocol.
/// This is only used internally for "FastAndCompact" serialization of simple types.
/// </summary>
public interface IObjectSerialization
{
    byte[] Serialize<T>(T value);

    T? Deserialize<T>(byte[] data);

    object? Deserialize(byte[] data, Type objectType);

}
