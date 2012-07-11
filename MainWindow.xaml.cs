using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using WF = System.Windows.Forms;
using D = System.Drawing;

namespace BiblioRap
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		List<FileInfo> fileList = null;
		public static string[] VidExt = "avi,mp4,mkv,wmv".Split(',');
		public static string[] SndExt = "mp3,mp2,amr".Split(',');
		public static string[] PicExt = "jpg,jpeg,png,gif,bmp,tiff".Split(',');
		public static string[] DocExt = "txt,doc,docx,pdf,nfo,epub,mobi".Split(',');
		public static string[] Ext
		{
			// VidExt + SndExt + PicExt + DocExt
			get { return VidExt.Concat(SndExt).Concat(PicExt).Concat(DocExt).ToArray<string>(); }
		}

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
				"Never gonna give you up" +
				"\nNever gonna let you down" +
				"\nNever gonna run around and desert you" +
				"\nNever gonna make you cry" +
				"\nNever gonna say goodbye" +
				"\nNever gonna tell a lie and hurt you",
				"You want help, huh ?",
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
			fileList = null;
			filterBox.Text = "";
			bool recursive = isRecursiveScan.IsChecked ?? false;
			ScannerSettings SS = new ScannerSettings(recursive, ScanLabel, ScanProgressBar, mediaFileList, Ext);
			
			string path = ScanDirectory.Text.Trim();
			if (path.IsNullOrEmpty())
				MessageBox.Show("Please type in a directory for scanning",
					"Please try again");
				/*MessageBox.Show("Look, man, uh... Well, honestly, this is embarrassing. Not for me as much as for you. \n"
					+ "Cuz, you know, um, you've just given me an empty string. Or path. Or directory. So, a mistake on your part, I see. \n"
					+ "But it's ok, you know, because I'll just forgive you and let you try again. Ok ? Now try again.",
					"Please try again");*/

			if (path == "*")
			{
				FileScanner.GetAllFilesSelectively(SS);
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
				scanPath.GetFilesSelectively(SS);
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
			if (e.ChangedButton != MouseButton.Left)
				return;
			object file = (e.Source as ListBox).SelectedItem;
			if (file is FileInfo)
			{
				Process.Start((file as FileInfo).FullName);
			}
			if (file is TFileInfo)
			{
				Process.Start((file as TFileInfo).FullName);
			}
		}
		private void FileRightClick(object sender, MouseButtonEventArgs e)
		{
			ContextMenu ctxM = new ContextMenu();
			FileInfo[] fis = new FileInfo[mediaFileList.SelectedItems.Count];
			int i = 0;
			foreach (object fi in mediaFileList.SelectedItems)
			{
				fis[i] = fi as TFileInfo;
				i++;
			}
			Point p0 = Mouse.GetPosition(Application.Current.MainWindow);
			D.Point p = new D.Point((int)p0.X, (int)p0.Y);
			ctxM.ShowContextMenu(fis, p);
		}

		private void filterBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			FileScanner.Abort();
			bool caseSensitive = searchCaseSensitive.IsChecked ?? false;

			// If items not saved yet, do so.
			if (fileList == null)
			{
				int count = mediaFileList.Items.Count;
				fileList = new List<FileInfo>(count);
				for (int i = 0; i < count; i++)
				{
					fileList.Add(mediaFileList.Items[i] as TFileInfo);
				}
			}

			// Clear the displayed filelist and add only the files that contain the filter.
			mediaFileList.Items.Clear();
			string filter = caseSensitive ? filterBox.Text.Trim() : filterBox.Text.ToLower();
			string name;
			foreach (TFileInfo file in fileList)
			{
				if (ShowFullPaths ?? false)
					name = caseSensitive ? file.FullName : file.FullName.ToLower();
				else
					name = caseSensitive ? file.Name : file.Name.ToLower();

				if (filter.IsNullOrEmpty() || name.Contains(filter))
					mediaFileList.Items.Add(file);
			}
		}

		private void searchCaseSensitive_Checked(object sender, RoutedEventArgs e)
		{
			if (filterBox.Text != "")
			{
				filterBox.Text = "";
				foreach (TFileInfo file in fileList)
					mediaFileList.Items.Add(file);
			}
		}
		private void searchCaseSensitive_Unchecked(object sender, RoutedEventArgs e)
		{
			if (filterBox.Text != "")
			{
				filterBox.Text = "";
				foreach (TFileInfo file in fileList)
					mediaFileList.Items.Add(file);
			}
		}

		private void mediaFileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (mediaFileList.Items.Count > 0 && mediaFileList.SelectedItem != null)
			{
				TFileInfo t = mediaFileList.SelectedItem as TFileInfo;
				ScanLabel.Text = (ShowFullPaths ?? false) ? t.FullName : t.Name;
			};
		}

		private void Thumbnailer_Click(object sender, RoutedEventArgs e)
		{
			if (mediaFileList.Items.Count > 0 && mediaFileList.SelectedItem != null)
				(new ThumbnailViewer(mediaFileList)).ShowDialog();
		}
		private void Originater_Click(object sender, RoutedEventArgs e)
		{
			FileScanner.Abort();
			(new Recreation(mediaFileList)).ShowDialog();
		}

	}
}