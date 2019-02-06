
// http://minicryptowallet.com/

using System;
using System.Numerics;

class Program
{
	static void Main(string[] args)
	{
		int passwordStrengthBit = 128;	// value less than 64 is strongly not recommended

		if (args.Length != 2)
		{
			Console.WriteLine("usage: charset number");
			return;
		}

		string charset = Password.GetCharset(args[0]);
		if (charset.Length == 0)
		{
			Console.WriteLine("Invalid charset.");
			return;
		}

		ulong number;
		if (!ulong.TryParse(args[1], out number))
		{
			Console.WriteLine("Invalid number.");
			return;
		}

		for (ulong i = 0; i < number; i++)
		{
			BigInteger	publicKeyX;
			BigInteger	publicKeyY;

			string password = Password.GeneratePassword(charset, BigInteger.Pow(2, passwordStrengthBit));
			BigInteger privateKey = Password.PasswordToPrivateKey(password);
			BouncyCastle.ECPrivateKeyToPublicKey(privateKey, out publicKeyX, out publicKeyY);
			byte[] publicKey = EllipticCurve.PublicKeyToBytes(publicKeyX, publicKeyY);
			byte[] address = ETHAddress.PublicKeyToETHAddress(publicKey);

			Console.WriteLine("0x" + Encode.BytesToHex(address) + " " + password);
		}
	}
}
