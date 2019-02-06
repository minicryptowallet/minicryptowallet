
// http://minicryptowallet.com/

using System;
using System.Numerics;

class Program
{
	static void Main()
	{
		ETHTransaction tx = new ETHTransaction();

		tx.m_nonce		= BigInteger.Parse("16");
		tx.m_gasPrice	= BigInteger.Parse("10000000000");
		tx.m_gasLimit	= BigInteger.Parse("21000");
		tx.m_to			= Encode.HexToBytes("1111111111111111111111111111111111111111");
		tx.m_value		= BigInteger.Parse("100");
		tx.m_initOrData	= Encode.HexToBytes("");

		tx.ECDsaSign("ETH", Encode.BytesToBigInteger(Encode.HexToBytes("1111111111111111111111111111111111111111111111111111111111111111")));

		Console.Write(Encode.BytesToHex(tx.EncodeRLP()));
	}
}
