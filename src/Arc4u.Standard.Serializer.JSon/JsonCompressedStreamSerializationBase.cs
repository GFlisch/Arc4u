using System.Text.Json;
using Microsoft.IO;

namespace Arc4u.Serializer
{
    /// <summary>
    /// Base class of Json serialization based on compressed streams.
    /// </summary>
    public abstract class JsonCompressedStreamSerializationBase
    {
        private readonly JsonSerializerOptions _options;
        private readonly System.Text.Json.Serialization.JsonSerializerContext _context;

        /// <summary>
        /// For performance purposes, we use Microsoft's recyclable MemoryStream pool
        /// </summary>
        private RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        /// <summary>
        /// Construct an instance with default options
        /// </summary>
        protected JsonCompressedStreamSerializationBase()
        {
        }

        /// <summary>
        /// Construct an instance, optionally specifying compression and other Json serializer options
        /// </summary>
        /// <param name="options">Json serializer options</param>
        protected JsonCompressedStreamSerializationBase(JsonSerializerOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Construct an instance, optionally specifying compression and a serialization context.
        /// This is used for source generation, implemented in .NET 6 or later (https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation?pivots=dotnet-6-0)
        /// </summary>
        /// <param name="context">Json context for source generation</param>
        protected JsonCompressedStreamSerializationBase(System.Text.Json.Serialization.JsonSerializerContext context)
        {
            _context = context;
        }

        protected abstract string SerializerType { get; }

        protected virtual RecyclableMemoryStreamManager RecyclableMemoryStreamManager => _recyclableMemoryStreamManager ??= new RecyclableMemoryStreamManager();

        protected void InternalSerialize<T>(Stream utf8json, T value)
        {
            if (_context != null)
            {
                JsonSerializer.Serialize(utf8json, value, typeof(T), _context);
            }
            else
            {
                JsonSerializer.Serialize(utf8json, value, _options);
            }
        }

        protected T InternalDeserialize<T>(Stream utf8json)
        {
            if (_context != null)
            {
                return (T)JsonSerializer.Deserialize(utf8json, typeof(T), _context);
            }
            else
            {
                return JsonSerializer.Deserialize<T>(utf8json, _options);
            }
        }

        protected object InternalDeserialize(Stream utf8json, Type returnType)
        {
            if (_context != null)
            {
                return JsonSerializer.Deserialize(utf8json, returnType, _context);
            }
            else
            {
                return JsonSerializer.Deserialize(utf8json, returnType, _options);
            }
        }
    }
}
