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
using System.Threading;
using System.IO;
using Microsoft.Win32;
using WF = System.Windows.Forms;
using System.Diagnostics;
using System.Security;

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
			string path = ScanDirectory.Text.Trim();
			bool recursive = isRecursiveScan.IsChecked ?? false;
			if (path == "*")
			{
				string[] drives = Directory.GetLogicalDrives();
				foreach (string drive in drives)
				{
					(new DirectoryInfo(drive)).GetFilesSelectively(recursive, ScanLabel, ScanProgressBar, mediaFileList, ScannableExtensions);
				}
			}
			else
			{
				DirectoryInfo scanPath = new DirectoryInfo(path);
				if (!scanPath.Exists)
				{
					MessageBox.Show(this, "The directory doesn't exist ! Please check if it is typed correctly.",
						"Directory missing !", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
					return;
				}

				mediaFileList.Items.Clear();
				scanPath.GetFilesSelectively(recursive, ScanLabel, ScanProgressBar, mediaFileList, ScannableExtensions);
			}
        }
		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog sfDlg = new SaveFileDialog();
			sfDlg.AddExtension = true;
			sfDlg.CheckPathExists = true;
			sfDlg.DefaultExt = ".txt";
			sfDlg.DereferenceLinks = true;
			sfDlg.Filter = "Text files|*.txt|All files|*.*";
			sfDlg.OverwritePrompt = true;
			sfDlg.Title = "Save file list";
			sfDlg.ValidateNames = true;

			if (sfDlg.ShowDialog() == true)
			{
				using (TextWriter writer = new StreamWriter(sfDlg.FileName))
				{
					if (showFullPath.IsChecked ?? false)
					{
						foreach (FileInfo file in mediaFileList.Items)
						{
							writer.WriteLine(file.FullName);
						}
					}
					else
					{
						foreach (FileInfo file in mediaFileList.Items)
						{
							writer.WriteLine(file.Name);
						}
					}
				}
			}
		}
		private void BrowseButton_Click(object sender, RoutedEventArgs e)
		{
			WF.FolderBrowserDialog fbDlg = new WF.FolderBrowserDialog();
			if (fbDlg.ShowDialog() == WF.DialogResult.OK)
				ScanDirectory.Text = fbDlg.SelectedPath;
		}

		private void FileDoubleClick(object sender, MouseButtonEventArgs e)
		{
			object file = (e.Source as ListBox).SelectedItem;
			if (file is FileInfo)
			{
				Process.Start((file as FileInfo).FullName);
			}
		}
	}

	public static class ExtensionMethods
	{
		private static Action EmptyDelegate = delegate() { };
		public static void Refresh(this UIElement elem)
		{
			elem.Dispatcher.Invoke(EmptyDelegate, DispatcherPriority.Render);
		}

		public static void Add<T>(this ItemCollection target, IEnumerable<T> source)
		{
			foreach (T item in source)
				target.Add(item);
		}
	}

	public static class FileScanner
	{

		public static uint WantedFilesCounter;

		public static void GetFilesSelectively(
			this DirectoryInfo path,
			bool scanRecursively,
			TextBlock statusDisplayer,
			ProgressBar statusProgress,
			ListBox statusItems,
			params string[] extensions)
		{
			if (statusProgress != null)
			{
				WantedFilesCounter = 0;
				Thread counter = new Thread(new ThreadStart(delegate()
				{
					path.GetFileCountSelectively(true, extensions);
				}));
				counter.Name = "File counting thread";
				counter.Start();
				counter.Join();
			}

			statusProgress.Dispatcher.BeginInvoke(new Action(delegate()
			{
				statusProgress.Maximum = WantedFilesCounter;
			}));
			Thread.Sleep(5);

			WantedFilesCounter = 0;

			Thread scanner = new Thread(new ThreadStart(delegate()
			{
				path._GetFilesSelectively(scanRecursively, statusDisplayer, statusProgress, statusItems, extensions);
			}));
			scanner.Name = "File scanning thread";
			scanner.Start();
		}
		public static void _GetFilesSelectively(
			this DirectoryInfo path,
			bool scanRecursively,
			TextBlock statusDisplayer,
			ProgressBar statusProgress,
			ListBox statusItems,
			params string[] extensions)
		{
			FileInfo[] allFiles;
			try
			{
				allFiles = path.GetFiles();
			}
			catch (Exception ex)
			{
				if (ex is UnauthorizedAccessException || ex is IOException || ex is SecurityException)
					return;
				throw;
			}

			var files = new List<FileInfo>();
			foreach (FileInfo file in allFiles)
			{
				// If the file's type is among the wanted ones :
				if (extensions.Contains(file.Extension.Replace(".", "")))
				{
					WantedFilesCounter++;

					if (statusProgress != null)
					{
						statusProgress.Dispatcher.BeginInvoke(
							DispatcherPriority.Normal,
							new Action(
								delegate()
								{
									statusProgress.Value = WantedFilesCounter;
								}
						));
					}

					files.Add(file);

					if (statusDisplayer != null)
					{
						statusDisplayer.Dispatcher.BeginInvoke(
							DispatcherPriority.Normal,
							new Action(
								delegate()
								{
									statusDisplayer.Text = file.FullName;
								}
						));
					}
					Thread.Sleep(5);
				}
			}
			statusItems.Dispatcher.BeginInvoke(
				DispatcherPriority.Normal,
				new Action(
					delegate()
					{
						foreach (FileInfo item in files)
							statusItems.Items.Add(item);
					}
			));
			Thread.Sleep(5);

			if (scanRecursively)
				foreach (DirectoryInfo directory in path.GetDirectories())
					directory._GetFilesSelectively(true, statusDisplayer, statusProgress, statusItems, extensions);
		}


		public static List<string> GetFilesSelectivelyFromDirectory(string path, bool scanRecursively, params string[] extensions)
		{
			List<string> files = new List<string>();
			bool isAppropriate;

			string[] allFiles;
			try
			{
				allFiles = Directory.GetFiles(path);
			}
			catch (Exception ex)
			{
				if (ex is UnauthorizedAccessException || ex is IOException || ex is SecurityException)
					return new List<string>();
				throw;
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
					files.Add(file);
			}

			if (scanRecursively)
				foreach (string directory in Directory.GetDirectories(path))
					files.AddRange(GetFilesSelectivelyFromDirectory(directory, true, extensions));

			return files;
		}
		public static List<FileInfo> GetFilesSelectivelyFromDirectory(DirectoryInfo path, bool scanRecursively, params string[] extensions)
		{
			List<FileInfo> files = new List<FileInfo>();

			FileInfo[] allFiles;
			try
			{
				allFiles = path.GetFiles();
			}
			catch (Exception ex)
			{
				if (ex is UnauthorizedAccessException || ex is IOException || ex is SecurityException)
					return new List<FileInfo>();
				throw;
			}

			foreach (FileInfo file in allFiles)
				if (extensions.Contains(file.Extension.Replace(".", "")))
					files.Add(file);

			if (scanRecursively)
				foreach (DirectoryInfo directory in path.GetDirectories())
					files.AddRange(GetFilesSelectivelyFromDirectory(directory, true, extensions));

			return files;
		}

		public static void GetFileCountSelectively(this DirectoryInfo path, bool scanRecursively, params string[] extensions)
		{
			FileInfo[] allFiles;
			try
			{
				allFiles = path.GetFiles();
			}
			catch (Exception ex)
			{
				if (ex is UnauthorizedAccessException || ex is IOException || ex is SecurityException)
					return;
				throw;
			}

			foreach (FileInfo file in allFiles)
				if (extensions.Contains(file.Extension.Replace(".", "")))
					WantedFilesCounter++;

			if (scanRecursively)
				foreach (DirectoryInfo directory in path.GetDirectories())
					directory.GetFileCountSelectively(true, extensions);
		}

		public static uint GetFileCountSelectivelyFromDirectory(string path, bool scanRecursively, params string[] extensions)
		{
			uint fileCount = 0;
			bool isAppropriate;

			string[] allFiles;
			try
			{
				allFiles = Directory.GetFiles(path);
			}
			catch (Exception ex)
			{
				if (ex is UnauthorizedAccessException || ex is IOException || ex is SecurityException)
					return 0;
				throw;
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
		public static uint GetFileCountSelectivelyFromDirectory(DirectoryInfo path, bool scanRecursively, params string[] extensions)
		{
			uint fileCount = 0;

			FileInfo[] allFiles;
			try
			{
				allFiles = path.GetFiles();
			}
			catch (Exception ex)
			{
				if (ex is UnauthorizedAccessException || ex is IOException || ex is SecurityException)
					return 0;
				throw;
			}

			foreach (FileInfo file in allFiles)
				if (extensions.Contains(file.Extension.Replace(".", "")))
					fileCount++;

			if (scanRecursively)
				foreach (DirectoryInfo directory in path.GetDirectories())
					fileCount += GetFileCountSelectivelyFromDirectory(directory, true, extensions);

			return fileCount;
		}
	}
}
