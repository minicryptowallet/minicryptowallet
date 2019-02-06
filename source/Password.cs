
// http://minicryptowallet.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

class Password
{
	public static string GetCharset(int start, int count)
	{
		var builder = new StringBuilder();

		foreach (char c in Enumerable.Range(start, count))
			builder.Append(c);

		return builder.ToString();
	}

	public static string GetCharsetAll       (){return GetCharset('!', 94);}
	public static string GetCharsetNumeric   (){return GetCharset('0', 10);}
	public static string GetCharsetUpperAlpha(){return GetCharset('A', 26);}
	public static string GetCharsetLowerAlpha(){return GetCharset('a', 26);}
	public static string GetCharsetSymbol    (){return string.Concat(GetCharsetAll().Except(GetCharsetNumeric()).Except(GetCharsetUpperAlpha()).Except(GetCharsetLowerAlpha()));}

	public static uint GetRandom()
	{
		var rng = new RNGCryptoServiceProvider();
		uint random = 0;

		for (int i = 0; i < 32; i++)
		{
			byte[] data = new byte[4];
			rng.GetBytes(data);

			random ^= BitConverter.ToUInt32(data, 0);
		}

		return random;
	}

	public static string GeneratePassword(string charset, BigInteger keySpaceMin)
	{
		string password = "";

		while (GetKeySpace(password) < keySpaceMin)
			password += charset[(int)(GetRandom() % charset.Length)];

		return password;
	}

	public static List<string> GetCharsetNames(string password)
	{
		var lCharsetName = new List<string>();

		if (password.Intersect(GetCharsetNumeric   ()).Count() != 0)lCharsetName.Add("Numeric"   );
		if (password.Intersect(GetCharsetUpperAlpha()).Count() != 0)lCharsetName.Add("UpperAlpha");
		if (password.Intersect(GetCharsetLowerAlpha()).Count() != 0)lCharsetName.Add("LowerAlpha");
		if (password.Intersect(GetCharsetSymbol    ()).Count() != 0)lCharsetName.Add("Symbol"    );
		if (password.Except   (GetCharsetAll       ()).Count() != 0)lCharsetName.Add("Other"     );

		return lCharsetName;
	}

	public static string GetCharset(string password)
	{
		string charset = "";

		if (password.Intersect(GetCharsetNumeric   ()).Count() != 0)charset += GetCharsetNumeric   ();
		if (password.Intersect(GetCharsetUpperAlpha()).Count() != 0)charset += GetCharsetUpperAlpha();
		if (password.Intersect(GetCharsetLowerAlpha()).Count() != 0)charset += GetCharsetLowerAlpha();
		if (password.Intersect(GetCharsetSymbol    ()).Count() != 0)charset += GetCharsetSymbol    ();

		return charset;
	}

	public static BigInteger GetKeySpace(string password)
	{
		if (password.Except(GetCharsetAll()).Count() != 0)
			return -1;

		return BigInteger.Pow(GetCharset(password).Length, password.Length);
	}

	public static BigInteger PasswordToPrivateKey(string password)
	{
		// change salt for improved security
		// reference https://en.wikipedia.org/wiki/Key_stretching
		// reference https://en.wikipedia.org/wiki/Salt_(cryptography)
		string salt = "Cryptography";

		var pbkdf2 = new Rfc2898DeriveBytes(
			Encoding.UTF8.GetBytes(password),
			Encoding.UTF8.GetBytes(salt),
			4096);

		BigInteger privateKey = Encode.BytesToBigInteger(pbkdf2.GetBytes(32));

		if (!EllipticCurve.IsPrivateKeyValid(privateKey))
			throw new Exception("privateKey is invalid.");	// extreme low probability to happen

		return privateKey;
	}
}
