using Arc4u.Serializer.ProtoBuf;
using System;
using System.Diagnostics;
using System.IO;

namespace Arc4u.Serializer
{
    /// <summary>
    /// A serialization using ProtoBuf for a specific type, without handling any special cases.
    /// This will be our fallback method in <see cref="ObjectSerializationHelper{T}.FallbackSerialization"/>.
    /// The implementation differs from standard ProtoBuf-net in two ways:
    /// - The underlying type model is updated automatically, without the need to decorate with <see cref="System.Runtime.Serialization.DataContractAttribute"/>
    /// - The fully qualified type name is added to the serialization information to allow unserializing even when the type doesn't match (but is compatible).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ProtoBufSerialization : IObjectSerialization
    {
        public virtual T Deserialize<T>(byte[] data)
        {
            return (T)Deserialize(data, typeof(T));
        }

        public virtual byte[] Serialize<T>(T value)
        {
            Activity.Current?.SetTag("SerializerType", "ProtobufV2");

            ProtoBufModel.ModelUpdater.Update(typeof(T));

            using (var stream = new MemoryStream())
            {
                ProtoBufModel.Instance.Serialize(stream, value);
                return stream.ToArray();
            }
        }

        public virtual object Deserialize(byte[] data, Type objectType)
        {
            Activity.Current?.SetTag("SerializerType", "ProtobufV2");

            ProtoBufModel.ModelUpdater.Update(objectType);

            using (var stream = new MemoryStream(data, 0, data.Length))
                return ProtoBufModel.Instance.Deserialize(stream, null, objectType);
        }
    }
}
