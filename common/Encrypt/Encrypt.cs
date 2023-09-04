using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using Common;

namespace Algorithm
{

    public class AsymmetricEncrypt
    {
        public static byte[] Encrypt(byte[] data, ref ShellInfo shellInfo)
        {
            byte[] ret = null;
            if (shellInfo.asysEncType == AsymmetricEncryptType.Rsa)
            {
                var rsa = new MRsa();
                rsa.GenerateKeys();
                ret = rsa.Encrypt(data);
                shellInfo.asPriKey = rsa.PrikeyToString();
                return ret;
            }
            else
            {
                var keyPairs = Ecc.GenKeyPair();
                ret = Ecc.encrypt(data, (ECPublicKeyParameters)keyPairs.Public);
                shellInfo.asPriKey = Ecc.privateKey2Str((ECPrivateKeyParameters)keyPairs.Private);
                return ret;
            }
        }
        public static byte[] Decrypt(byte[] data, ShellInfo shellInfo)
        {
            byte[] ret = null;
            if (shellInfo.asysEncType == AsymmetricEncryptType.Rsa)
            {
                var rsa =new MRsa();
                rsa.GenerateKeys();
                rsa.FromString(shellInfo.asPriKey);
                ret = rsa.Decrypt(data);
            }
            else
            {
                var privateKey = Ecc.str2PrivateKey(shellInfo.asPriKey);
                ret = Ecc.decrypt(data, (ECPrivateKeyParameters)privateKey);
            }
            return ret;
        }
    }
    public class SymmetricEncrypt
    {
        public static byte[]  Encrypt(byte[] data, ref ShellInfo shellInfo, byte[] key)
        {
            byte[]  ret = null;
            if (shellInfo.sysEncType == SymmetricEncryptType.Aes)
            {
                ret = Algorithm.MAes.Encrypt(data, key, ref shellInfo);
            }else if(shellInfo.sysEncType == SymmetricEncryptType.Des)
            {
                ret = Algorithm.Des.Encrypt(data, key, ref shellInfo);
            }else if(shellInfo.sysEncType==SymmetricEncryptType.Tdea)
            {
                ret = Algorithm.Tdea.Encrypt(data, key, ref shellInfo);
            }else if(shellInfo.sysEncType==SymmetricEncryptType.RC4)
            {
                var rc4 = new Algorithm.RC4(key);
                ret = rc4.Encrypt(data);
            }
            return ret;
        }
        public static byte[] Decrypt(byte[] data, ShellInfo shellInfo, byte[] key)
        {
            byte[] ret = null;
            //byte[] iv = Tool.RemoveTrailingZeros( Tool.StrToBytes(shellInfo.iv));
            byte[] iv = shellInfo.iv;

            if (shellInfo.sysEncType == SymmetricEncryptType.Aes)
            {
                ret = Algorithm.MAes.Decrypt(data, key, iv);
            }else if (shellInfo.sysEncType == SymmetricEncryptType.Des)
            {
                ret= Algorithm.Des.Decrypt(data, key, iv);
            }else if (shellInfo.sysEncType == SymmetricEncryptType.Tdea)
            {
                ret = Algorithm.Tdea.Decrypt(data, key, iv);
            }else if (shellInfo.sysEncType == SymmetricEncryptType.RC4)
            {
                var rc4 = new Algorithm.RC4(key);
                ret = rc4.Decrypt(data);
            }
            return ret;
        }
    }

