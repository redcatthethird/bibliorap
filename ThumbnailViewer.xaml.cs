using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Shell;
using D = System.Drawing;

namespace BiblioRap
{
	/// <summary>
	/// Interaction logic for TestPage.xaml
	/// </summary>
	public partial class ThumbnailViewer : Window
	{
		private ListBox refList = null;
		private int index = -1;
		private ThumbnailSize thumbSize = ThumbnailSize.ExtraLarge;

		public ThumbnailViewer(ListBox lBox)
		{
			refList = lBox;
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			index = refList.SelectedIndex;
			Refresh();
			foreach (ThumbnailSize size in Enum.GetValues(typeof(ThumbnailSize)))
			{
				DisplayModer.Items.Add(size.ToString());
			}
			DisplayModer.SelectedIndex = 0;

			
		}

		private void Displayer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			switch (e.ClickCount)
			{
				case 2:
					Process.Start((refList.Items[index] as TFileInfo).FullName);
					break;
				default:
					break;
			}
		}

		private void Lefter_Click(object sender, RoutedEventArgs e)
		{
			index--;
			Refresh();
		}

		private void Righter_Click(object sender, RoutedEventArgs e)
		{
			index++;
			Refresh();
		}

		private void Refresh()
		{
			// Overflow behavior.
			if (index > refList.Items.Count - 1) index = 0;
			if (index < 0) index = refList.Items.Count - 1;

			TFileInfo fileInfo = refList.Items[index] as TFileInfo;
			if (fileInfo == null)
				throw new ArgumentNullException("Something got screwed up, the thing selected in the ItemsControl isn't a FileInfo.");

			Displayer.Source = Thumbnail(fileInfo, thumbSize);

			this.Title = fileInfo.Name;
		}

		BitmapSource Thumbnail(TFileInfo fi, ThumbnailSize ts)
		{
			if (Misc.Windows7)
				switch (ts)
				{
					case ThumbnailSize.ExtraLarge:
						return ShellFile.FromFilePath(fi.FullName).Thumbnail.ExtraLargeBitmapSource;
					case ThumbnailSize.Large:
						return ShellFile.FromFilePath(fi.FullName).Thumbnail.LargeBitmapSource;
					case ThumbnailSize.Medium:
						return ShellFile.FromFilePath(fi.FullName).Thumbnail.MediumBitmapSource;
					case ThumbnailSize.Small:
						return ShellFile.FromFilePath(fi.FullName).Thumbnail.SmallBitmapSource;
					default:
						return ShellFile.FromFilePath(fi.FullName).Thumbnail.LargeBitmapSource;
				}
			else if (fi.ext == Ext.Pic)
			{
				int w, h;
				D.Image i = D.Image.FromFile(fi.FullName);
				if (i.Width > i.Height)
				{
					w = 256;
					h = w * i.Height / i.Width;
				}
				else
				{
					h = 256;
					w = h * i.Width / i.Height;
				}
				return new Bitmap(i.GetThumbnailImage(w, h, null, IntPtr.Zero)).ToBitmapSource();
			}
			else
				return D.Icon.ExtractAssociatedIcon(fi.FullName).ToBitmap().ToBitmapSource();
		}

		private void DisplayModer_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			thumbSize = (ThumbnailSize)Enum.Parse(typeof(ThumbnailSize), e.AddedItems[0] as string);
			Refresh();
		}

		private void Displayer_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			FileInfo[] s = new FileInfo[1] { refList.Items[index] as TFileInfo };
			System.Windows.Point p = e.GetPosition(this);
			(new ContextMenu()).ShowContextMenu(s, new D.Point((int)p.X, (int)p.Y));
		}
	}

	public enum ThumbnailSize
	{
		ExtraLarge,
		Large,
		Medium,
		Small
	}
}
