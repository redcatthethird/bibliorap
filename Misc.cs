using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using RMA.Shell;
using System.Windows.Controls;

namespace BiblioRap
{
	public static class Misc
	{
		public static bool Windows7;

		static Misc()
		{
			Version vs = Environment.OSVersion.Version;

			if (vs.Major == 6 && vs.Minor != 0)
				Windows7 = true;
			else
				Windows7 = false;
		}

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

		public static BitmapSource BitmapSource(this Bitmap bmp)
		{
			return bmp.ToBitmapSource();
		}

		/*public static Bitmap GetResized(this Bitmap bmp, int width, int height)
		{
			MemoryStream ms = new MemoryStream();
			bmp.Save(ms, ImageFormat.Png);
			BitmapImage bi = new BitmapImage();
			bi.BeginInit();
			if (width > 0)
				bi.DecodePixelWidth = width;
			if (height > 0)
				bi.DecodePixelHeight = height;
			bi.StreamSource = ms;
			bi.EndInit();


			MemoryStream mse = new MemoryStream();
			//BmpBitmapEncoder e = new BmpBitmapDecoder();
		}*/
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
