using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Shell;

namespace BiblioRap
{
	public static class PicGetter
	{
		public static ConcurrentQueue<TFileInfo> cq = new ConcurrentQueue<TFileInfo>();

		public static void GetPix(UIElement e)
		{
			TFileInfo f;
			while (!cq.IsEmpty)
			{
				if (cq.TryDequeue(out f))
				{
					f.Thu();
					e.Refresh();
				}
			}
		}

		public static void Thu(this TFileInfo f)
		{
			BitmapSource bs;
			if (Misc.Windows7)
			{
				ShellFile sf = ShellFile.FromFilePath(f.FullName);
				bs = sf.Thumbnail.ExtraLargeBitmapSource;
			}
			else if (f.isPic())
				bs = new Bitmap(Image.FromFile(f.FullName)).ToBitmapSource();
			else
				bs = Icon.ExtractAssociatedIcon(f.FullName).ToBitmap().ToBitmapSource();
			f.Thumb = bs;
		}
	}

	public class TFileInfo : DependencyObject
	{
		public FileInfo f;

		public TFileInfo(FileInfo fi)
		{
			f = fi;
			PicGetter.cq.Enqueue(this);
		}
		public TFileInfo(string s)
			: this(new FileInfo(s)) { }

		public String FullName
		{
			get { return f.FullName; }
		}
		public String Name
		{
			get { return f.Name; }
		}

		public bool isPic()
		{
			foreach (string str in MainWindow.PicExt)
				if (f.FullName.EndsWith(str))
					return true;
			return false;
		}

		private BitmapSource _thumbnail;
		private bool _thIsInit = false;

		public BitmapSource Thumb
		{
			get { return (BitmapSource)GetValue(ThumbProperty); }
			set { SetValue(ThumbProperty, value); }
		}
		public static readonly DependencyProperty ThumbProperty =
			DependencyProperty.Register(
			"Thumb",
			typeof(BitmapSource),
			typeof(TFileInfo),
			new UIPropertyMetadata((new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("BiblioRap.Images.BlankDoc.png"))).BitmapSource()));

		public static implicit operator FileInfo(TFileInfo fi)
		{
			return new FileInfo(fi.FullName);
		}
		public static implicit operator TFileInfo(FileInfo fi)
		{
			return new TFileInfo(fi);
		}
	}
}
