
// http://minicryptowallet.com/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;

class Program
{
	static void Main(string[] args)
	{
		if (args.Length < 2)
		{
			Console.WriteLine("usage: file node [-pause]");
			return;
		}
		string file	= args[0];
		string url	= args[1];

		IPAddress address;
		if (IPAddress.TryParse(url, out address))
			url = "http://" + address.ToString() + ":8545/";

		try
		{
			string data = BitConverter.ToString(File.ReadAllBytes(file)).Replace("-", "").ToLower();

			var d = new Dictionary<string, object>();
			d["jsonrpc"]	= "2.0";
			d["method"]		= "eth_sendRawTransaction";
			d["params"]		= new string[]{"0x" + data};
			d["id"]			= 1;

			string json = new JavaScriptSerializer().Serialize(d);

			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
			WebClient client = new WebClient();
			client.Headers[HttpRequestHeader.ContentType] = "application/json";
			string response = client.UploadString(url, json);
			Console.WriteLine(response);
		}
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
		}

		if (args.Length > 2)
		{
			Console.Write("\nPress any key to continue...\n");
			Console.ReadKey(true);
		}
	}
}
