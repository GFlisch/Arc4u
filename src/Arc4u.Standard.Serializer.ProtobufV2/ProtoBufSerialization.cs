using Arc4u.Serializer.ProtoBuf;
using System;
using System.Diagnostics;
using System.IO;

namespace Arc4u.Serializer
{
    /// <summary>
    /// A serialization using ProtoBuf for a specific type, without handling any special cases.
    /// The implementation differs from standard ProtoBuf-net in two ways:
    /// - The underlying type model is updated automatically, without the need to decorate with <see cref="System.Runtime.Serialization.DataContractAttribute"/>
    /// - The fully qualified type name is added to the serialization information to allow unserializing even when the type doesn't match (but is compatible).
    /// </summary>
    [Obsolete("Use Arc4u.Serializer.JSon instead.")]
    public class ProtoBufSerialization : IObjectSerialization
    {
        private static class TypedSerialize<T>
        {
            static TypedSerialize()
            {
                ProtoBufModel.ModelUpdater.Update(typeof(T));
            }

            public static byte[] Serialize(T value)
            {
                Activity.Current?.SetTag("SerializerType", "ProtobufV2");

                using (var stream = new MemoryStream())
                {
                    ProtoBufModel.Instance.Serialize(stream, value);
                    return stream.ToArray();
                }
            }

            public static T Deserialize(byte[] data)
            {
                Activity.Current?.SetTag("SerializerType", "ProtobufV2");

                using (var stream = new MemoryStream(data, 0, data.Length))
                    return (T)ProtoBufModel.Instance.Deserialize(stream, null, typeof(T));
            }
        }

        public virtual byte[] Serialize<T>(T value)
        {
            return TypedSerialize<T>.Serialize(value);
        }

        public virtual T Deserialize<T>(byte[] data)
        {
            return TypedSerialize<T>.Deserialize(data);
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
