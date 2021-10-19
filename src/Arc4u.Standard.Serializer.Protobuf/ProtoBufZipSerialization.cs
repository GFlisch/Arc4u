using System;
using System.IO;
using System.IO.Compression;

namespace Arc4u.Serializer.Protobuf
{
    public class ProtoBufZipSerialization : ProtoBufSerialization
    {
        public override byte[] Serialize<T>(T value)
        {
            var blob = base.Serialize(value);

            using (var stream = new MemoryStream())
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    var zipArchiveEntry = archive.CreateEntry("content", CompressionLevel.Fastest);
                    using (var zipStream = zipArchiveEntry.Open()) zipStream.Write(blob, 0, blob.Length);
                }
                stream.Seek(0, SeekOrigin.Begin);
                return stream.ToArray();
            }
        }

        public override object Deserialize(byte[] data, Type objectType)
        {
            using (var stream = new MemoryStream(data))
            using (var outStream = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(stream))
                {
                    var entry = archive.GetEntry("content");
                    using (var contentStream = entry.Open())
                    {
                        int read = 0;
                        var buffer = new byte[10240];
                        do
                        {
                            read = contentStream.Read(buffer, 0, 10240);
                            outStream.Write(buffer, 0, read);
                        }
                        while (read == 10240);

                        if (outStream.Length > int.MaxValue)
                            throw new InternalBufferOverflowException("A message cannot exceed the Int32.MaxValue.");

                        outStream.Seek(0, SeekOrigin.Begin);
                        return base.Deserialize(outStream.ToArray(), objectType);
                    }
                }
            }
        }
    }
}
