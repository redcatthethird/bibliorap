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
using IO = System.IO;
using Microsoft.Win32;
using WF = System.Windows.Forms;

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
				string[] drives = IO.Directory.GetLogicalDrives();
				foreach (string drive in drives)
				{
					(new IO.DirectoryInfo(drive)).GetFilesSelectively(recursive, ScanLabel, ScanProgressBar, mediaFileList, ScannableExtensions);
				}
			}
			else
			{
				IO.DirectoryInfo scanPath = new IO.DirectoryInfo(path);
				if (!scanPath.Exists)
				{
					MessageBox.Show(this, "The directory doesn't exist ! Please check if it is typed correctly,",
						"IO.Directory missing !", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
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
				using (IO.TextWriter writer = new IO.StreamWriter(sfDlg.FileName))
				{
					foreach (string file in mediaFileList.Items)
					{
						writer.WriteLine(file);
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
	}

	public static class ExtensionMethods
	{
		public static uint WantedFilesCounter;
		public static uint WantedFilesScanCounter;

		public static void GetFilesSelectively(
			this IO.DirectoryInfo path,
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
					path.GetFileCountSelectively(true, statusProgress, extensions);
				}));
				counter.Name = "File counting thread";
				counter.Start();
				counter.Join();
			}

			statusProgress.Dispatcher.BeginInvoke(new Action(delegate()
			{
				statusProgress.Maximum = WantedFilesCounter;
			}));
			Thread.Sleep(0);

			WantedFilesScanCounter = 0;
			
			Thread scanner = new Thread(new ThreadStart(delegate()
			{
				path._GetFilesSelectively(scanRecursively, statusDisplayer, statusProgress, statusItems, extensions);
			}));
			scanner.Name = "File scanning thread";
			scanner.Start();
		}
		public static void _GetFilesSelectively(
			this IO.DirectoryInfo path,
			bool scanRecursively,
			TextBlock statusDisplayer,
			ProgressBar statusProgress,
			ListBox statusItems,
			params string[] extensions)
		{
			IO.FileInfo[] allFiles;
			try
			{
				allFiles = path.GetFiles();
			}
			catch (UnauthorizedAccessException)
			{
				return;
			}
			catch (IO.IOException)
			{
				return;
			}
			catch (System.Security.SecurityException)
			{
				return;
			}

			foreach (IO.FileInfo file in allFiles)
			{
				// If the file's type is among the wanted ones :
				if (extensions.Contains(file.Extension.Replace(".", "")))
				{
					WantedFilesScanCounter++;

					if (statusProgress != null)
					{
						statusProgress.Dispatcher.BeginInvoke(
							DispatcherPriority.Normal,
							new Action(
								delegate()
								{
									statusProgress.Value = WantedFilesScanCounter;
								}
						));
					}

					statusItems.Dispatcher.BeginInvoke(
						DispatcherPriority.Normal,
						new Action(
							delegate()
							{
								statusItems.Items.Add(file.Name);
							}
					));

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

					Thread.Sleep(7);
				}
			}

			if (scanRecursively)
				foreach (IO.DirectoryInfo directory in path.GetDirectories())
					directory._GetFilesSelectively(true, statusDisplayer, statusProgress, statusItems, extensions);
		}


		public static List<string> GetFilesSelectivelyFromDirectory(string path, bool scanRecursively, TextBlock statusDisplayer, params string[] extensions)
		{
			List<string> files = new List<string>();
			bool isAppropriate;

			string[] allFiles;
			try
			{
				allFiles = IO.Directory.GetFiles(path);
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
				foreach (string directory in IO.Directory.GetDirectories(path))
					files.AddRange(GetFilesSelectivelyFromDirectory(directory, true, statusDisplayer, extensions));

			return files;
		}
		public static List<IO.FileInfo> GetFilesSelectivelyFromDirectory(IO.DirectoryInfo path, bool scanRecursively, TextBlock statusDisplayer, params string[] extensions)
		{
			List<IO.FileInfo> files = new List<IO.FileInfo>();

			IO.FileInfo[] allFiles;
			try
			{
				allFiles = path.GetFiles();
			}
			catch (UnauthorizedAccessException)
			{
				return new List<IO.FileInfo>();
			}

			foreach (IO.FileInfo file in allFiles)
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
				foreach (IO.DirectoryInfo directory in path.GetDirectories())
					files.AddRange(GetFilesSelectivelyFromDirectory(directory, true, statusDisplayer, extensions));

			return files;
		}

		public static void GetFileCountSelectively(this IO.DirectoryInfo path, bool scanRecursively, ProgressBar countDisplayer, params string[] extensions)
		{
			IO.FileInfo[] allFiles;
			try
			{
				allFiles = path.GetFiles();
			}
			catch (UnauthorizedAccessException)
			{
				return;
			}
			catch (IO.IOException)
			{
				return;
			}
			catch (System.Security.SecurityException)
			{
				return;
			}


			foreach (IO.FileInfo file in allFiles)
				if (extensions.Contains(file.Extension.Replace(".", "")))
					WantedFilesCounter++;

			if (scanRecursively)
				foreach (IO.DirectoryInfo directory in path.GetDirectories())
					directory.GetFileCountSelectively(true, countDisplayer, extensions);
		}

		public static uint GetFileCountSelectivelyFromDirectory(string path, bool scanRecursively, params string[] extensions)
		{
			uint fileCount = 0;
			bool isAppropriate;

			string[] allFiles;
			try
			{
				allFiles = IO.Directory.GetFiles(path);
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
				foreach (string directory in IO.Directory.GetDirectories(path))
					fileCount += GetFileCountSelectivelyFromDirectory(directory, true, extensions);

			return fileCount;
		}
		public static uint GetFileCountSelectivelyFromDirectory(IO.DirectoryInfo path, bool scanRecursively, params string[] extensions)
		{
			uint fileCount = 0;

			IO.FileInfo[] allFiles;
			try
			{
				allFiles = path.GetFiles();
			}
			catch (UnauthorizedAccessException)
			{
				return 0;
			}

			foreach (IO.FileInfo file in allFiles)
				if (extensions.Contains(file.Extension.Replace(".", "")))
					fileCount++;

			if (scanRecursively)
				foreach (IO.DirectoryInfo directory in path.GetDirectories())
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
