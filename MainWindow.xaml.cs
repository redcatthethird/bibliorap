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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace BiblioRap
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public static string[] ScannableExtensions = ("avi" + ',' + "mp3" + ',' + "jpg,jpeg,png,gif,bmp,tiff").Split(',');

		public static int PeskyMenuItemClickCount = 0;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void HelpCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void HelpHasExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			MessageBox.Show(this,
				"Did you really think anyone can help you ? "
				+ "Hah hah hah hah... you are on your own on this, kiddo'. "
				+ "Best wishes, Red. a.k.a your worst nightmare...",
				"Useless Info",
				MessageBoxButton.OK,
				MessageBoxImage.Information);
		}

		private void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void CloseHasExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			if (MessageBox.Show(this, "Do you really want to close \"" + this.Title + "\" ?",
					"Annoying prompt", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
				this.Close();
		}

		private void peskyMenuItem_Click(object sender, RoutedEventArgs e)
		{
			PeskyMenuItemClickCount++;
			if (PeskyMenuItemClickCount >= 5)
				peskyLittleThing.Visibility = Visibility.Collapsed;
		}

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
			DirectoryInfo scanPath = new DirectoryInfo(ScanDirectory.Text);
			if (!scanPath.Exists)
			{
				MessageBox.Show(this, "The directory doesn't exist ! Please check if it is typed correctly,",
					"Directory missing !", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
				return;
			}

			List<FileInfo> files = scanPath.GetFilesSelectively(isRecursiveScan.IsChecked ?? true, ScanDirectory, ScannableExtensions);

			mediaFileList.Items.Clear();
			foreach (FileInfo file in files)
			{
				mediaFileList.Items.Add(file.Name);
			}
        }

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			throw new NotImplementedException();
		}

		private void ShowAboutDialog(object sender, RoutedEventArgs e)
		{
			(new AboutDialog()).ShowDialog();
		}

		private void BrowseButton_Click(object sender, RoutedEventArgs e)
		{
			throw new NotImplementedException();
		}
	}

	public static class ExtensionMethods
	{

		public static List<FileInfo> GetFilesSelectively(this DirectoryInfo path, bool scanRecursively, TextBox statusDisplayer = null, params string[] extensions)
		{
			List<FileInfo> files = new List<FileInfo>();

			foreach (FileInfo file in path.GetFiles())
			{
				if (statusDisplayer != null)
					statusDisplayer.Text = file.FullName;

				if (extensions.Contains(file.Extension.Replace(".", "")))
					files.Add(file);
			}

			if (scanRecursively)
				foreach (DirectoryInfo directory in path.GetDirectories())
					files.AddRange(directory.GetFilesSelectively(true, statusDisplayer, extensions));

			// TO DO: Implement exception handling for UnauthorizedAccessException

			return files;
		}

		public static List<string> GetFilesSelectivelyFromDirectory(string path, bool scanRecursively, TextBox statusDisplayer = null, params string[] extensions)
		{
			List<string> files = new List<string>();
			bool isAppropriate;

			foreach (string file in Directory.GetFiles(path))
			{
				if (statusDisplayer != null)
					statusDisplayer.Text = file;

				isAppropriate = false;
				foreach (string extension in extensions)
					if (file.EndsWith("." + extension, true, System.Globalization.CultureInfo.InvariantCulture))
					{
						isAppropriate = true;
						break;
					}
				if (isAppropriate)
					files.Add(file);
			}

			if (scanRecursively)
				foreach (string directory in Directory.GetDirectories(path))
					files.AddRange(GetFilesSelectivelyFromDirectory(directory, true, statusDisplayer, extensions));

			return files;
		}
		public static List<FileInfo> GetFilesSelectivelyFromDirectory(DirectoryInfo path, bool scanRecursively, TextBox statusDisplayer = null, params string[] extensions)
		{
			List<FileInfo> files = new List<FileInfo>();

			foreach (FileInfo file in path.GetFiles())
			{
				if (statusDisplayer != null)
					statusDisplayer.Text = file.FullName;

				if (extensions.Contains(file.Extension.Replace(".", "")))
					files.Add(file);
			}

			if (scanRecursively)
				foreach (DirectoryInfo directory in path.GetDirectories())
					files.AddRange(GetFilesSelectivelyFromDirectory(directory, true, statusDisplayer, extensions));

			return files;
		}
	}
}
