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
					double screenW = System.Windows.SystemParameters.PrimaryScreenWidth;
					double screenH = System.Windows.SystemParameters.PrimaryScreenHeight;

					if (Misc.Windows7)
					{
						//_thumbnail = fileInfo.GetThumbnail().ToBitmapSource();
						ShellFile sf = ShellFile.FromFilePath(fileInfo.FullName);
						Bitmap b = sf.Thumbnail.SmallBitmap;
						bool reswidth = b.Width/screenW > b.Height/screenH; // should I resize the width ?
						(int)System.Windows.SystemParameters.PrimaryScreenWidth / 10
						_thumbnail = sf.Thumbnail.ExtraLargeBitmap.GetHbitmap();
					}
					else
					{
						if (isPic)
						{
							_thumbnail = new Bitmap(Image.FromFile(fileInfo.FullName)).ToBitmapSource();
						}
						else
							_thumbnail = Icon.ExtractAssociatedIcon(fileInfo.FullName).ToBitmap().ToBitmapSource();
					}
				}
				return _thumbnail;
			}
		}
	}
}
