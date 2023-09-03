using System;
using System.Linq;
using System.IO;
//using System.Text;
using SevenZip.Compression.LZMA;

namespace MultiCompress
{
    internal class Lzma
    {
		// 压缩函数
		public static byte[] Compress(byte[] input)
		{
			using (MemoryStream inStream = new MemoryStream(input))
			using (MemoryStream outStream = new MemoryStream())
			{
				Encoder encoder = new Encoder();
				encoder.WriteCoderProperties(outStream);
				long fileSize = inStream.Length;
				for (int i = 0; i < 8; i++)
					outStream.WriteByte((byte)(fileSize >> (8 * i)));
				encoder.Code(inStream, outStream, -1, -1, null);
				return outStream.ToArray();
			}
		}

		// 解压缩函数
		public static byte[] Decompress(byte[] input)
		{
			using (MemoryStream inStream = new MemoryStream(input))
			using (MemoryStream outStream = new MemoryStream())
			{
				Decoder decoder = new Decoder();
				int propertiesSize = 5;
				byte[] properties = new byte[propertiesSize];
				if (inStream.Read(properties, 0, propertiesSize) != propertiesSize)
					throw new Exception("Input .lzma is too short");
				long outSize = 0;
				for (int i = 0; i < 8; i++)
				{
					int v = inStream.ReadByte();
					if (v < 0)
						throw new Exception("Can't Read 1");
					outSize |= ((long)(byte)v) << (8 * i);
				}
				decoder.SetDecoderProperties(properties);
				decoder.Code(inStream, outStream, inStream.Length, outSize, null);
				return outStream.ToArray();
			}
		}
	}
}
