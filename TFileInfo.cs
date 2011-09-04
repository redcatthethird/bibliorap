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

		private BitmapSource _thumbnail;
		private bool _thIsInit = false;
		public BitmapSource Thumbnail
		{
			get
			{
				if (!_thIsInit)
				{
					//_thumbnail = fileInfo.GetThumbnail().ToBitmapSource();
					ShellFile sf = ShellFile.FromFilePath(fileInfo.FullName);
					_thumbnail = sf.Thumbnail.LargeBitmapSource;
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