    public class Des
    {
        public static byte[] Compress32To8(byte[] data)
        {
            if (data.Length != 32)
            {
                throw new ArgumentException("Input data length must be 32 bytes.");
            }

            byte[] compressedData = new byte[8];

            for (int i = 0; i < 8; i++)
            {
                byte b1 = data[i * 4];
                byte b2 = data[i * 4 + 1];
                byte b3 = data[i * 4 + 2];
                byte b4 = data[i * 4 + 3];

                compressedData[i] = (byte)(b1 ^ b2 ^ b3 ^ b4);
            }

            return compressedData;
        }
        public static byte[] Encrypt(byte[] data, byte[] key, ref ShellInfo shellInfo)
        {
            using (DES des = DES.Create())
            {
                des.Key = Compress32To8(key);
                des.GenerateIV();
               shellInfo.iv = des.IV;
                des.Mode = CipherMode.CBC;
                des.Padding = PaddingMode.PKCS7;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    CryptoStream cryptoStream = new CryptoStream(memoryStream, des.CreateEncryptor(), CryptoStreamMode.Write);
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }
        public static byte[] Decrypt(byte[] encryptedData, byte[] key, byte[] iv)
        {
            using (DES des = DES.Create())
            {
                des.Key = Compress32To8(key);
                des.IV = iv;
                des.Mode = CipherMode.CBC;
                des.Padding = PaddingMode.PKCS7;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    CryptoStream cryptoStream = new CryptoStream(memoryStream, des.CreateDecryptor(), CryptoStreamMode.Write);
                    cryptoStream.Write(encryptedData, 0, encryptedData.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }
    }
    public class Tdea
    {
        public static byte[] Compress32To24(byte[] data)
        {
            if (data.Length != 32)
            {
                throw new ArgumentException("Input data length must be 32 bytes.");
            }

            byte[] compressedData = new byte[24];

            for (int i = 0; i < 24; i++)
            {
                byte b1 = data[24 + (i % 8)];


                compressedData[i] = (byte)(b1 ^ data[i]);
            }

            return compressedData;
        }
        public static byte[] Encrypt(byte[] data, byte[] key, ref ShellInfo shellInfo)
        {
            using (TripleDES tdea = TripleDES.Create())
            {
                tdea.Key = Compress32To24(key);
                tdea.GenerateIV();
                shellInfo.iv = tdea.IV;
                tdea.Mode = CipherMode.CBC;
                tdea.Padding = PaddingMode.PKCS7;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    CryptoStream cryptoStream = new CryptoStream(memoryStream, tdea.CreateEncryptor(), CryptoStreamMode.Write);
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }
        public static byte[] Decrypt(byte[] encryptedData, byte[] key, byte[] iv)
        {
            using (TripleDES tdea = TripleDES.Create())
            {
                tdea.Key = Compress32To24(key);
                tdea.IV = iv;
                tdea.Mode = CipherMode.CBC;
                tdea.Padding = PaddingMode.PKCS7;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    CryptoStream cryptoStream = new CryptoStream(memoryStream, tdea.CreateDecryptor(), CryptoStreamMode.Write);
                    cryptoStream.Write(encryptedData, 0, encryptedData.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }
    }
    public class MAes
    {
        public static byte[]  Encrypt(byte[] data, byte[] key, ref ShellInfo shellInfo)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();
                //shellInfo.iv = Tool.BytesToStr(aes.IV);
                shellInfo.iv = new byte[aes.IV.Length]; 
                Array.Copy(aes.IV, shellInfo.iv, aes.IV.Length);

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }
        public static byte[] Decrypt(byte[] encryptedData, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write);
                    cryptoStream.Write(encryptedData, 0, encryptedData.Length);
                    //cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }
    }
    public class MRsa
    {
        private  RSACryptoServiceProvider rsa;

        public void GenerateKeys()
        {
            rsa = new RSACryptoServiceProvider(2048); // 2048Î»µÄRSAÃÜÔ¿¶Ô
        }

        public  string PubkeyToString()
        {
            return rsa.ToXmlString(false); 
        }

        public string PrikeyToString()
        {
            return rsa.ToXmlString(true);
        }
        public void FromString(string xml)
        {
            rsa.FromXmlString(xml);
        }

        public byte[] Encrypt(byte[] data)
        {
            
            return rsa.Encrypt(data, false);
        }

        public byte[] Decrypt(byte[] encryptedData )
        {

            return rsa.Decrypt(encryptedData, false);
        }
    }
    //public class BRsa
    //{
    //    public static AsymmetricCipherKeyPair GenerateKeys()
    //    {
    //        var keyPairGenerator = GeneratorUtilities.GetKeyPairGenerator("RSA");
    //        keyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
    //        var keyPair = keyPairGenerator.GenerateKeyPair();
    //        return keyPair;
    //    }
    //    public static byte[] Encrypt(byte[] data, AsymmetricKeyParameter publicKey)
    //    {
    //        var cipher = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");
    //        cipher.Init(true, publicKey);
    //        return cipher.DoFinal(data);
    //    }

    //    public static byte[] Decrypt(byte[] encryptedData, AsymmetricKeyParameter privateKey)
    //    {
    //        var cipher = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");
    //        cipher.Init(false, privateKey);
    //        return cipher.DoFinal(encryptedData);
    //    }
    //}


}


