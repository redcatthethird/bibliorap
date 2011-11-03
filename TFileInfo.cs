using System;
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
		private FileInfo fileInfo;

		public TFileInfo(FileInfo fi)
		{
			fileInfo = fi;
		}

		public String FullName
		{
			get { return fileInfo.FullName; }
		}
		public String Name
		{
			get { return fileInfo.Name; }
		}

		private bool isPic
		{
			get
			{
				foreach (string str in MainWindow.PicExt)
					if (fileInfo.FullName.EndsWith(str))
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
					if (Misc.Windows7)
					{
						//_thumbnail = fileInfo.GetThumbnail().ToBitmapSource();
						ShellFile sf = ShellFile.FromFilePath(fileInfo.FullName);
						_thumbnail = sf.Thumbnail.LargeBitmapSource;
					}
					else
					{
						if (isPic)
							_thumbnail = new Bitmap(Image.FromFile(fileInfo.FullName)).ToBitmapSource();
						else
							_thumbnail = Icon.ExtractAssociatedIcon(fileInfo.FullName).ToBitmap().ToBitmapSource();
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
