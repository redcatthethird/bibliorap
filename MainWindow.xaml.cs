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
using System.Windows.Threading;
using System.IO;

namespace BiblioRap
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public static string[] ScannableExtensions = ("avi" + ',' + "mp3" + ',' + "jpg,jpeg,png,gif,bmp,tiff").Split(',');

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

		private void ShowAboutDialog(object sender, RoutedEventArgs e)
		{
			(new AboutDialog()).ShowDialog();
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

			List<FileInfo> files = scanPath.GetFilesSelectively(isRecursiveScan.IsChecked ?? true, ScanLabel, ScanProgressBar, ScannableExtensions);

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
		private void BrowseButton_Click(object sender, RoutedEventArgs e)
		{
			throw new NotImplementedException();
		}
	}

	public static class ExtensionMethods
	{
		public static List<FileInfo> GetFilesSelectively(this DirectoryInfo path, bool scanRecursively, TextBlock statusDisplayer, ProgressBar statusProgress, params string[] extensions)
		{
			List<FileInfo> files = new List<FileInfo>();

			FileInfo[] allFiles;
			try
			{
				allFiles = path.GetFiles();
			}
			catch (UnauthorizedAccessException)
			{
				return new List<FileInfo>();
			}

			uint fileCount = 0;
			if (statusProgress != null)
				statusProgress.Maximum = path.GetFileCountSelectively(scanRecursively, extensions);

			foreach (FileInfo file in allFiles)
			{
				fileCount++;
				if (statusDisplayer != null)
				{
					statusDisplayer.Text = file.FullName;
					statusDisplayer.Refresh();
				}
				if (statusProgress != null)
				{
					statusProgress.Value = fileCount;
					statusDisplayer.Refresh();
				}

				if (extensions.Contains(file.Extension.Replace(".", "")))
					files.Add(file);
			}

			if (scanRecursively)
				foreach (DirectoryInfo directory in path.GetDirectories())
					files.AddRange(directory.GetFilesSelectively(true, statusDisplayer, statusProgress, extensions));

			return files;
		}
		public static List<string> GetFilesSelectivelyFromDirectory(string path, bool scanRecursively, TextBlock statusDisplayer, params string[] extensions)
		{
			List<string> files = new List<string>();
			bool isAppropriate;

			string[] allFiles;
			try
			{
				allFiles = Directory.GetFiles(path);
			}
			catch (UnauthorizedAccessException)
			{
				return new List<string>();
			}

			foreach (string file in allFiles)
			{
				if (statusDisplayer != null)
				{
					statusDisplayer.Text = file;
					statusDisplayer.Refresh();
				}

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
		public static List<FileInfo> GetFilesSelectivelyFromDirectory(DirectoryInfo path, bool scanRecursively, TextBlock statusDisplayer, params string[] extensions)
		{
			List<FileInfo> files = new List<FileInfo>();

			FileInfo[] allFiles;
			try
			{
				allFiles = path.GetFiles();
			}
			catch (UnauthorizedAccessException)
			{
				return new List<FileInfo>();
			}

			foreach (FileInfo file in allFiles)
			{
				if (statusDisplayer != null)
				{
					statusDisplayer.Text= file.FullName;
					statusDisplayer.Refresh();
				}

				if (extensions.Contains(file.Extension.Replace(".", "")))
					files.Add(file);
			}

			if (scanRecursively)
				foreach (DirectoryInfo directory in path.GetDirectories())
					files.AddRange(GetFilesSelectivelyFromDirectory(directory, true, statusDisplayer, extensions));

			return files;
		}

		public static int GetFileCountSelectively(this DirectoryInfo path, bool scanRecursively, params string[] extensions)
		{
			int fileCount = 0;

			FileInfo[] allFiles;
			try
			{
				allFiles = path.GetFiles();
			}
			catch (UnauthorizedAccessException)
			{
				return 0;
			}

			foreach (FileInfo file in allFiles)
				if (extensions.Contains(file.Extension.Replace(".", "")))
					fileCount++;

			if (scanRecursively)
				foreach (DirectoryInfo directory in path.GetDirectories())
					fileCount += directory.GetFileCountSelectively(true, extensions);

			return fileCount;
		}
		public static int GetFileCountSelectivelyFromDirectory(string path, bool scanRecursively, params string[] extensions)
		{
			int fileCount = 0;
			bool isAppropriate;

			string[] allFiles;
			try
			{
				allFiles = Directory.GetFiles(path);
			}
			catch (UnauthorizedAccessException)
			{
				return 0;
			}

			foreach (string file in allFiles)
			{
				isAppropriate = false;
				foreach (string extension in extensions)
					if (file.EndsWith("." + extension, true, System.Globalization.CultureInfo.InvariantCulture))
					{
						isAppropriate = true;
						break;
					}
				if (isAppropriate)
					fileCount++;
			}

			if (scanRecursively)
				foreach (string directory in Directory.GetDirectories(path))
					fileCount += GetFileCountSelectivelyFromDirectory(directory, true, extensions);

			return fileCount;
		}
		public static int GetFileCountSelectivelyFromDirectory(DirectoryInfo path, bool scanRecursively, params string[] extensions)
		{
			int fileCount = 0;

			FileInfo[] allFiles;
			try
			{
				allFiles = path.GetFiles();
			}
			catch (UnauthorizedAccessException)
			{
				return 0;
			}

			foreach (FileInfo file in allFiles)
				if (extensions.Contains(file.Extension.Replace(".", "")))
					fileCount++;

			if (scanRecursively)
				foreach (DirectoryInfo directory in path.GetDirectories())
					fileCount += GetFileCountSelectivelyFromDirectory(directory, true, extensions);

			return fileCount;
		}


		private static Action EmptyDelegate = delegate() { };
		public static void Refresh(this UIElement elem)
		{
			elem.Dispatcher.Invoke(EmptyDelegate, DispatcherPriority.Render);
		}
	}
}
