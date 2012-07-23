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
using E = System.Windows.Media.Effects;
using C = System.Windows.Controls;

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
				bs = sf.Thumbnail.LargeBitmapSource;
			}
			else if (f.ext == Ext.Pic)
			{
				int w, h;
				Image i = Image.FromFile(f.FullName);
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
				bs = new Bitmap(i.GetThumbnailImage(w, h, null, IntPtr.Zero)).ToBitmapSource();
			}
			else
				bs = Icon.ExtractAssociatedIcon(f.FullName).ToBitmap().ToBitmapSource();
			f.Thumb = bs;
		}
	}

	public enum LibState
	{
		Addable,
		Unaddable,
		Added
	}

	public class TFileInfo : DependencyObject
	{
		public FileInfo f;

		public static string winDrive = "";
		static TFileInfo()
			{
				try
				{
					winDrive = Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, 3);
				}
				catch (Exception)
				{
					foreach (string drive in Directory.GetLogicalDrives())
					{
						if (Directory.Exists(drive + "WINDOWS\\System32") || Directory.Exists(drive + "WINDOWS NT\\System32"))
						{
							winDrive = drive;
							break;
						}
					}
				}
				finally
				{
				}
				if (winDrive == "")
				{
					// couldn't find default directory, should probably do some error checking
					winDrive = "C:\\";
				}
			}
		public TFileInfo(FileInfo fi)
			{
				f = fi;
				PicGetter.cq.Enqueue(this);


				switch (libs)
				{
					case LibState.Unaddable:
						LibStatus = (new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("BiblioRap.Images.Danger.png"))).BitmapSource();
						break;
					case LibState.Added:
						LibStatus = (new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("BiblioRap.Images.Ok.png"))).BitmapSource();
						break;
					default:
						break;
				}
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

		public Ext ext
			{
				get
				{
					string s = f.FullName;
					foreach (string str in MainWindow.VidExt)
						if (s.EndsWith(str))
							return Ext.Vid;
					foreach (string str in MainWindow.SndExt)
						if (s.EndsWith(str))
							return Ext.Snd;
					foreach (string str in MainWindow.PicExt)
						if (s.EndsWith(str))
							return Ext.Pic;
					foreach (string str in MainWindow.DocExt)
						if (s.EndsWith(str))
							return Ext.Doc;
					return Ext.Rnd;
				}
			}
		public LibState libs
			{
				get
				{
					// check if the file is in it's correct media folder
					// if so, it means it was already added
					switch (ext)
					{
						case Ext.Vid:
							if (FullName.Contains(MainWindow.mediaDir + @"\Video"))
								return LibState.Added;
							break;
						case Ext.Snd:
							if (FullName.Contains(MainWindow.mediaDir + @"\Audio"))
								return LibState.Added;
							break;
						case Ext.Pic:
							if (FullName.Contains(MainWindow.mediaDir + @"\Photo"))
								return LibState.Added;
							break;
						case Ext.Doc:
							if (FullName.Contains(MainWindow.mediaDir + @"\Written"))
								return LibState.Added;
							break;
						default:
							break;
					}

					// if file pertains to the windows drive and isn't in the users folder
					if (FullName.Contains(winDrive) && !FullName.Contains(@"\Users\"))
					{
						// it can't be added
						return LibState.Unaddable;
					}

					// otherwise, if it belongs to a program
					if (FullName.Contains(@"\Program Files\"))
						// it can't be added
						return LibState.Unaddable;

					return LibState.Addable;
				}
			}

		public BitmapSource LibStatus
			{
				get { return (BitmapSource)GetValue(LibStatusProperty); }
				set { SetValue(LibStatusProperty, value); }
			}
		public static readonly DependencyProperty LibStatusProperty =
			DependencyProperty.Register("LibStatus", typeof(BitmapSource), typeof(TFileInfo),
			new UIPropertyMetadata((new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("BiblioRap.Images.Play.png"))).BitmapSource()));

		public BitmapSource Thumb
			{
				get { return (BitmapSource)GetValue(ThumbProperty); }
				set { SetValue(ThumbProperty, value); }
			}
		public static readonly DependencyProperty ThumbProperty =
			DependencyProperty.Register("Thumb", typeof(BitmapSource), typeof(TFileInfo),
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