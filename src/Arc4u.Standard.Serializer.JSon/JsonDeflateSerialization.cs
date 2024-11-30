using System.IO.Compression;
using System.Text.Json;

namespace Arc4u.Serializer;

/// <summary>
/// Implement object serialization with Json and Deflate compression (https://en.wikipedia.org/wiki/Deflate).
/// </summary>
public class JsonDeflateSerialization : JsonCompressedStreamSerialization
{
    /// <summary>
    /// Construct an instance with default options
    /// </summary>
    public JsonDeflateSerialization()
    {
    }

    /// <summary>
    /// Construct an instance, with specific serializer options
    /// </summary>
    /// <param name="options">Json serializer options.</param>
    public JsonDeflateSerialization(JsonSerializerOptions options)
        : base(options)
    {
    }

    /// <summary>
    /// Construct an instance with a serialization context.
    /// This is used for source generation, implemented in .NET 6 or later (https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation?pivots=dotnet-6-0)
    /// </summary>
    /// <param name="context">Json context for source generation.</param>
    public JsonDeflateSerialization(System.Text.Json.Serialization.JsonSerializerContext context)
        : base(context)
    {
    }

    protected override string SerializerType => "Json+DeflateCompression";

    protected override Stream CreateForCompression(Stream stream)
    {
        return new DeflateStream(stream, CompressionLevel.Fastest, leaveOpen: true);
    }

    protected override Stream CreateForDecompression(Stream stream)
    {
        return new DeflateStream(stream, CompressionMode.Decompress);
    }
}
