﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Shell;

namespace BiblioRap
{
	public class TFileInfo
	{
		public FileInfo f;

		public TFileInfo(FileInfo fi)
		{
			f = fi;
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
		public BitmapSource Thumbnail
		{
			get
			{
				if (!_thIsInit)
				{
					double screenW = System.Windows.SystemParameters.PrimaryScreenWidth;
					double screenH = System.Windows.SystemParameters.PrimaryScreenHeight;

					if (Misc.Windows7)
					{
						//_thumbnail = fileInfo.GetThumbnail().ToBitmapSource();
						ShellFile sf = ShellFile.FromFilePath(f.FullName);
						Bitmap b = sf.Thumbnail.SmallBitmap;
						bool reswidth = b.Width/screenW > b.Height/screenH; // should I resize the width ?
						//(int)System.Windows.SystemParameters.PrimaryScreenWidth / 10;
						_thumbnail = sf.Thumbnail.ExtraLargeBitmap.ToBitmapSource();
					}
					else
					{
						if (isPic)
						{
							_thumbnail = new Bitmap(Image.FromFile(f.FullName)).ToBitmapSource();
						}
						else
							_thumbnail = Icon.ExtractAssociatedIcon(f.FullName).ToBitmap().ToBitmapSource();
					}
				}
				return _thumbnail;
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
