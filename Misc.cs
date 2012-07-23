using System;
using System.Collections.Generic;
using System.Windows;
using System.Text;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;
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

		public static string MD5Hash(this TFileInfo tf)
		{
			FileStream stream = tf.f.OpenRead();
			//calculate the files hash
			byte[] hash = (new MD5CryptoServiceProvider()).ComputeHash(stream);

			//string builder to hold the results
			StringBuilder sb = new StringBuilder();

			//loop through each byte in the byte array
			foreach (byte b in hash)
			{
				//format each byte into the proper value and append
				//current value to return value
				sb.Append(b.ToString("X2"));
			}

			//return the MD5 hash of the file
			return sb.ToString();
		} 

		public static BitmapSource BitmapSource(this Bitmap bmp)
		{
			return bmp.ToBitmapSource();
		}
	}
}
