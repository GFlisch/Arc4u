using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using Microsoft.IO;

namespace Arc4u.Serializer;

public class JsonZipSerialization : JsonCompressedStreamSerializationBase, IObjectSerialization
{
    /// <summary>
    /// Construct an instance with default options
    /// </summary>
    public JsonZipSerialization()
    {
    }

    /// <summary>
    /// Construct an instance, with specific serializer options
    /// </summary>
    /// <param name="options">Json serializer optios. Default is to use the default options</param>
    public JsonZipSerialization(JsonSerializerOptions options)
        : base(options)
    {
    }

    /// <summary>
    /// Construct an instance with a serialization context.
    /// This is used for source generation, implemented in .NET 6 or later (https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation?pivots=dotnet-6-0)
    /// </summary>
    /// <param name="context">Json context for source generation.</param>
    public JsonZipSerialization(System.Text.Json.Serialization.JsonSerializerContext context)
        : base(context)
    {
    }

    /// <summary>
    /// The name of the entry in the .ZIP file. Default is "content".
    /// </summary>
    protected virtual string EntryName => "content";

    protected override string SerializerType => "Json+ZipArchive";

    public byte[] Serialize<T>(T value)
    {
        Activity.Current?.SetTag("SerializerType", SerializerType);

        using var output = RecyclableMemoryStreamManager.GetStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry(EntryName, CompressionLevel.Fastest);
            using var contentStream = entry.Open();
            InternalSerialize(contentStream, value);
        }
        return output.ToArray();
    }

    public T? Deserialize<T>(byte[] data)
    {
        Activity.Current?.SetTag("SerializerType", SerializerType);
        using var compressed = RecyclableMemoryStreamManager.GetStream(data);
        using var archive = new ZipArchive(compressed);
        var entry = archive.GetEntry(EntryName);
        if (null == entry)
        {
            throw new InvalidOperationException($"Zip archive doesn't have an entry {EntryName}");
        }
        using var contentStream = entry.Open();

        return InternalDeserialize<T>(contentStream);
    }

    public object? Deserialize(byte[] data, Type objectType)
    {
        Activity.Current?.SetTag("SerializerType", SerializerType);
        using var compressed = RecyclableMemoryStreamManager.GetStream(data);
        using var archive = new ZipArchive(compressed);
        var entry = archive.GetEntry(EntryName);
        if (null == entry)
        {
            throw new InvalidOperationException($"Zip archive doesn't have an entry {EntryName}");
        }
        using var contentStream = entry.Open();
        return InternalDeserialize(contentStream, objectType);
    }
}
