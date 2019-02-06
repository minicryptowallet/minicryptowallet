
// http://minicryptowallet.com/

using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

class EllipticCurve		// secp256k1
{
	public static bool IsPrivateKeyValid(BigInteger privateKey)
	{
		BigInteger n = BigInteger.Parse("00fffffffffffffffffffffffffffffffebaaedce6af48a03bbfd25e8cd0364141", System.Globalization.NumberStyles.HexNumber);

		if (privateKey <= 0)return false;
		if (privateKey >= n)return false;

		return true;
	}

	public static bool HexToPrivateKey(string hex, out BigInteger privateKey)
	{
		privateKey = 0;

		byte[] data = Encode.HexToBytes(hex, 32);
		if (data == null)
			return false;

		privateKey = Encode.BytesToBigInteger(data);

		if (!IsPrivateKeyValid(privateKey))
			return false;

		return true;
	}

	public static byte[] ECDsaSignHash(BigInteger privateKey, byte[] hash)
	{
		if (!IsPrivateKeyValid(privateKey))
			throw new Exception("privateKey is invalid.");

		ECDsa ecdsa = ECDsa.Create(ECCurve.CreateFromFriendlyName("secp256k1"));	// windows 10: https://docs.microsoft.com/en-us/windows/desktop/SecCNG/cng-named-elliptic-curves
		ECParameters parameters = ecdsa.ExportExplicitParameters(true);
		parameters.D = Encode.BigIntegerToBytes(privateKey, 32);
		// parameters.Q is not used in SignHash
		ecdsa.ImportParameters(parameters);

		while (true)
		{
			byte[] sig = ecdsa.SignHash(hash);	// 64 bytes

			if (sig[32] < 0x7f)	// low s value
				return sig;
		}
	}

	public static byte[] PublicKeyToBytes(BigInteger publicKeyX, BigInteger publicKeyY)
	{
		return Encode.BigIntegerToBytes(publicKeyX, 32).Concat(Encode.BigIntegerToBytes(publicKeyY, 32)).ToArray();
	}
}
