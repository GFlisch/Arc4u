using Arc4u.Diagnostics;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Arc4u.Serializer
{
    public class JsonSerialization : IObjectSerialization
    {
        public byte[] Serialize<T>(T value)
        {
            Activity.Current?.AddTag("SerializerType", "Json");

            string json = String.Empty;

            switch (value)
            {
                case TimeSpan ts:
                    json = JsonSerializer.Serialize(ts.Ticks);
                    break;
                default:
                    json = JsonSerializer.Serialize(value);
                    break;
            }

            return UTF8Encoding.UTF8.GetBytes(json);

        }

        public T Deserialize<T>(byte[] data)
        {
            Activity.Current?.AddTag("SerializerType", "Json");

            var json = UTF8Encoding.UTF8.GetString(data);

            var typo = typeof(T);
            var type = IsNullable(typo) ? Nullable.GetUnderlyingType(typo).Name : typo.Name;
            switch (type)
            {
                case "TimeSpan":
                    long? ticks = JsonSerializer.Deserialize<long?>(json);
                    return ticks.HasValue ? (T)(object)new TimeSpan(ticks.Value) : default(T);
                default:
                    return JsonSerializer.Deserialize<T>(json);
            }
        }

        private bool IsNullable(Type type) => Nullable.GetUnderlyingType(type) != null;

        public object Deserialize(byte[] data, Type objectType)
        {
            var json = UTF8Encoding.UTF8.GetString(data);

            var type = IsNullable(objectType) ? Nullable.GetUnderlyingType(objectType).Name : objectType.Name;
            switch (type)
            {
                case "TimeSpan":
                    long? ticks = JsonSerializer.Deserialize<long?>(json);
                    return ticks.HasValue ? (object)new TimeSpan(ticks.Value) : null;
                default:
                    return JsonSerializer.Deserialize(json, objectType);
            }
        }
    }
}
