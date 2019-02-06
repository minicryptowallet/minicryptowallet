
// http://minicryptowallet.com/

using System.Linq;

class ETHAddress
{
	public static byte[] PublicKeyToETHAddress(byte[] publicKey)
	{
		return BouncyCastle.Keccak(publicKey).Skip(12).ToArray();
	}
}
