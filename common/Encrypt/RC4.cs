using System;
using System.Text;


namespace Algorithm
{
    public class RC4
    {
        private byte[] S;
        private byte[] T;

        public RC4(byte[] key)
        {
            if (key.Length != 32)
            {
                throw new ArgumentException("RC4 key length must be 32 bytes.");
            }

            S = new byte[256];
            T = new byte[256];

            for (int i = 0; i < 256; i++)
            {
                S[i] = (byte)i;
                T[i] = key[i % 32];
            }

            int j = 0;
            for (int i = 0; i < 256; i++)
            {
                j = (j + S[i] + T[i]) % 256;
                Swap(S, i, j);
            }
        }

        public byte[] Encrypt(byte[] input)
        {
            byte[] output = new byte[input.Length];
            int i = 0;
            int j = 0;

            for (int k = 0; k < input.Length; k++)
            {
                i = (i + 1) % 256;
                j = (j + S[i]) % 256;
                Swap(S, i, j);
                int t = (S[i] + S[j]) % 256;
                output[k] = (byte)(input[k] ^ S[t]);
            }

            return output;
        }

        public byte[] Decrypt(byte[] input)
        {
            // RC4加解密使用的是相同的操作，所以解密就是再次加密
            return Encrypt(input);
        }

        private static void Swap(byte[] array, int i, int j)
        {
            byte temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
}
