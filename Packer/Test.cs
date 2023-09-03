using System;
using System.IO;
using SevenZip.Compression.LZMA;

namespace Packer
{
    internal class Test
    {
        static void Main(string[] args)
        {
            var bytesToCompress = GetBytes("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam tincidunt dui at ligula ullamcorper, at sagittis magna molestie. Curabitur quis magna nec lacus feugiat iaculis. In ut orci non nisl rutrum ultricies. Aliquam ex sapien, dapibus id cursus ac, malesuada nec risus. Integer vulputate rutrum viverra. Donec risus risus, tempus vel ipsum at, facilisis malesuada nunc. Duis non quam sed mi finibus venenatis sed in arcu. Nam eget ornare dui. Praesent ligula massa, varius eget risus sed, euismod interdum velit. Nulla hendrerit velit ut augue dapibus, et interdum nisl accumsan. Nunc convallis consequat nibh, eu facilisis lorem lacinia id. Donec vitae massa nulla. Maecenas a nisi consectetur, semper eros eget, congue ipsum. Suspendisse nec est eu tellus facilisis accumsan.");

            //byte[] compressedByte = MyCompress(bytesToCompress);
            //byte[] oriByte = MyDecompress(compressedByte);
            {
                byte[] compressOut = MCompress.Lzma.Compress(bytesToCompress);
                byte[] deBuf = MCompress.Lzma.Decompress(compressOut);
                String std =GetString(deBuf);
            }
            {
                byte[] compressOut = MCompress.Zstd.Compress(bytesToCompress);

                byte[] decompressOut = MCompress.Zstd.Decompress(compressOut);
                string str = GetString(decompressOut);
            }
            {
                byte[] compressOut = MCompress.Lzo.Compress(bytesToCompress);
                byte[] deBuf = new byte[bytesToCompress.Length];
                byte[] decompressOut = MCompress.Lzo.Decompress(compressOut, deBuf);
                string str = GetString(decompressOut);
            }
            {
                byte[] compressOut = MCompress.Ucl.Compress(bytesToCompress);
                byte[] decompressOut = MCompress.Ucl.Decompress(compressOut, bytesToCompress.Length);
                string str = GetString(decompressOut);
            }
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey(true);
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        static byte[] StreamToByteArray(Stream ms)
        {
            byte[] bytes = new byte[ms.Length];
            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
