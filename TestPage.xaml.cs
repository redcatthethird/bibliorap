using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using Microsoft.WindowsAPICodePack.Shell;

namespace BiblioRap
{
	/// <summary>
	/// Interaction logic for TestPage.xaml
	/// </summary>
	public partial class TestPage : Window
	{
		private FileInfo fileInfo;

		public TestPage(string Path)
		{
			InitializeComponent();
			fileInfo = new FileInfo(Path);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Displayer.Source = ShellFile.FromFilePath(fileInfo.FullName).Thumbnail.ExtraLargeBitmapSource;
		}
	}
}
