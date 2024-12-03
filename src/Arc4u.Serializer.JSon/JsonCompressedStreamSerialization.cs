using System.Diagnostics;
using System.Text.Json;
using Microsoft.IO;

namespace Arc4u.Serializer;

/// <summary>
/// Base class of Json serialization based on compressed streams.
/// </summary>
public abstract class JsonCompressedStreamSerialization : JsonCompressedStreamSerializationBase, IObjectSerialization
{
    /// <summary>
    /// Construct with default options
    /// </summary>
    protected JsonCompressedStreamSerialization()
    {
    }

    /// <summary>
    /// Construct an instance, optionally specifying compression and other Json serializer options
    /// </summary>
    /// <param name="options">Json serializer options</param>
    protected JsonCompressedStreamSerialization(JsonSerializerOptions options)
        : base(options)
    {
    }

    /// <summary>
    /// Construct an instance, optionally specifying compression and a serialization context.
    /// This is used for source generation, implemented in .NET 6 or later (https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation?pivots=dotnet-6-0)
    /// </summary>
    /// <param name="context">Json context for source generation</param>
    protected JsonCompressedStreamSerialization(System.Text.Json.Serialization.JsonSerializerContext context)
        : base(context)
    {
    }

    protected abstract Stream CreateForCompression(Stream stream);

    protected abstract Stream CreateForDecompression(Stream stream);

    public byte[] Serialize<T>(T value)
    {
        Activity.Current?.SetTag("SerializerType", SerializerType);

        using var output = RecyclableMemoryStreamManager.GetStream();
        using (var compressed = CreateForCompression(output))
        {
            InternalSerialize(compressed, value);
        }

        return output.ToArray();
    }

    public T? Deserialize<T>(byte[] data)
    {
        Activity.Current?.SetTag("SerializerType", SerializerType);

        using var compressed = RecyclableMemoryStreamManager.GetStream(data);
        using var uncompressed = CreateForDecompression(compressed);
        return InternalDeserialize<T>(uncompressed);
    }

    public object? Deserialize(byte[] data, Type objectType)
    {
        Activity.Current?.SetTag("SerializerType", SerializerType);

        using var compressed = RecyclableMemoryStreamManager.GetStream(data);
        using var uncompressed = CreateForDecompression(compressed);
        return InternalDeserialize(uncompressed, objectType);
    }
}
