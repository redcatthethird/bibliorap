using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using RMA.Shell;
using WF = System.Windows.Forms;

namespace BiblioRap
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		List<FileInfo> fileList = null;
		private static string[] VidExt = "avi,mp4,mkv,wmv".Split(',');
		private static string[] SndExt = "mp3,mp2,amr".Split(',');
		private static string[] PicExt = "jpg,jpeg,png,gif,bmp,tiff".Split(',');
		public static string[] Ext
		{
			// VidExt + SndExt + PicExt
			get { return VidExt.Concat(SndExt).Concat(PicExt).ToArray<string>(); }
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
			fileList = null;
			filterBox.Text = "";
			
			string path = ScanDirectory.Text.Trim();
			if (path.IsNullOrEmpty())
				MessageBox.Show("Look, man, uh... Well, honestly, this is embarrassing. Not for me as much as for you. \n"
					+ "Cuz, you know, um, you've just given me an empty string. Or path. Or directory. So, a mistake on your part, I see. \n"
					+ "But it's ok, you know, because I'll just forgive you and let you try again. Ok ? Now try again.",
					"Please try again");

			bool recursive = isRecursiveScan.IsChecked ?? false;
			bool counting = countFiles.IsChecked ?? false;
			if (path == "*")
			{
				FileScanner.GetAllFilesSelectively(recursive, ScanLabel, counting ? ScanProgressBar : null, mediaFileList, allFiles, countedFiles, Ext);
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
				scanPath.GetFilesSelectively(recursive, ScanLabel, counting ? ScanProgressBar : null, mediaFileList, allFiles, countedFiles, Ext);
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
			if (file is TFileInfo)
			{
				Process.Start((file as TFileInfo).FullName);
			}
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

		private void Thumbnailer_Click(object sender, RoutedEventArgs e)
		{
			(new ThumbnailViewer(mediaFileList)).ShowDialog();
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
	}

	public static class ExtensionMethods
	{
		private static Action EmptyDelegate = delegate() { };
		public static void Refresh(this UIElement elem)
		{
			elem.Dispatcher.Invoke(EmptyDelegate, DispatcherPriority.Render);
		}

		public static bool IsNullOrEmpty(this string str)
		{
			return str == null || str.Length == 0;
		}

		/// <summary>
		/// Returns the Windows-displayed thumbnail of a file described by a FileInfo.
		/// Using the shell. Windows dll. Supposedly working, but not really.
		/// </summary>
		/// <param name="fi">FileInfo specifying the file.</param>
		/// <returns>Thumbnail of the file.</returns>
		public static Bitmap GetThumbnail(this FileInfo fi)
		{
			Bitmap thumb;
			using (ShellThumbnail st = new ShellThumbnail())
			{
				thumb = st.GetThumbnail(fi.FullName);
			}
			return thumb;
		}

		[DllImport("gdi32", CharSet = CharSet.Auto)]
		internal extern static int DeleteObject(IntPtr hObject);

		public static BitmapSource ToBitmapSource(this Bitmap bmp)
		{
			BitmapSource bs;
			IntPtr ip = bmp.GetHbitmap();
			try
			{
				bs = Imaging.CreateBitmapSourceFromHBitmap(
					ip,
					IntPtr.Zero,
					Int32Rect.Empty,
					BitmapSizeOptions.FromEmptyOptions());
			}
			finally
			{
				DeleteObject(ip);
			}

			return bs;
		}

		public static T Clamp<T>(this T nr, T min, T max)
			where T : IComparable<T>
		{
			if (nr.CompareTo(min) < 0)
				return min;
			if (nr.CompareTo(max) > 0)
				return max;
			return nr;
		}

		public static void Add<T>(this ItemCollection target, IEnumerable<T> source)
		{
			foreach (T item in source)
				target.Add(item);
		}
	}

	public static class VisualTraverser
	{
		public static Child GetFirstChild<Child>(DependencyObject list)
			where Child : DependencyObject
		{
			DependencyObject candidate = null;
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(list); i++)
			{
				candidate = VisualTreeHelper.GetChild(list, i);
				if (candidate is Child)
					break;
			}
			if (!(candidate is Child))
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(list); i++)
				{
					candidate = VisualTreeHelper.GetChild(list, i);
					candidate = GetFirstChild<Child>(candidate);
					if (candidate is Child)
						break;
				}
			return candidate as Child;
		}

		// Important: Here I assume the ListBoxItem contains a single TextBlock.
		public static string GetItemString(ListBoxItem item)
		{
			return GetFirstChild<TextBlock>(item).Text;
		}
	}
}