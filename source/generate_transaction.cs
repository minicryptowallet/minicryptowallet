
// http://minicryptowallet.com/

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

class WindowMain : Window
{
	private	int							m_passwordStrengthBit	= 64;
	private	ETHTransaction				m_tx					= new ETHTransaction();
	private	Dictionary<string, string>	m_dAddressNonce			= new Dictionary<string, string>();
	private	List<string>				m_lBlockExplorer		= new List<string>();
	private	List<string>				m_lNode					= new List<string>();

	public	Menu						m_menu					= new Menu();
	private	PasswordBox					m_passwordBoxPP			= new PasswordBox()	{VerticalContentAlignment = VerticalAlignment.Center};
	private	ComboBox					m_comboBoxNonce			= new ComboBox()	{IsEditable = true};
	private	ComboBox					m_comboBoxTo			= new ComboBox()	{IsEditable = true, MaxDropDownHeight = 1024};
	private	ComboBox					m_comboBoxValue			= new ComboBox()	{IsEditable = true, MaxDropDownHeight = 1024};
	private	TextBox						m_textBoxInitOrData		= new TextBox()		{VerticalContentAlignment = VerticalAlignment.Center, IsEnabled = false};
	private	Button						m_buttonClear			= new Button()		{Margin = new Thickness(4, 0, 0, 0), Content = "Clear"};
	private	Button						m_buttonBin				= new Button()		{Margin = new Thickness(4, 0, 0, 0), Content = "Bin"};
	private	Button						m_buttonHex				= new Button()		{Margin = new Thickness(4, 0, 0, 0), Content = "Hex"};
	private	ComboBox					m_comboBoxGasPrice		= new ComboBox()	{IsEditable = true, MaxDropDownHeight = 1024};
	private	ComboBox					m_comboBoxGasLimit		= new ComboBox()	{IsEditable = true, MaxDropDownHeight = 1024};
	private	Paragraph					m_paragraphTransaction	= new Paragraph();

	string GetCurrency()
	{
#if CURRENCY_ETH
		return "ETH";
#endif

#if CURRENCY_ETC
		return "ETC";
#endif
	}

	void InitOrDataClear()
	{
		m_tx.m_initOrData = new byte[0];
		m_textBoxInitOrData.Text = "";
		UpdateTransaction(false);
	}

	void InitOrDataLoad(bool bin)
	{
		var dlg = new Microsoft.Win32.OpenFileDialog();
		if (dlg.ShowDialog() == true)
		{
			byte[] data;

			try
			{
				if (bin)
					data = File.ReadAllBytes(dlg.FileName);
				else
				{
					data = Encode.HexToBytes(File.ReadAllText(dlg.FileName));
					if (data == null)
						throw new Exception("Invalid hex file.");
				}

				if (data.Length > 1024 * 1024)
					throw new Exception("Size larger than 1MB is not supported.");
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
				return;
			}

			m_tx.m_initOrData = data;
			m_textBoxInitOrData.Text = data.Length.ToString() + " bytes";
			UpdateTransaction(false);
		}
	}

	void PrintAddressAndBlockExplorer(string desc, byte[] address)
	{
		m_paragraphTransaction.Inlines.Add(desc + ": 0x" + Encode.BytesToHex(address));

		foreach (string url in m_lBlockExplorer)
		{
			if (!url.Contains("<address>"))
				continue;

			Uri uri = new Uri(url.Replace("<address>", Encode.BytesToHex(address)));
			if (uri.IsFile)
				continue;

			Hyperlink hyperlink = new Hyperlink(new Run(uri.Host)){NavigateUri = uri, ToolTip = uri};
			hyperlink.Click += (sender, e) => System.Diagnostics.Process.Start(uri.ToString());
			m_paragraphTransaction.Inlines.Add(" ");
			m_paragraphTransaction.Inlines.Add(hyperlink);
		}

		m_paragraphTransaction.Inlines.Add("\n");
	}

