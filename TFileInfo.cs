using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

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


		private Bitmap _thumbnail;
		private bool _thIsInit = false;
		public Bitmap Thumbnail
		{
			get
			{
				if (!_thIsInit)
				{
					_thumbnail = fileInfo.GetThumbnail();
				}
				return _thumbnail;
			}
		}
	}
}
