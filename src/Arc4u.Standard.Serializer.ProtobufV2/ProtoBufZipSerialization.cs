using System;
using System.IO;
using System.IO.Compression;

namespace Arc4u.Serializer.Protobuf
{
    public class ProtoBufZipSerialization : ProtoBufSerialization
    {
        private const string EntryName = "content";

        public override byte[] Serialize<T>(T value)
        {
            var blob = base.Serialize(value);

            using (var stream = new MemoryStream())
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    var zipArchiveEntry = archive.CreateEntry(EntryName, CompressionLevel.Fastest);
                    using (var zipStream = zipArchiveEntry.Open()) zipStream.Write(blob, 0, blob.Length);
                }
                stream.Seek(0, SeekOrigin.Begin);
                return stream.ToArray();
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
                    var read = contentStream.Read(buffer, 0, buffer.Length);
                    if (read != buffer.Length)
                        throw new EndOfStreamException($"Premature end of stream: expected {buffer.Length} bytes but could only read {read}");
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