	void UpdateTransaction(bool loadNonce)
	{
		m_paragraphTransaction.Inlines.Clear();

		// Password / Private key
		BigInteger privateKey;
		if (EllipticCurve.HexToPrivateKey(m_passwordBoxPP.Password, out privateKey))
			m_passwordBoxPP.Background = Brushes.LightSkyBlue;
		else
		{
			m_passwordBoxPP.Background = Brushes.White;

			if (Password.GetKeySpace(m_passwordBoxPP.Password) < BigInteger.Pow(2, m_passwordStrengthBit))
				return;

			privateKey = Password.PasswordToPrivateKey(m_passwordBoxPP.Password);
		}

		BigInteger	publicKeyX;
		BigInteger	publicKeyY;
		BouncyCastle.ECPrivateKeyToPublicKey(privateKey, out publicKeyX, out publicKeyY);

		byte[] publicKey = EllipticCurve.PublicKeyToBytes(publicKeyX, publicKeyY);
		byte[] address = ETHAddress.PublicKeyToETHAddress(publicKey);

		PrintAddressAndBlockExplorer("From", address);

		// Nonce
		if (loadNonce)
		{
			string text = "";
			if (m_dAddressNonce.ContainsKey(Encode.BytesToHex(address)))
				text = m_dAddressNonce[Encode.BytesToHex(address)];

			Dispatcher.BeginInvoke(new Action(() => m_comboBoxNonce.Text = text));
		}

		if (!BigInteger.TryParse(m_comboBoxNonce.Text, out m_tx.m_nonce)){m_paragraphTransaction.Inlines.Add("Nonce: <Invalid>\n"); return;}
		if (m_tx.m_nonce < 0                                            ){m_paragraphTransaction.Inlines.Add("Nonce: <Invalid>\n"); return;}

		m_paragraphTransaction.Inlines.Add("Nonce: " + m_tx.m_nonce + "\n");

		// To
		string to = m_comboBoxTo.Text.Trim();
		int index = to.IndexOf(" ");
		if (index != -1)
			to = to.Substring(0, index);
		m_tx.m_to = Encode.HexToBytes(to);

		     if (m_tx.m_to        == null){m_paragraphTransaction.Inlines.Add("To: <Invalid>\n");	return;	}
		else if (m_tx.m_to.Length == 0   ){m_paragraphTransaction.Inlines.Add("To: Empty\n");				}
		else if (m_tx.m_to.Length == 20  ){PrintAddressAndBlockExplorer("To", m_tx.m_to);					}
		else                              {m_paragraphTransaction.Inlines.Add("To: <Invalid>\n");	return;	}

		// Value
		if (Encode.DecimalToBigInteger(m_comboBoxValue.Text, 18, out m_tx.m_value))
			m_paragraphTransaction.Inlines.Add("Value: " + Encode.BigIntegerToDecimal(m_tx.m_value, 18) + " " + GetCurrency() + "\n");
		else
		{
			m_paragraphTransaction.Inlines.Add("Value: <Invalid>\n");
			return;
		}

		// Init / Data
		if (m_tx.m_initOrData.Length > 0)
			m_paragraphTransaction.Inlines.Add("Init / Data: " + m_tx.m_initOrData.Length + " bytes\n");

		// Gas price
		if (!Encode.DecimalToBigInteger(m_comboBoxGasPrice.Text, 18, out m_tx.m_gasPrice))
		{
			m_paragraphTransaction.Inlines.Add("Gas price: <Invalid>\n");
			return;
		}

//		m_paragraphTransaction.Inlines.Add("Gas price: " + Encode.BigIntegerToDecimal(m_tx.m_gasPrice, 18) + " " + GetCurrency() + "\n");

		// Gas limit
		if (!BigInteger.TryParse(m_comboBoxGasLimit.Text, out m_tx.m_gasLimit)){m_paragraphTransaction.Inlines.Add("Gas limit: <Invalid>\n");	return;}
		if (m_tx.m_gasLimit < 0                                               ){m_paragraphTransaction.Inlines.Add("Gas limit: <Invalid>\n");	return;}

//		m_paragraphTransaction.Inlines.Add("Gas limit: " + m_tx.m_gasLimit + "\n");

		BigInteger gasLimitMin = m_tx.GetGasLimitMin();
		if (m_tx.m_gasLimit < gasLimitMin)
			m_paragraphTransaction.Inlines.Add(new Run("Minimal gas limit is " + gasLimitMin + ".\n"){Foreground = Brushes.Red});

		// calculation
		m_paragraphTransaction.Inlines.Add(new Run("Fee: "         + Encode.BigIntegerToDecimal(               m_tx.m_gasPrice * m_tx.m_gasLimit, 18) + " " + GetCurrency() + "\n"){ToolTip = "Fee = Gas price * Gas limit"});
		m_paragraphTransaction.Inlines.Add(new Run("Value + Fee: " + Encode.BigIntegerToDecimal(m_tx.m_value + m_tx.m_gasPrice * m_tx.m_gasLimit, 18) + " " + GetCurrency() + "\n"){ToolTip = "Fee = Gas price * Gas limit"});

		// additional check
		if (m_tx.m_to.Length == 0 && m_tx.m_initOrData.Length == 0)
			return;

		// tx
		m_tx.ECDsaSign(GetCurrency(), privateKey);
		byte[] tx = m_tx.EncodeRLP();
		byte[] hash = BouncyCastle.Keccak(tx);

		// button
		foreach (string node in m_lNode)
		{
			Button buttonSend = new Button(){Margin = new Thickness(2), Content = "Send to " + node, ToolTip = "Send raw transaction to " + node};
			buttonSend.Click += (sender, e) =>
			{
				// save
				string file = GetCurrency() + "_tx_" + DateTime.Now.ToString("yyMMdd_HHmmss") + "_0x" + Encode.BytesToHex(hash);
				File.WriteAllBytes(file, tx);

				// nonce read
				m_dAddressNonce.Clear();

				foreach (var entry in ConfigFile.ReadAddressInfo(GetCurrency() + "_nonce"))
					m_dAddressNonce[entry.Item1] = entry.Item2;

				// nonce update
				m_dAddressNonce[Encode.BytesToHex(address)] = m_tx.m_nonce.ToString();

				// nonce write
				List<string> lLine = new List<string>();
				foreach (var entry in m_dAddressNonce)
					lLine.Add("0x" + entry.Key + " " + entry.Value);
				File.WriteAllLines(GetCurrency() + "_nonce", lLine);

				// send
				System.Diagnostics.Process.Start("send_transaction.exe", file + " " + node + " -pause");
			};
			m_paragraphTransaction.Inlines.Add(buttonSend);
		}

		Button buttonCopy = new Button(){Margin = new Thickness(2), Content = "Copy", ToolTip = "Copy raw transaction to clipboard"};
		Button buttonSave = new Button(){Margin = new Thickness(2), Content = "Save", ToolTip = "Save raw transaction to file"};

		buttonCopy.Click += (sender, e) => Clipboard.SetText(Encode.BytesToHex(tx));
		buttonSave.Click += (sender, e) =>
		{
			var dlg = new Microsoft.Win32.SaveFileDialog();
			dlg.FileName = GetCurrency() + "_tx_0x" + Encode.BytesToHex(hash);
			if (dlg.ShowDialog() == true)
				File.WriteAllBytes(dlg.FileName, tx);
		};

		m_paragraphTransaction.Inlines.Add(buttonCopy);
		m_paragraphTransaction.Inlines.Add(buttonSave);
	}

