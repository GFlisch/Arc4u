using Microsoft.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace Arc4u.Serializer
{
    /// <summary>
    /// Serialize using Json, optionally compressed with customizable options
    /// </summary>
    public class JsonSerialization2  : IObjectSerialization
    {
        private readonly bool _compressed;
        private readonly JsonSerializerOptions _options;
        private readonly System.Text.Json.Serialization.JsonSerializerContext _context;

        /// <summary>
        /// For performance purposes, we use Microsoft's recyclable MemoryStrream pool
        /// </summary>
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        /// <summary>
        /// Construct an instance, specifying compression
        /// </summary>
        /// <param name="compressed">True if compressed, false if not</param>
        public JsonSerialization2(bool compressed)
        {
            _compressed = compressed;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }


        /// <summary>
        /// Default constructor for uncompressed Json
        /// </summary>
        public JsonSerialization2()
            : this(false)
        {
        }

        /// <summary>
        /// Construct an instance, optionally specifying compression and other Json serializer options
        /// </summary>
        /// <param name="compressed">True if compressed, false if not. Default false</param>
        /// <param name="options">Optional Json serializer optios. Default is to use the default options</param>
        public JsonSerialization2(bool compressed = false, JsonSerializerOptions options = null)
            :this(compressed)
        {
            _options = options;
        }


        /// <summary>
        /// Construct an instance, optionally specifying compression and a serialization context.
        /// This is used for source generation, implemented in .NET 6 or later (https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation?pivots=dotnet-6-0)
        /// </summary>
        /// <param name="compressed">True if compressed, false if not. Default false</param>
        /// <param name="context">Optional Json context Default is no context.</param>
        public JsonSerialization2(bool compressed = false, System.Text.Json.Serialization.JsonSerializerContext context = null)
            : this(compressed)
        {
            _context = context;
        }

        private static Stream CreateForCompression(Stream stream)
        {
#if NETSTANDARD2_1_OR_GREATER
            // Be careful when choosing the compression level for Brötli.
            // See https://github.com/dotnet/runtime/issues/26097 and https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7/#compression. Once we are on .NET 7+, this can be replaced by CompressionLevel.Optimal which will default to 4.
            return new BrotliStream(stream, CompressionLevel.Fastest, leaveOpen: true);
#else
            return new DeflateStream(stream, CompressionLevel.Fastest, leaveOpen: true);
#endif
        }

        private static Stream CreateForDecompression(Stream stream)
        {
#if NETSTANDARD2_1_OR_GREATER
            return new BrotliStream(stream, CompressionMode.Decompress);
#else
            return new DeflateStream(stream, CompressionMode.Decompress);
#endif
        }

        public byte[] Serialize<T>(T value)
        {
            Activity.Current?.SetTag("SerializerType", _compressed ? "CompressedJson" : "Json");

            if (_compressed)
                using (var output = _recyclableMemoryStreamManager.GetStream())
                {
                    using (var compressed = CreateForCompression(output))
                        if (_context != null)
                            JsonSerializer.Serialize(compressed, value, typeof(T), _context);
                        else
                            JsonSerializer.Serialize(compressed, value, _options);
                    return output.ToArray();
                }
            else
                return JsonSerializer.SerializeToUtf8Bytes(value, _options);
        }

        public T Deserialize<T>(byte[] data)
        {
            Activity.Current?.SetTag("SerializerType", _compressed ? "CompressedJson" : "Json");

            if (_compressed)
                using (var compressed = _recyclableMemoryStreamManager.GetStream(data))
                using (var uncompressed = CreateForDecompression(compressed))
                    if (_context != null)
                        return (T)JsonSerializer.Deserialize(uncompressed, typeof(T), _context);
                    else
                        return JsonSerializer.Deserialize<T>(uncompressed, _options);
            else
                return JsonSerializer.Deserialize<T>(data, _options);
        }


        public object Deserialize(byte[] data, Type objectType)
        {
            Activity.Current?.SetTag("SerializerType", _compressed ? "CompressedJson" : "Json");

            if (_compressed)
                using (var compressed = _recyclableMemoryStreamManager.GetStream(data))
                using (var uncompressed = CreateForDecompression(compressed))
                    if (_context != null)
                        return JsonSerializer.Deserialize(uncompressed, objectType, _context);
                    else
                        return JsonSerializer.Deserialize(uncompressed, objectType, _options);
            else
                return JsonSerializer.Deserialize(data, objectType, _options);
        }
    }
}
