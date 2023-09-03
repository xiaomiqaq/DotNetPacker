using System;
using System.IO;
using System.IO.Compression;
using Zstandard.Net;
namespace MultiCompress
{
    public class Zstd
    {
        public static byte[] Compress(byte[] input)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var compressionStream = new ZstandardStream(memoryStream, CompressionMode.Compress))
                {
                    compressionStream.Write(input, 0, input.Length);
                }
                return memoryStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] compressedInput)
        {
            using (var compressedStream = new MemoryStream(compressedInput))
            using (var decompressionStream = new ZstandardStream(compressedStream, CompressionMode.Decompress))
            using (var memoryStream = new MemoryStream())
            {
                decompressionStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
