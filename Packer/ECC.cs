using System;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;

public class ECCEncryption
{
    private static readonly SecureRandom Random = new SecureRandom();

    // ѡ����Բ���߲���������secp256r1
    private static readonly string CurveName = "secp256r1";

    public static (byte[], byte[]) GenerateKeys()
    {
        using (ECDsa ecdsa = ECDsa.Create()) // Ĭ��ʹ�� P-256 ����
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

        // ���ɹ�����Կ
        byte[] sharedSecret = agreement.CalculateAgreement(publicKeyParameters.Q).ToByteArrayUnsigned();

        // ʹ�ù�����Կ����AES�ԳƼ���
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

        // ��IV���������ƴ����һ�𷵻�
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

        // �Ӽ��������в�ֳ�IV�ͼ�������
        byte[] iv = new byte[16]; // IV�ĳ��ȸ���AES�㷨ѡ��������������ʹ��16�ֽڵ�IV
        byte[] encryptedData = new byte[ivAndEncryptedData.Length - iv.Length];
        Array.Copy(ivAndEncryptedData, iv, iv.Length);
        Array.Copy(ivAndEncryptedData, iv.Length, encryptedData, 0, encryptedData.Length);

        // ���ɹ�����Կ
        byte[] sharedSecret = agreement.CalculateAgreement(new ECPublicKeyParameters(privateKeyParameters.Parameters.G.Multiply(privateKeyParameters.D), privateKeyParameters.Parameters)).ToByteArrayUnsigned();

        // ʹ�ù�����Կ����AES�Գƽ���
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
