using System.Text.Json;

namespace Arc4u.Configuration.Store.Internals;

/// <summary>
/// We need a value holder for holding the value to be serialized  because some of those values
/// can be primitive types and these can't be passed as arguments to <see cref="JsonSerializer"/>.
/// By encapsulating values in a serialiable <see cref="ValueHolder{TValue}"/>, we avoid having to check for that special case.
/// </summary>
/// <typeparam name="TValue"></typeparam>
sealed class ValueHolder<TValue> : IValueHolder
{
    /// <summary>
    /// This must be public because of Json serialization
    /// </summary>
    public TValue? Value { get; }

    public ValueHolder(TValue? value) => Value = value;

    public string Serialize() => JsonSerializer.Serialize(this, new JsonSerializerOptions());

    public void Serialize(Stream stream) => JsonSerializer.Serialize(stream, this);

    public static TValue? Deserialize(string json) => JsonSerializer.Deserialize<ValueHolder<TValue>>(json)!.Value;
}


static class ValueHolder
{
    public static ValueHolder<TValue> Create<TValue>(TValue? value) => new(value);
}
