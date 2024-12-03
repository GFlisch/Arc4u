using System;
using System.IO;
using System.IO.Compression;

namespace Arc4u.Serializer
{
    [Obsolete("Use Arc4u.Serializer.JSon instead.")]
    public class ProtoBufZipSerialization : ProtoBufSerialization
    {
        private const string EntryName = "content";

        public override byte[] Serialize<T>(T value)
        {
            var blob = base.Serialize(value);

            using (var output = new MemoryStream())
            {
                using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
                {
                    var entry = archive.CreateEntry(EntryName, CompressionLevel.Fastest);
                    using (var contentStream = entry.Open())
                        contentStream.Write(blob, 0, blob.Length);
                }
                return output.ToArray();
            }
        }

        private static byte[] GetUncompressedData(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (ZipArchive archive = new ZipArchive(stream))
            {
                var entry = archive.GetEntry(EntryName);
                if (entry.Length > int.MaxValue)
                    throw new InternalBufferOverflowException("A message cannot exceed the Int32.MaxValue.");
                var buffer = new byte[entry.Length];
                using (var contentStream = entry.Open())
                {
                    // According tot the docs (https://learn.microsoft.com/en-us/dotnet/api/system.io.stream.read?view=net-6.0):
                    // "An implementation is free to return fewer bytes than requested even if the end of the stream has not been reached."
                    // This can (and does!) happen here, so we need to read until we have reached the requested number of bytes.
                    int index = 0;
                    int bytesLeft = buffer.Length;
                    while(bytesLeft > 0)
                    {
                        var read = contentStream.Read(buffer, index, bytesLeft);
                        if(read == 0)
                            break;
                        index += read;
                        bytesLeft -= read;
                    }
                    // If bytesLeft == 0, there could be *more* bytes left in the stream, which is an anomaly we don't check.
                    if (bytesLeft > 0)
                        throw new EndOfStreamException($"Premature end of stream: expected {buffer.Length} bytes but could only read {buffer.Length - bytesLeft} ({bytesLeft} bytes short).");
                }
                return buffer;
            }
        }

        public override T Deserialize<T>(byte[] data)
        {
            return base.Deserialize<T>(GetUncompressedData(data));
        }

        public override object Deserialize(byte[] data, Type objectType)
        {
            return base.Deserialize(GetUncompressedData(data), objectType);
        }
    }
}
