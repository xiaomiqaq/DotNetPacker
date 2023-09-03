using System;
using System.Linq;
using System.Text;
using UclCompression;

namespace MultiCompress
{
    internal class Ucl
	{
		public static byte[] Compress(byte[] input)
        {
			byte[] output = UclCompression.Ucl.NRV2E_99_Compress(input, 10);
			return output;
        }

		
		public static byte[] Decompress(byte[] input, int oriSize)
        {
			byte[] decompressed = UclCompression.Ucl.NRV2E_Decompress_8(input, oriSize);
			return decompressed;

		}


    }
}
