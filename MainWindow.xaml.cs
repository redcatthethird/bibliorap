using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using System.Threading;
using System.Windows.Media.Imaging;
using WF = System.Windows.Forms;
using D = System.Drawing;
using T = System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Shell;

namespace BiblioRap
{
	public enum Ext
	{
		Vid,
		Snd,
		Pic,
		Doc,
		Rnd
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		List<FileInfo> fileList = null;
		public static string mediaDir;
		public static MainWindow mnWn;
		
		public static string[] VidExt = "avi,mp4,mkv,wmv".Split(',');
		public static string[] SndExt = "mp3,mp2,amr,wav,wma,flac".Split(',');
		public static string[] PicExt = "jpg,jpeg,png,gif,bmp,tiff".Split(',');
		public static string[] DocExt = "txt,doc,docx,pdf,nfo,epub,mobi".Split(',');
		public static string[] Exten
		{
			// VidExt + SndExt + PicExt + DocExt
			get { return VidExt.Concat(SndExt).Concat(PicExt).Concat(DocExt).ToArray<string>(); }
		}

		public bool? ShowFullPaths
		{
			get { return (bool?)GetValue(ShowFullPathsProperty); }
			set { SetValue(ShowFullPathsProperty, value); }
		}
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
			ScannerSettings SS = new ScannerSettings(recursive, ScanLabel, ScanProgressBar, mediaFileList, Exten);
			
			string path = ScanDirectory.Text.Trim();
			if (path.IsNullOrEmpty())
				MessageBox.Show("Selectati un folder pentru scanere", "Incercati din nou");

			if (path == "*")
			{
				mediaFileList.Items.Clear();
				FileScanner.GetAllFilesSelectively(SS);
			}
			else
			{
				if (!Directory.Exists(path))
				{
					MessageBox.Show(this, "Folderul nu exista! Verificati daca este scris corect.",
						"Folder lipsa!", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
					return;
				}
				DirectoryInfo scanPath = new DirectoryInfo(path);

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
			fbDlg.Description = "Alegeti un folder pentru a fi scanat.";
			if (fbDlg.ShowDialog() == WF.DialogResult.OK)
				ScanDirectory.Text = fbDlg.SelectedPath;
		}
		private void FileDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton != MouseButton.Left)
				return;
			object file = (e.Source as ListBox).SelectedItem;
			Process.Start((file as TFileInfo).FullName);
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

			// Clear the displayed file list and add only the files that contain the filter.
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

		private void ScanDirectory_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
				StartScanButton_Click(sender, e);
		}

		private void Th_Click(object sender, RoutedEventArgs e)
		{
			FileScanner.Abort();
			PicGetter.GetPix(mediaFileList);
		}

		private void mainWin_Loaded(object sender, RoutedEventArgs e)
		{
			mnWn = mainWin;

			var x = new WF.FolderBrowserDialog();
			x.ShowNewFolderButton = true;
			x.RootFolder = Environment.SpecialFolder.Desktop;
			x.Description = "Alegeti un folder ca sa serveasca drept baza pentru organizarea fisierelor media.";
			WF.DialogResult d = x.ShowDialog();
			if (d != WF.DialogResult.OK && d != WF.DialogResult.Yes)
				this.Close();
			mediaDir = x.SelectedPath;

			DirectoryInfo t = new DirectoryInfo(mediaDir + @"\Video");
			if (!t.Exists)
				t.Create();
			t = new DirectoryInfo(mediaDir + @"\Audio");
			if (!t.Exists)
				t.Create();
			t = new DirectoryInfo(mediaDir + @"\Photo");
			if (!t.Exists)
				t.Create();
			t = new DirectoryInfo(mediaDir + @"\Written");
			if (!t.Exists)
				t.Create();
		}

		private void Stack_Click(object sender, RoutedEventArgs e)
		{
			FileScanner.Abort();

			TFileInfo t;
			if (mediaFileList.SelectedItems.Count < 1)
				return;

			TFileInfo[] z = new TFileInfo[mediaFileList.SelectedItems.Count];
			mediaFileList.SelectedItems.CopyTo(z, 0);

			foreach (object o in z)
				{
					t = o as TFileInfo;
					if (t.libs != LibState.Added)
					{
						if (t.libs == LibState.Unaddable)
						{
							MessageBoxResult r = MessageBox.Show("Ati selectat pentru reorganizare un fisier"
														+ " important pentru programele sau sistemul de operare al dumneavoastra."
														+ " Doriti sa continuati ?", "Warning", MessageBoxButton.YesNoCancel,
														MessageBoxImage.Warning, MessageBoxResult.Cancel);
							switch (r)
							{
								case MessageBoxResult.No:
									continue;
								case MessageBoxResult.Cancel:
									return;
								default:
									break;
							}
						}
						mediaFileList.Items.Remove(o);
						switch (t.ext)
						{
							case Ext.Vid:
								t.f.MoveTo(MainWindow.mediaDir + @"\Video\" + t.Name);
								break;
							case Ext.Snd:
								t.f.MoveTo(MainWindow.mediaDir + @"\Audio\" + t.Name);
								break;
							case Ext.Pic:
								t.f.MoveTo(MainWindow.mediaDir + @"\Photo\" + t.Name);
								break;
							case Ext.Doc:
								t.f.MoveTo(MainWindow.mediaDir + @"\Written\" + t.Name);
								break;
							default:
								break;
						}
					}					
				}
		}
	}
}