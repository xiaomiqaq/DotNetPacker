using System;
using System.IO;
using System.Security.Cryptography;


namespace Common
{
    public enum CompressType
    {
        Lzma,
        Lzo,
        Ucl,
        Zstd
    }
    public enum SymmetricEncryptType
    {
        Des,
        Aes,
        Tdea,
        RC4
    }
    public enum AsymmetricEncryptType
    {
        Rsa,
        Ecc
    }
    //[Serializable]
    //public struct ShellInfo
    //{
    //    public SymmetricEncryptType sysEncType;
    //    public AsymmetricEncryptType asysEncType;
    //    public CompressType compType;
    //    public int oriSize;
    //    public int keySize;
    //    public string eccPriKey;
    //    public string encKey;
    //    public string iv;
    //}
    //[Serializable]
    //public class ShellInfo
    //{
    //    public SymmetricEncryptType sysEncType;
    //    public AsymmetricEncryptType asysEncType;
    //    public CompressType compType;
    //    public int oriSize;
    //    public int keySize;
    //    public string eccPriKey;
    //    public byte[] encKey;
    //    public byte[] iv;

    //    //public byte[] Serialize()
    //    //{
    //    //    byte[] serializedBytes;
    //    //    using (MemoryStream memoryStream = new MemoryStream())
    //    //    {
    //    //        IFormatter formatter = new BinaryFormatter();
    //    //        formatter.Serialize(memoryStream, this);
    //    //        serializedBytes = memoryStream.ToArray();
    //    //    }
    //    //    return serializedBytes;
    //    //}
    //    //public static ShellInfo Deserialize(byte[] data)
    //    //{
    //    //    ShellInfo deserializedShellInfo;
    //    //    using (MemoryStream memoryStream = new MemoryStream(data))
    //    //    {
    //    //        IFormatter formatter = new BinaryFormatter();
    //    //        deserializedShellInfo = (ShellInfo)formatter.Deserialize(memoryStream);
    //    //    }
    //    //    return deserializedShellInfo;
    //    //}
    //    //public byte[] SerializeXml()
    //    //{
    //    //    XmlSerializer serializer = new XmlSerializer(typeof(ShellInfo));
    //    //    using (MemoryStream writer = new MemoryStream())
    //    //    {
    //    //        serializer.Serialize(writer, this);
    //    //        return writer.ToArray();
    //    //    }
            
    //    //}
    //    //public static ShellInfo XmlDeserialize(byte[] data)
    //    //{
    //    //    XmlSerializer serializer = XmlSerializer.
    //    //    using (MemoryStream reader = new MemoryStream(data))
    //    //    {
    //    //        ShellInfo deserializedShellInfo = (ShellInfo)serializer.Deserialize(reader);
    //    //        // Use deserializedShellInfo
    //    //    }
    //    //    return deserializedShellInfo;
    //    //}
    //}

    class Tool
    {
        //序列化xml
        //static public string SerializeToXml(ShellInfo shellInfo)
        //{
        //    XmlSerializer serializer = new XmlSerializer(typeof(ShellInfo));
        //    StringWriter writer = new StringWriter();
        //    serializer.Serialize(writer, shellInfo);
        //    return writer.ToString();
        //}
        //static public ShellInfo DeserializeFromXml(string xml)
        //{
        //    XmlSerializer serializer = new XmlSerializer(typeof(ShellInfo));
        //    using (StringReader reader = new StringReader(xml))
        //    {
        //        return (ShellInfo)serializer.Deserialize(reader);
        //    }
        //}

        public static byte[] GenerateRandomKey(int length)
        {
            byte[] randomBytes = new byte[length];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return randomBytes;
        }
        static public byte[] StrToBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static public string BytesToStr(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char) + 1];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        static public byte[] RemovePadding(byte[] paddedBytes)
        {
           
            byte[] restoredBytes = new byte[paddedBytes.Length / 2];

            for (int i = 0; i < restoredBytes.Length; i++)
            {
                restoredBytes[i] = paddedBytes[i * 2];
            }

            return restoredBytes;
        }
        static public byte[] RemoveTrailingZeros(byte[] input)
        {
            int length = input.Length;

            // 从末尾开始，逐个字节判断是否为零字节
            while (length > 0 && input[length - 1] == 0x00)
            {
                length--;
            }

            // 创建新的 byte[] 数组，将不包含末尾零字节的部分复制到新数组中
            byte[] result = new byte[length];
            Array.Copy(input, result, length);

            return result;
        }
    }
    public class FileTool
    {
        public static void DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
                Console.WriteLine("文件已成功删除。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除文件时发生错误: {ex.Message}");
            }
        }
    }


}
