using Arc4u.Serializer.Protobuf;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Globalization;

namespace Arc4u.Serializer.ProtoBuf
{
    /// <summary>
    /// This is a private type model for cache serialization using Protobuf.
    /// We don't want to use the default model, since we need special requirements (see <see cref="Create"/>)
    /// In addition, we need to include support for deserializing a type based on the type it has been serialized.
    /// </summary>
    static class ProtoBufModel
    {
        public static readonly RuntimeTypeModel Instance = Create();
        public static readonly RuntimeTypeModelUpdater ModelUpdater = new RuntimeTypeModelUpdater(Instance);

        [ProtoContract]
        public class DateTimeOffsetProxy
        {
            [ProtoMember(1)] public long Ticks;
            [ProtoMember(2)] public TimeSpan Offset;

            public static implicit operator DateTimeOffsetProxy(DateTimeOffset value)
            {
                return new DateTimeOffsetProxy()
                {
                    Ticks = value.Ticks,
                    Offset = value.Offset
                };
            }

            public static implicit operator DateTimeOffset(DateTimeOffsetProxy value)
            {
                return new DateTimeOffset(value.Ticks, value.Offset);
            }
        }

        [ProtoContract]
        public class CultureInfoProxy
        {
            [ProtoMember(1)]
            public int CultureId { get; set; }

            public static implicit operator CultureInfoProxy(CultureInfo culture)
            {
                if (culture == null) return null;
                var obj = new CultureInfoProxy
                {
                    CultureId = culture.LCID
                };
                return obj;
            }

            public static implicit operator CultureInfo(CultureInfoProxy surrogate)
            {
                if (surrogate == null) return null;
                return new CultureInfo(surrogate.CultureId);
            }
        }

        private static RuntimeTypeModel Create()
        {
            var model = RuntimeTypeModel.Create();
            /// protobuf.net's default behavior can't handle <see cref="DateTimeOffset"/>
            model.Add(typeof(DateTimeOffset), false).SetSurrogate(typeof(DateTimeOffsetProxy));
            model.Add(typeof(CultureInfo), false).SetSurrogate(typeof(CultureInfoProxy));
            // integrate any other type information
            TypeSerializationRegistry.Register(model);
            /// this allows the use of <see cref="System.Runtime.Serialization.DataMemberAttribute"/> without Order property, which is easier on the cache users.
            /// we can get away with it since serialization is used to transfer objects of the network, and not persist them in any way that would survive the application.
            /// The importance of this is much less if we continue to use <see cref="RuntimeTypeModelUpdater"/>
            model.InferTagFromNameDefault = true;
            return model;
        }
    }
}
