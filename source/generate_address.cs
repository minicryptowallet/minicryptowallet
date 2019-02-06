
// http://minicryptowallet.com/

using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

class WindowMain : Window
{
	private	int			m_passwordStrengthBit	= 128;	// value less than 64 is strongly not recommended

	public	Menu		m_menu					= new Menu();
	private	TextBox		m_textBoxPP				= new TextBox();
	private	Paragraph	m_paragraphAddress		= new Paragraph();
	private	Button		m_buttonSave			= new Button(){Content = "Save"};

	void GeneratePassword(string charset)
	{
		m_textBoxPP.Text = Password.GeneratePassword(charset, BigInteger.Pow(2, m_passwordStrengthBit));
		m_textBoxPP.Focus();
		m_textBoxPP.CaretIndex = m_textBoxPP.Text.Length;
	}

	void Output(BigInteger privateKey)
	{
		BigInteger	publicKeyX;
		BigInteger	publicKeyY;
		BouncyCastle.ECPrivateKeyToPublicKey(privateKey, out publicKeyX, out publicKeyY);

		byte[] publicKey = EllipticCurve.PublicKeyToBytes(publicKeyX, publicKeyY);
		byte[] address = ETHAddress.PublicKeyToETHAddress(publicKey);

		m_paragraphAddress.Inlines.Add("Private key (confidential): 0x" + Encode.BytesToHex(Encode.BigIntegerToBytes(privateKey, 32)) + "\n");
		m_paragraphAddress.Inlines.Add("Public key: 0x"                 + Encode.BytesToHex(publicKey)                                + "\n");
		m_paragraphAddress.Inlines.Add("ETH address: 0x"                + Encode.BytesToHex(address)                                  + "\n");
		m_paragraphAddress.Inlines.Add("ETC address: 0x"                + Encode.BytesToHex(address)                                  + "\n");
	}

	void PPChanged(object sender, TextChangedEventArgs e)
	{
		m_textBoxPP.Background = Brushes.White;
		m_paragraphAddress.Inlines.Clear();

		// empty
		if (m_textBoxPP.Text == "")
		{
			m_paragraphAddress.Inlines.Add(
				"Select any charset from menu to generate random password with " + m_passwordStrengthBit + "-bit strength.\n"
				+ "ETH and ETC addresses are derived from the password.\n\n"
				+ "Procedures:\n"
				+ "random password -> EC private key -> EC public key -> ETH and ETC addresses\n");
			return;
		}

		// private key
		BigInteger privateKey;
		if (EllipticCurve.HexToPrivateKey(m_textBoxPP.Text, out privateKey))
		{
			m_textBoxPP.Background = Brushes.LightSkyBlue;
			Output(privateKey);
			return;
		}

		// password
		BigInteger keySpace = Password.GetKeySpace(m_textBoxPP.Text);
		BigInteger keySpaceMin = BigInteger.Pow(2, m_passwordStrengthBit);

		if (keySpace < 0)
			m_paragraphAddress.Inlines.Add("Password contains invalid character.\n");
		else if (keySpace < keySpaceMin)
		{
			m_paragraphAddress.Inlines.Add("Current password charset: "  + string.Join(" ", Password.GetCharsetNames(m_textBoxPP.Text)) + "\n");
			m_paragraphAddress.Inlines.Add("Current Password length: "   + m_textBoxPP.Text.Length                                      + "\n");
			m_paragraphAddress.Inlines.Add("Current Password strength: " + keySpace                                                     + "\n");
			m_paragraphAddress.Inlines.Add("Minimum password strength: " + keySpaceMin                                                  + "\n");
		}
		else
		{
			m_paragraphAddress.Inlines.Add("Password (confidential): " + m_textBoxPP.Text + "\n");
			Output(Password.PasswordToPrivateKey(m_textBoxPP.Text));
		}
	}

	void Save(object sender, RoutedEventArgs e)
	{
		var dlg = new Microsoft.Win32.SaveFileDialog();
		if (dlg.ShowDialog() == true)
			File.WriteAllText(dlg.FileName, new TextRange(m_paragraphAddress.ContentStart, m_paragraphAddress.ContentEnd).Text);
	}

	public WindowMain()
	{
		Title = "ETH ETC Address Generator - MiniCryptoWallet";
		Background = SystemColors.ControlBrush;

		WPF.Add(m_menu.Items, "Num"                   ).Click += (sender, e) => GeneratePassword(Password.GetCharsetNumeric());
		WPF.Add(m_menu.Items, "Upper"                 ).Click += (sender, e) => GeneratePassword(Password.GetCharsetUpperAlpha());
		WPF.Add(m_menu.Items, "Lower"                 ).Click += (sender, e) => GeneratePassword(Password.GetCharsetLowerAlpha());
		WPF.Add(m_menu.Items, "Num+Upper"             ).Click += (sender, e) => GeneratePassword(Password.GetCharsetNumeric() + Password.GetCharsetUpperAlpha());
		WPF.Add(m_menu.Items, "Num+Lower"             ).Click += (sender, e) => GeneratePassword(Password.GetCharsetNumeric() + Password.GetCharsetLowerAlpha());
		WPF.Add(m_menu.Items, "Upper+Lower"           ).Click += (sender, e) => GeneratePassword(Password.GetCharsetUpperAlpha() + Password.GetCharsetLowerAlpha());
		WPF.Add(m_menu.Items, "Num+Upper+Lower"       ).Click += (sender, e) => GeneratePassword(Password.GetCharsetNumeric() + Password.GetCharsetUpperAlpha() + Password.GetCharsetLowerAlpha());
		WPF.Add(m_menu.Items, "Num+Upper+Lower+Symbol").Click += (sender, e) => GeneratePassword(Password.GetCharsetNumeric() + Password.GetCharsetUpperAlpha() + Password.GetCharsetLowerAlpha() + Password.GetCharsetSymbol());
		m_menu.Items.Add(new Separator());

		DockPanel panel = new DockPanel();
		WPF.Add(panel, Dock.Top, m_menu);
		WPF.Add(panel, Dock.Top, new Label(){Content = "Password / Private key"});
		WPF.Add(panel, Dock.Top, m_textBoxPP);
		WPF.Add(panel, Dock.Top, new Label(){Content = "Address"});
		WPF.Add(panel, Dock.Bottom, m_buttonSave);
		WPF.Add(panel, Dock.Top, new RichTextBox(){Document = new FlowDocument(m_paragraphAddress), IsReadOnly = true, IsReadOnlyCaretVisible = true, VerticalScrollBarVisibility = ScrollBarVisibility.Auto});

		m_textBoxPP.TextChanged += PPChanged;
		m_buttonSave.Click += Save;

		m_textBoxPP.Focus();
		PPChanged(null, null);

		Content = panel;
	}
}
