
// http://minicryptowallet.com/

using System;
using System.Collections.Generic;
using System.IO;

class ConfigFile
{
	public static List<string> ReadTrimmedLines(string file)
	{
		var lLine = new List<string>();

		if (!File.Exists(file))
			return lLine;

		foreach (string line in File.ReadLines(file))
		{
			string s = line.Trim();
			s = s.Replace("\t", " ");

			if (s == ""          )continue;
			if (s.StartsWith("#"))continue;

			lLine.Add(s);
		}

		return lLine;
	}

	public static List<Tuple<string, string>> ReadAddressInfo(string file)
	{
		var lEntry = new List<Tuple<string, string>>();

		foreach (string line in ReadTrimmedLines(file))
		{
			// format: address [info]

			string address	= line;
			string desc		= "";

			int index = line.IndexOf(" ");
			if (index != -1)
			{
				address	= line.Substring(0, index);
				desc	= line.Substring(index).Trim();
			}

			byte[] data = Encode.HexToBytes(address, 20);
			if (data != null)
				lEntry.Add(new Tuple<string, string>(Encode.BytesToHex(data), desc));
		}

		return lEntry;
	}

	public static object RegistryGet(string name, object valueDefault)
	{
		object value = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\" + AppDomain.CurrentDomain.FriendlyName).GetValue(name);
		return (value != null) ? value : valueDefault;
	}

	public static void RegistrySet(string name, object value)
	{
		Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\" + AppDomain.CurrentDomain.FriendlyName).SetValue(name, value);
	}
}
