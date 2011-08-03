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
		public static string[] ScannableExtensions = ("avi,mp4,mkv,wmv" + ',' + "mp3,mp2,amr" + ',' + "jpg,jpeg,png,gif,bmp,tiff").Split(',');

		public bool? ShowFullPaths
		{
			get { return (bool?)GetValue(ShowFullPathsProperty); }
			set { SetValue(ShowFullPathsProperty, value); }
		}
		// Using a DependencyProperty as the backing store for ShowFullPaths.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ShowFullPathsProperty =
			DependencyProperty.Register("ShowFullPaths", typeof(bool?), typeof(MainWindow), new UIPropertyMetadata(false));

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

        private void StartScanButton_Click(object sender, RoutedEventArgs e)
        {
			FileScanner.Abort();

			string path = ScanDirectory.Text.Trim();
			bool recursive = isRecursiveScan.IsChecked ?? false;
			bool counting = countFiles.IsChecked ?? false;
			if (path == "*")
			{
				FileScanner.GetAllFilesSelectively(recursive, ScanLabel, counting ? ScanProgressBar : null, mediaFileList, ScannableExtensions);
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
				scanPath.GetFilesSelectively(recursive, ScanLabel, counting ? ScanProgressBar : null, mediaFileList, ScannableExtensions);
			}
        }
		private void StopScanButton_Click(object sender, RoutedEventArgs e)
		{
			FileScanner.Abort();
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
	}
}