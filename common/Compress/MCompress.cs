using System;
using Common;
namespace Algorithm
{

    internal class AllCompress
    {
        static public byte[] Compress(CompressType type, byte[] input)
        {
            switch (type)
            {
                case CompressType.Lzma:
                    return MultiCompress.Lzma.Compress(input);
                case CompressType.Lzo:
                    return MultiCompress.Lzo.Compress(input);
                case CompressType.Ucl:
                    return MultiCompress.Ucl.Compress(input);
                case CompressType.Zstd:
                    return MultiCompress.Zstd.Compress(input);
                default:
                    throw new ArgumentException("Invalid compression type.");
            }

        }
        static public byte[] DeCompress(CompressType type, byte[] input, int oriSize)
        {
            switch (type)
            {
                case CompressType.Lzma:
                    return MultiCompress.Lzma.Decompress(input);
                case CompressType.Lzo:
                    {
                        byte[] output = new byte[oriSize];
                        return MultiCompress.Lzo.Decompress(input,output);
                    }
                case CompressType.Ucl:
                    return MultiCompress.Ucl.Decompress(input,oriSize);
                case CompressType.Zstd:
                    return MultiCompress.Zstd.Decompress(input);
                default:
                    throw new ArgumentException("Invalid compression type.");
            }

        }
    }
}