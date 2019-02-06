
// http://minicryptowallet.com/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

class WPF
{
	public static MenuItem Add(ItemCollection items, object header)
	{
		MenuItem item = new MenuItem(){Header = header};
		items.Add(item);
		return item;
	}

	public static UIElement Add(DockPanel panel, Dock dock, UIElement element)
	{
		DockPanel.SetDock(element, dock);
		panel.Children.Add(element);
		return element;
	}

	public static UIElement Add(Grid grid, UIElement element)
	{
		Grid.SetColumn(element, grid.Children.Count % grid.ColumnDefinitions.Count);
		Grid.SetRow   (element, grid.Children.Count / grid.ColumnDefinitions.Count);
		grid.Children.Add(element);
		return element;
	}

	[STAThread]
	static void Main()
	{
		if (!BouncyCastle.IntegrityCheck())
		{
			MessageBox.Show("BouncyCastle assembly integrity check failed.");
			return;
		}

		try
		{
			WindowMain window = new WindowMain();
			window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

			window.Width		= (int)ConfigFile.RegistryGet("Width", 800);
			window.Height		= (int)ConfigFile.RegistryGet("Height", 600);
			window.FontFamily	= new FontFamily((string)ConfigFile.RegistryGet("FontFamily", "Courier New"));
			window.FontSize		= (int)ConfigFile.RegistryGet("FontSize", 12);

			MenuItem menuOptions = Add(window.m_menu.Items, "Options");
			foreach (var family in Fonts.SystemFontFamilies)
			{
				MenuItem item = Add(menuOptions.Items, family);
				item.IsChecked = family.Source == window.FontFamily.Source;
				item.Click += (sender, e) =>
				{
					window.FontFamily = family;

					foreach (MenuItem x in menuOptions.Items)
						x.IsChecked = x == item;
				};
			}
			Add(window.m_menu.Items, " - ").Click += (sender, e) => window.FontSize = Math.Max(8.0, window.FontSize / 1.1);
			Add(window.m_menu.Items, " + ").Click += (sender, e) => window.FontSize *= 1.1;

			MenuItem menuHelp = Add(window.m_menu.Items, "Help");
			Add(menuHelp.Items, "http://minicryptowallet.com/").Click += (sender, e) => System.Diagnostics.Process.Start("http://minicryptowallet.com/");

			new Application().Run(window);

			ConfigFile.RegistrySet("Width",			(int)window.Width);
			ConfigFile.RegistrySet("Height",		(int)window.Height);
			ConfigFile.RegistrySet("FontFamily",	window.FontFamily.Source);
			ConfigFile.RegistrySet("FontSize",		(int)window.FontSize);
		}
		catch (Exception e)
		{
			MessageBox.Show(e.Message);
		}
	}
}
