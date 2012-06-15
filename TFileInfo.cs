using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Shell;

namespace BiblioRap
{
	public class TFileInfo : DependencyObject
	{
		public FileInfo f;
		static int x = 0;

		public TFileInfo(FileInfo fi)
		{
			f = fi;

			//RequestTrueThumb();
		}
		public TFileInfo(string s)
			: this(new FileInfo(s)) { }

		private void RequestTrueThumb()
		{			
			Thread thump = new Thread(new ThreadStart(delegate()
			{
				if (Misc.Windows7)
				{
					ShellFile sf = ShellFile.FromFilePath(f.FullName);
					Thumbnail = sf.Thumbnail.ExtraLargeBitmapSource;
				}
				else
					if (isPic)
					{
						Thumbnail = new Bitmap(Image.FromFile(f.FullName)).ToBitmapSource();
					}
					else
						Thumbnail = Icon.ExtractAssociatedIcon(f.FullName).ToBitmap().ToBitmapSource();
			}));
			thump.Name = "Thumb obtainer #" + (x++).ToString();
			thump.IsBackground = true;
			thump.Start();
		}

		public String FullName
		{
			get { return f.FullName; }
		}
		public String Name
		{
			get { return f.Name; }
		}

		private bool isPic
		{
			get
			{
				foreach (string str in MainWindow.PicExt)
					if (f.FullName.EndsWith(str))
						return true;
				return false;
			}
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

		public BitmapSource Thumbnail
		{
			get // not used stub
			{
				if (!_thIsInit)
				{
					_thumbnail = Icon.ExtractAssociatedIcon(f.FullName).ToBitmap().ToBitmapSource();


					/*
					if (Misc.Windows7)
					{
						ShellFile sf = ShellFile.FromFilePath(f.FullName);
						_thumbnail = sf.Thumbnail.ExtraLargeBitmapSource;
					}
					else
					{
						if (isPic)
						{
							_thumbnail = new Bitmap(Image.FromFile(f.FullName)).ToBitmapSource();
						}
						else
							_thumbnail = Icon.ExtractAssociatedIcon(f.FullName).ToBitmap().ToBitmapSource();
					}*/
				}
				return _thumbnail;
			}
			set // used thingy
			{
				MessageBox.Show(this.Dispatcher.Thread.ManagedThreadId.ToString());
				this.Dispatcher.BeginInvoke(new Action<BitmapSource, TFileInfo>((b, f) => f.Thumb=b), value, this);
			}
		}

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
