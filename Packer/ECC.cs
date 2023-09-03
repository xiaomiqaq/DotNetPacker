using System;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;

public class ECCEncryption
{
    private static readonly SecureRandom Random = new SecureRandom();

    // 选择椭圆曲线参数，比如secp256r1
    private static readonly string CurveName = "secp256r1";

    public static (byte[], byte[]) GenerateKeys()
    {
        using (ECDsa ecdsa = ECDsa.Create()) // 默认使用 P-256 曲线
        {
            byte[] privateKeyBytes = ecdsa.ExportECPrivateKey();
            byte[] publicKeyBytes = ecdsa.ExportSubjectPublicKeyInfo();
            return (privateKeyBytes, publicKeyBytes);
        }
    }

    public static byte[] Encrypt(byte[] publicKeyBytes, byte[] dataToEncrypt)
    {
        ECPublicKeyParameters publicKeyParameters = (ECPublicKeyParameters)PublicKeyFactory.CreateKey(publicKeyBytes);
        IBasicAgreement agreement = AgreementUtilities.GetBasicAgreement("ECDH");
        agreement.Init(publicKeyParameters);

        // 生成共享密钥
        byte[] sharedSecret = agreement.CalculateAgreement(publicKeyParameters.Q).ToByteArrayUnsigned();

        // 使用共享密钥进行AES对称加密
        byte[] encryptedData;
        using (var aes = new System.Security.Cryptography.AesCryptoServiceProvider())
        {
            aes.Key = sharedSecret;
            aes.GenerateIV();
            aes.Mode = System.Security.Cryptography.CipherMode.CBC;
            aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;

            using (var encryptor = aes.CreateEncryptor())
            {
                encryptedData = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
            }
        }

        // 将IV与加密数据拼接在一起返回
        byte[] ivAndEncryptedData = new byte[aes.IV.Length + encryptedData.Length];
        Array.Copy(aes.IV, 0, ivAndEncryptedData, 0, aes.IV.Length);
        Array.Copy(encryptedData, 0, ivAndEncryptedData, aes.IV.Length, encryptedData.Length);

        return ivAndEncryptedData;
    }

    public static byte[] Decrypt(byte[] privateKeyBytes, byte[] ivAndEncryptedData)
    {
        ECPrivateKeyParameters privateKeyParameters = (ECPrivateKeyParameters)PrivateKeyFactory.CreateKey(privateKeyBytes);
        IBasicAgreement agreement = AgreementUtilities.GetBasicAgreement("ECDH");
        agreement.Init(privateKeyParameters);

        // 从加密数据中拆分出IV和加密数据
        byte[] iv = new byte[16]; // IV的长度根据AES算法选择而定，这里假设使用16字节的IV
        byte[] encryptedData = new byte[ivAndEncryptedData.Length - iv.Length];
        Array.Copy(ivAndEncryptedData, iv, iv.Length);
        Array.Copy(ivAndEncryptedData, iv.Length, encryptedData, 0, encryptedData.Length);

        // 生成共享密钥
        byte[] sharedSecret = agreement.CalculateAgreement(new ECPublicKeyParameters(privateKeyParameters.Parameters.G.Multiply(privateKeyParameters.D), privateKeyParameters.Parameters)).ToByteArrayUnsigned();

        // 使用共享密钥进行AES对称解密
        byte[] decryptedData;
        using (var aes = new System.Security.Cryptography.AesCryptoServiceProvider())
        {
            aes.Key = sharedSecret;
            aes.IV = iv;
            aes.Mode = System.Security.Cryptography.CipherMode.CBC;
            aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;

            using (var decryptor = aes.CreateDecryptor())
            {
                decryptedData = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
            }
        }

        return decryptedData;
    }
}
