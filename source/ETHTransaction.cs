
// http://minicryptowallet.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

class ETHTransaction
{
	public BigInteger	m_nonce			= 0;
	public BigInteger	m_gasPrice		= 0;
	public BigInteger	m_gasLimit		= 0;
	public byte[]		m_to			= new byte[0];	// 0 or 20 bytes
	public BigInteger	m_value			= 0;			// wei
	public byte[]		m_initOrData	= new byte[0];

	public BigInteger	m_v;							// signature
	public BigInteger	m_r;							// signature
	public BigInteger	m_s;							// signature

	public BigInteger GetGasLimitMin()
	{
		ulong gasLimitMin = 21000;					// Gtransaction

		foreach (byte b in m_initOrData)
			gasLimitMin += (b == 0) ? 4UL : 68UL;	// Gtxdatazero Gtxdatanonzero

		return gasLimitMin;
	}

	public int GetChainId(string currency)
	{
		if (currency.ToUpper() == "ETH")return 1;	// EIP 155
		if (currency.ToUpper() == "ETC")return 61;	// EIP 155

		throw new Exception("Currency " + currency + " is unsupported.");
	}

	public void ECDsaSign(string currency, BigInteger privateKey)
	{
		int chainId = GetChainId(currency);

		m_v = new BigInteger(chainId);
		m_r = new BigInteger(0);
		m_s = new BigInteger(0);

		byte[] e = BouncyCastle.Keccak(EncodeRLP());
		byte[] sig = EllipticCurve.ECDsaSignHash(privateKey, e);

		BigInteger	publicKeyX;
		BigInteger	publicKeyY;
		BouncyCastle.ECPrivateKeyToPublicKey(privateKey, out publicKeyX, out publicKeyY);
		int recoveryId = BouncyCastle.ECCalcRecoveryId(sig.Take(32).ToArray(), sig.Skip(32).ToArray(), e, publicKeyX, publicKeyY);

		m_v = new BigInteger(chainId * 2 + 35 + recoveryId);
		m_r = Encode.BytesToBigInteger(sig.Take(32).ToArray());
		m_s = Encode.BytesToBigInteger(sig.Skip(32).ToArray());
	}

	public byte[] EncodeRLP()
	{
		var rlp = new List<byte>();

		rlp.AddRange(Encode.RLP(Encode.BigIntegerToBytes(m_nonce)));
		rlp.AddRange(Encode.RLP(Encode.BigIntegerToBytes(m_gasPrice)));
		rlp.AddRange(Encode.RLP(Encode.BigIntegerToBytes(m_gasLimit)));
		rlp.AddRange(Encode.RLP(m_to));
		rlp.AddRange(Encode.RLP(Encode.BigIntegerToBytes(m_value)));
		rlp.AddRange(Encode.RLP(m_initOrData));

		rlp.AddRange(Encode.RLP(Encode.BigIntegerToBytes(m_v)));
		rlp.AddRange(Encode.RLP(Encode.BigIntegerToBytes(m_r)));
		rlp.AddRange(Encode.RLP(Encode.BigIntegerToBytes(m_s)));

		return Encode.RLP(rlp.ToArray(), true);
	}
}