	public WindowMain()
	{
		foreach (var entry in ConfigFile.ReadAddressInfo(GetCurrency() + "_nonce"))
			m_dAddressNonce[entry.Item1] = entry.Item2;

		m_lBlockExplorer	= ConfigFile.ReadTrimmedLines(GetCurrency() + "_block_explorer");
		m_lNode				= ConfigFile.ReadTrimmedLines(GetCurrency() + "_node");

		// window
		Title = GetCurrency() + " Transaction Generator - MiniCryptoWallet";
		Background = SystemColors.ControlBrush;

		foreach (string file in Directory.EnumerateFiles(Directory.GetCurrentDirectory(), GetCurrency() + "_background.*"))
		{
			BitmapImage image = new BitmapImage(new Uri(file));
			DpiScale scale = VisualTreeHelper.GetDpi(this);
			Background = new ImageBrush(image){
				TileMode = TileMode.Tile,
				ViewportUnits = BrushMappingMode.Absolute,
				Viewport = new Rect(0, 0, image.Width * image.DpiX / 96 / scale.DpiScaleX, image.Height * image.DpiY / 96 / scale.DpiScaleY)};
		}

		// control
		Grid gridInitOrData = new Grid();
		gridInitOrData.RowDefinitions.Add(new RowDefinition());
		gridInitOrData.ColumnDefinitions.Add(new ColumnDefinition());							WPF.Add(gridInitOrData, m_textBoxInitOrData);
		gridInitOrData.ColumnDefinitions.Add(new ColumnDefinition(){Width = GridLength.Auto});	WPF.Add(gridInitOrData, m_buttonClear);
		gridInitOrData.ColumnDefinitions.Add(new ColumnDefinition(){Width = GridLength.Auto});	WPF.Add(gridInitOrData, m_buttonBin);
		gridInitOrData.ColumnDefinitions.Add(new ColumnDefinition(){Width = GridLength.Auto});	WPF.Add(gridInitOrData, m_buttonHex);

		Grid grid = new Grid();
		grid.ColumnDefinitions.Add(new ColumnDefinition(){Width = GridLength.Auto});
		grid.ColumnDefinitions.Add(new ColumnDefinition());
		for (int i = 0; i < 7; i++)
			grid.RowDefinitions.Add(new RowDefinition());
		WPF.Add(grid, new Label(){HorizontalContentAlignment = HorizontalAlignment.Right, Content = "Password / Private key"});		WPF.Add(grid, m_passwordBoxPP   );
		WPF.Add(grid, new Label(){HorizontalContentAlignment = HorizontalAlignment.Right, Content = "Nonce"                 });		WPF.Add(grid, m_comboBoxNonce   );
		WPF.Add(grid, new Label(){HorizontalContentAlignment = HorizontalAlignment.Right, Content = "To"                    });		WPF.Add(grid, m_comboBoxTo      );
		WPF.Add(grid, new Label(){HorizontalContentAlignment = HorizontalAlignment.Right, Content = "Value"                 });		WPF.Add(grid, m_comboBoxValue   );
		WPF.Add(grid, new Label(){HorizontalContentAlignment = HorizontalAlignment.Right, Content = "Init / Data"           });		WPF.Add(grid, gridInitOrData    );
		WPF.Add(grid, new Label(){HorizontalContentAlignment = HorizontalAlignment.Right, Content = "Gas price"             });		WPF.Add(grid, m_comboBoxGasPrice);
		WPF.Add(grid, new Label(){HorizontalContentAlignment = HorizontalAlignment.Right, Content = "Gas limit"             });		WPF.Add(grid, m_comboBoxGasLimit);
		foreach (FrameworkElement element in grid.Children)
			element.Margin = new Thickness(0, 4, 0, 0);

		DockPanel panel = new DockPanel(){Opacity = 0.8};
		WPF.Add(panel, Dock.Top, m_menu);
		WPF.Add(panel, Dock.Top, grid);
		WPF.Add(panel, Dock.Top, new Label(){Content = GetCurrency() + " transaction"});
		WPF.Add(panel, Dock.Top, new RichTextBox(){Document = new FlowDocument(m_paragraphTransaction), IsReadOnly = true, IsDocumentEnabled = true, VerticalScrollBarVisibility = ScrollBarVisibility.Auto});

		Content = panel;

		// data and handler
		Action<bool> actionSimpleAdvanced = toggle =>
		{
			bool advanced = toggle ^ ((string)ConfigFile.RegistryGet("Advanced", "True") == "True");
			ConfigFile.RegistrySet("Advanced", advanced);

			for (int i = 4; i < grid.RowDefinitions.Count; i++)
				grid.RowDefinitions[i].MaxHeight = advanced ? Double.PositiveInfinity : 0;

			foreach (UIElement element in grid.Children)
			{
				if (Grid.GetRow(element) >= 4)
					element.IsEnabled = advanced;
			}
		};
		WPF.Add(m_menu.Items, "Simple / Advanced").Click += (sender, e) => actionSimpleAdvanced(true);
		actionSimpleAdvanced(false);

		ContextMenu menu = new ContextMenu();
		if (File.Exists(GetCurrency() + "_password_private_key"))
		{
			foreach (string line in File.ReadLines(GetCurrency() + "_password_private_key"))
				WPF.Add(menu.Items, new string('*', line.Length)).Click += (sender, e) => m_passwordBoxPP.Password = line;
		}
		if (menu.Items.Count != 0)
			menu.Items.Add(new Separator());
		WPF.Add(menu.Items, "Paste").Click += (sender, e) => m_passwordBoxPP.Paste();
		WPF.Add(menu.Items, "Clear").Click += (sender, e) => m_passwordBoxPP.Clear();
		m_passwordBoxPP.ContextMenu = menu;
		m_passwordBoxPP.PasswordChanged += (sender, e) => UpdateTransaction(true);
		m_passwordBoxPP.Focus();

		for (int i = 0; i < 1000; i++)
			m_comboBoxNonce.Items.Add(i);

		foreach (var entry in ConfigFile.ReadAddressInfo(GetCurrency() + "_to"))
			m_comboBoxTo.Items.Add(("0x" + entry.Item1 + " " + entry.Item2).Trim());

		foreach (string line in ConfigFile.ReadTrimmedLines(GetCurrency() + "_value"))
			m_comboBoxValue.Items.Add(line);

		m_buttonClear.Click	+= (sender, e) => InitOrDataClear();
		m_buttonBin.Click	+= (sender, e) => InitOrDataLoad(true);
		m_buttonHex.Click	+= (sender, e) => InitOrDataLoad(false);

		foreach (string line in ConfigFile.ReadTrimmedLines(GetCurrency() + "_gas_price"))
			m_comboBoxGasPrice.Items.Add(line);
		m_comboBoxGasPrice.SelectedIndex = 0;

		foreach (string line in ConfigFile.ReadTrimmedLines(GetCurrency() + "_gas_limit"))
			m_comboBoxGasLimit.Items.Add(line);
		m_comboBoxGasLimit.SelectedIndex = 0;

		Loaded += (_0, _1) =>
		{
			foreach (ComboBox comboBox in new ComboBox[]{m_comboBoxNonce, m_comboBoxTo, m_comboBoxValue, m_comboBoxGasPrice, m_comboBoxGasLimit})
				((TextBox)comboBox.Template.FindName("PART_EditableTextBox", comboBox)).TextChanged += (sender, e) => UpdateTransaction(false);
		};
	}
}
