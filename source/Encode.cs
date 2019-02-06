
// http://minicryptowallet.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

class Encode
{
	public static byte[] BigIntegerToBytes(BigInteger value, int totalLength = -1)
	{
		if (value < 0)
			throw new Exception("Negative value is not supported.");

		byte[] data = value.ToByteArray().Reverse().ToArray();

		if (data[0] == 0)
			data = data.Skip(1).ToArray();

		if (totalLength >= 0)
		{
			if (totalLength < data.Length)
				throw new Exception("totalLength can't be met.");

			data = new byte[totalLength - data.Length].Concat(data).ToArray();
		}

		return data;
	}

	public static BigInteger BytesToBigInteger(byte[] data)
	{
		return new BigInteger(new byte[1].Concat(data).Reverse().ToArray());
	}

	public static byte[] RLP(byte[] data, bool list = false)
	{
		if (!list)
		if (data.Length == 1)
		if (data[0] < 0x80)
			return new byte[]{data[0]};							// 0x00 - 0x7f

		var rlp = new List<byte>();

		if (data.Length < 0x38)
		{
			if (list)
				rlp.Add(Convert.ToByte(0xc0 + data.Length));	// 0xc0 - 0xf7
			else
				rlp.Add(Convert.ToByte(0x80 + data.Length));	// 0x80 - 0xb7
		}
		else
		{
			byte[] length = BigIntegerToBytes(new BigInteger(data.Length));

			if (list)
				rlp.Add(Convert.ToByte(0xf7 + length.Length));	// 0xf8 - 0xff
			else
				rlp.Add(Convert.ToByte(0xb7 + length.Length));	// 0xb8 - 0xbf

			rlp.AddRange(length);
		}

		rlp.AddRange(data);

		return rlp.ToArray();
	}

	public static string BytesToHex(byte[] data)
	{
		return BitConverter.ToString(data).Replace("-", "").ToLower();
	}

	public static byte[] HexToBytes(string hex, int totalLength = -1)
	{
		hex = hex.ToLower();

		if (hex.StartsWith("0x"))
			hex = hex.Substring(2);

		if (hex.Except("0123456789abcdef").Count() != 0)
			return null;

		if (hex.Length % 2 != 0)
			return null;

		byte[] data = new byte[hex.Length / 2];

		for (int i = 0; i < hex.Length / 2; i++)
			data[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);

		if (totalLength >= 0)
		{
			if (totalLength != data.Length)
				return null;
		}

		return data;
	}

	public static bool DecimalToBigInteger(string s, int decimals, out BigInteger value)
	{
		try
		{
			decimal d = decimal.Parse(s);	// may throw exception

			for (int i = 0; i < decimals; i++)
				d *= 10;					// may throw exception

			if (d != decimal.Round(d))throw new Exception();
			if (d < 0                )throw new Exception();

			value = new BigInteger(d);

			return true;
		}
		catch (Exception)
		{
		}

		value = 0;

		return false;
	}

	public static string BigIntegerToDecimal(BigInteger value, int decimals)
	{
		if (value < 0)
			throw new Exception("Negative value is not supported.");

		string s = value.ToString();

		while (s.Length <= decimals)
			s = "0" + s;

		int lengthLeft = s.Length - decimals;

		s = s.Substring(0, lengthLeft) + "." + s.Substring(lengthLeft);
		s = s.TrimEnd('0');
		s = s.TrimEnd('.');

		return s;
	}
}
