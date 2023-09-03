using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using System;

namespace Algorithm
{
	public class Ecc
	{
		private const string SIGN_ALGORITHM = "SHA256withECDSA"; //"SM3withSM2";
		private static readonly SecureRandom random = new SecureRandom();
		private static readonly ECDomainParameters domainParameters;
		private readonly ECPrivateKeyParameters privateKey;
		private readonly ECPublicKeyParameters publicKey;

		static Ecc()
		{
			DerObjectIdentifier oid = X9ObjectIdentifiers.Prime256v1;
			X9ECParameters ecps = CustomNamedCurves.GetByOid(oid);
			domainParameters = new ECDomainParameters(ecps.Curve, ecps.G, ecps.N, ecps.H, ecps.GetSeed());
		}

		private Ecc(ECPrivateKeyParameters privateKey, ECPublicKeyParameters publicKey)
		{
			this.privateKey = privateKey;
			this.publicKey = publicKey;
		}

		public static Ecc instance(AsymmetricCipherKeyPair keyPair)
		{
			return new Ecc((ECPrivateKeyParameters)keyPair.Private, (ECPublicKeyParameters)keyPair.Public);
		}

		public static Ecc instance()
		{
			AsymmetricCipherKeyPair keyPair = GenKeyPair();
			return new Ecc((ECPrivateKeyParameters)keyPair.Private, (ECPublicKeyParameters)keyPair.Public);
		}

		//产生EC公私钥对
		public static AsymmetricCipherKeyPair GenKeyPair()
		{
			ECKeyGenerationParameters ecKeyGenerationParameters = new ECKeyGenerationParameters(domainParameters, random);
			ECKeyPairGenerator gen = new ECKeyPairGenerator();

			gen.Init(ecKeyGenerationParameters);
			return gen.GenerateKeyPair();
		}

		/// <summary>
		/// 签名
		/// </summary>
		/// <param name="content">待签名内容</param>
		/// <param name="privateKey">EC私钥</param>
		/// <returns></returns>
		public static byte[] sign(byte[] content, ECPrivateKeyParameters privateKey)
		{
			ISigner signer = SignerUtilities.GetSigner(SIGN_ALGORITHM);

			signer.Init(true, new ParametersWithRandom(privateKey, random));
			signer.BlockUpdate(content, 0, content.Length);

			return signer.GenerateSignature();
		}

		/// <summary>
		/// 验签
		/// </summary>
		/// <param name="content">待验签内容</param>
		/// <param name="signature">待比较的签名结果</param>
		/// <param name="publicKey">EC公钥</param>
		/// <returns></returns>
		public static bool verify(byte[] content, byte[] signature, ECPublicKeyParameters publicKey)
		{
			ISigner signer = SignerUtilities.GetSigner(SIGN_ALGORITHM);

			signer.Init(false, publicKey);
			signer.BlockUpdate(content, 0, content.Length);

			return signer.VerifySignature(signature);
		}

		//使用SM2（国密2）算法解密数据
		public static byte[] decrypt(byte[] cipherText, ECPrivateKeyParameters privateKey)
		{
			SM2Engine sm2Engine = new SM2Engine();
			sm2Engine.Init(false, privateKey);
			return sm2Engine.ProcessBlock(cipherText, 0, cipherText.Length);
		}

		//使用SM2（国密2）算法加密数据
		public static byte[] encrypt(byte[] plainText, ECPublicKeyParameters publicKey)
		{
			SM2Engine sm2Engine = new SM2Engine();
			sm2Engine.Init(true, new ParametersWithRandom(publicKey, random));
			return sm2Engine.ProcessBlock(plainText, 0, plainText.Length);
		}

		/**
		 * convert a privateKey to string
		 * @param privateKey 私钥
		 * @return 私钥字符串，以base64编码
		 */
		public static string privateKey2Str(ECPrivateKeyParameters privateKey)
		{
			return Convert.ToBase64String(privateKey2Bytes(privateKey));
		}

		public static byte[] privateKey2Bytes(ECPrivateKeyParameters privateKey)
		{
			return privateKey.D.ToByteArray();
		}

		/**
		 * convert the privateKey to string
		 * @return 私钥字符串，以base64编码
		 */
		public string privateKey2Str()
		{
			return privateKey2Str(this.privateKey);
		}

		/**
		 * 将私钥从字符串转为私钥对象
		 * @param content 私钥字符串，标准base64格式
		 * @return 私钥
		 */
		public static ECPrivateKeyParameters str2PrivateKey(string privateKey)
		{
			byte[] publicKeyBytes = Convert.FromBase64String(privateKey);
			return bytes2PrivateKey(publicKeyBytes);
		}

		public static ECPrivateKeyParameters bytes2PrivateKey(byte[] privateKey)
		{
			BigInteger D = new BigInteger(1, privateKey);
			return new ECPrivateKeyParameters(D, domainParameters);
		}

		/**
		 * convert a publicKey to string
		 * @param publicKey 公钥
		 * @return 公钥字符串，以base64格式返回
		 */
		public static string publicKey2Str(ECPublicKeyParameters publicKey)
		{
			BigInteger x = publicKey.Q.AffineXCoord.ToBigInteger();
			BigInteger y = publicKey.Q.AffineYCoord.ToBigInteger();
			byte[] bX = x.ToByteArray();
			byte[] bY = y.ToByteArray();
			int xLen = bX.Length;
			byte[] key = new byte[1 + xLen + bY.Length];
			key[0] = (byte)xLen; //EC256的情况为32
			Array.Copy(bX, 0, key, 1, xLen);
			Array.Copy(bY, 0, key, 1 + xLen, bY.Length);

			return Convert.ToBase64String(key);
		}

		public string publicKey2Str()
		{
			return publicKey2Str(this.publicKey);
		}

		/**
		 * 将公钥从字符串转为私钥对象
		 * @param content 公钥字符串，标准base64格式
		 * @return 公钥
		 */
		public static ECPublicKeyParameters str2PublicKey(string publicKey)
		{
			byte[] key = Convert.FromBase64String(publicKey);
			return bytes2PublicKey(key);
		}

		public static ECPublicKeyParameters bytes2PublicKey(byte[] key)
		{
			int xLen = (key[0]) & 0xff;
			if (xLen >= key.Length)
			{
				throw new ArgumentException("Invalid key data");
			}
			byte[] x = new byte[xLen];
			Array.Copy(key, 1, x, 0, xLen);
			byte[] y = new byte[key.Length - 1 - xLen];
			Array.Copy(key, 1 + xLen, y, 0, y.Length);
			ECPoint Q = domainParameters.Curve.ValidatePoint(new BigInteger(1, x), new BigInteger(1, y));
			return new ECPublicKeyParameters(Q, domainParameters);
		}
	}
}