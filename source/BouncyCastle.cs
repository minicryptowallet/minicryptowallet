
// http://minicryptowallet.com/

using System;
using System.IO;
using System.Linq;
using System.Numerics;

class BouncyCastle
{
	public static bool IntegrityCheck()
	{
		string file = typeof(Org.BouncyCastle.Math.BigInteger).Assembly.Location;
		byte[] buffer = File.ReadAllBytes(file);
		byte[] hash = new System.Security.Cryptography.SHA256Managed().ComputeHash(buffer);

		return Encode.BytesToHex(hash) == "83ba441c5572bba81381427c18ae36eeb9c8b831e51edd449a54a31838a5577d";
	}

	public static void ECPrivateKeyToPublicKey(
		BigInteger		privateKey,
		out BigInteger	publicKeyX,
		out BigInteger	publicKeyY)
	{
		if (!EllipticCurve.IsPrivateKeyValid(privateKey))
			throw new Exception("privateKey is invalid.");

		var parameters = Org.BouncyCastle.Asn1.Sec.SecNamedCurves.GetByName("secp256k1");
		var d = new Org.BouncyCastle.Math.BigInteger(privateKey.ToString());
		var Q = parameters.G.Multiply(d).Normalize();

		publicKeyX = BigInteger.Parse(Q.XCoord.ToBigInteger().ToString());
		publicKeyY = BigInteger.Parse(Q.YCoord.ToBigInteger().ToString());
	}

	public static int ECCalcRecoveryId(
		byte[]		r,	// 32 bytes
		byte[]		s,	// 32 bytes
		byte[]		e,	// 32 bytes
		BigInteger	publicKeyX,
		BigInteger	publicKeyY)
	{
		var parameters	= Org.BouncyCastle.Asn1.Sec.SecNamedCurves.GetByName("secp256k1");

		var r_			= new Org.BouncyCastle.Math.BigInteger(1, r);
		var s_			= new Org.BouncyCastle.Math.BigInteger(1, s);
		var e_			= new Org.BouncyCastle.Math.BigInteger(1, e);
		var publicKeyX_	= new Org.BouncyCastle.Math.BigInteger(publicKeyX.ToString());
		var publicKeyY_	= new Org.BouncyCastle.Math.BigInteger(publicKeyY.ToString());

		var Q = parameters.Curve.CreatePoint(publicKeyX_, publicKeyY_).Normalize();
		if (!Q.IsValid())
			throw new Exception("Invalid publicKey.");

		for (int recoveryId = 0; recoveryId < 2; recoveryId++)
		{
			// sR
			byte[] encoded = new byte[1]{Convert.ToByte(2 + recoveryId)}.Concat(r).ToArray();
			byte[] left = parameters.Curve.DecodePoint(encoded).Multiply(s_).Normalize().GetEncoded(true);

			// eG + rQ
			byte[] right = parameters.G.Multiply(e_).Add(Q.Multiply(r_)).Normalize().GetEncoded(true);

			// compare
			if (left.SequenceEqual(right))
				return recoveryId;
		}

		throw new Exception("recoveryId is not found.");
	}

	public static byte[] Keccak(byte[] data)
	{
		var digest = new Org.BouncyCastle.Crypto.Digests.KeccakDigest(256);

		digest.BlockUpdate(data, 0, data.Length);

		byte[] hash = new byte[32];
		digest.DoFinal(hash, 0);

		return hash;
	}
}
