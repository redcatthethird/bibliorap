/*
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;

namespace Shell
{

	/// <summary>
	/// Generates a thumbnail of a folder's picture or a file's image.
	/// </summary>
	/// <remarks>This is the "Folder's Picture" and not the "Folder's Icon"! Use SHGetFileInfo to generate the thumbnail for a folder's icon or for a file that does not have a thumbnail handler.</remarks>
	/// <reference>Microsoft Office Thumbnails in SharePoint at http://msdn.microsoft.com/en-us/library/aa289172%28VS.71%29.aspx .</reference>
	public class ShellThumbnail
	{

		//TODO - Work out the details for image size and IEIFLAG handling.

		#region " Determining Thumbnail Size and Quality [documentation] "

		//http://support.microsoft.com/kb/835823
		//Determining Thumbnail Size and Quality
		//Browse to HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer. Create or modify two DWORDs called ThumbnailSize and ThumbnailQuality. For ThumbnailSize set the value in pixels, with the default being 96. For ThumbnailQuality set the value as a number that represents the percentage quality between 50 and 100.


		//http://www.pctools.com/guides/registry/detail/1066/ (modified)
		//  User Key: [HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer]
		//System Key: [HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Explorer]
		//Value Name: ThumbnailSize, ThumbnailQuality
		// Data Type: REG_DWORD (DWORD Value)
		//Value Data: Size in pixels (32-255), Quality Percentage (50-100)


		//Microsoft® Windows® XP Registry Guide 
		//Jerry Honeycutt 
		//09/11/2002 
		//Microsoft Press
		//http://www.microsoft.com/mspress/books/sampchap/6232.aspx#118
		//<H3><I><A name=118></A>Thumbnails</I></H3>The Thumbnails category controls the 
		//quality of thumbnails in Windows Explorer. Table 5-10 describes the values for 
		//Image Quality and Size. Create values that you don't see in the registry. The 
		//default value for <CODE>ThumbnailQuality</CODE> is <CODE>0x5A</CODE>. The 
		//default value for <CODE>ThumbnailSize</CODE> is <CODE>0x60</CODE>. Keep in mind 
		//that higher quality and larger thumbnails require more disk space, which is not 
		//usually a problem, but they also take longer to display. Changing the quality 
		//does not affect thumbnails that already exist on the file system.
		//<P><B>Table 5-10 </B><I>Values in Thumbnails</I>
		//<P>
		//<TABLE border=0 cellSpacing=1 cellPadding=4 width="100%">
		//<TBODY>
		//<TR>
		//<TD bgColor=#999999 vAlign=top><B>Setting</B></TD>
		//<TD bgColor=#999999 vAlign=top><B>Name</B></TD>
		//<TD bgColor=#999999 vAlign=top><B>Type</B></TD>
		//<TD bgColor=#999999 vAlign=top><B>Data</B></TD></TR>
		//<TR>
		//<TD bgColor=#cccccc 
		//vAlign=top><CODE><B>HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer</B></CODE></TD>
		//<TD bgColor=#cccccc vAlign=top> </TD>
		//<TD bgColor=#cccccc vAlign=top> </TD>
		//<TD bgColor=#cccccc vAlign=top> </TD></TR>
		//<TR>
		//<TD bgColor=#cccccc vAlign=top><CODE>Image Quality</CODE></TD>
		//<TD bgColor=#cccccc vAlign=top><CODE>ThumbnailQuality</CODE></TD>
		//<TD bgColor=#cccccc vAlign=top><CODE>REG_DWORD</CODE></TD>
		//<TD bgColor=#cccccc vAlign=top><CODE>0x32 - 0x64</CODE></TD></TR>
		//<TR>
		//<TD bgColor=#cccccc vAlign=top><CODE>Size (pixels)</CODE></TD>
		//<TD bgColor=#cccccc vAlign=top><CODE>ThumbnailSize</CODE></TD>
		//<TD bgColor=#cccccc vAlign=top><CODE>REG_DWORD</CODE></TD>
		//<TD bgColor=#cccccc vAlign=top><CODE>0x20 - 0xFF</CODE></TD></TR></TBODY></TABLE></P>

		#endregion


		public static readonly Size DefaultThumbnailSize = new Size(96, 96);

		public const int DefaultColorDepth = 32;
		/// <summary>
		/// Used to request an image from an object, such as an item in a Shell folder.
		/// </summary>
		/// <param name="path">An absolute path to a file or folder.</param>
		public static Bitmap ExtractImage(string path)
		{
			return ExtractImage(path, Size.Empty, DefaultColorDepth, 0);
		}

		/// <summary>
		/// Used to request an image from an object, such as an item in a Shell folder.
		/// </summary>
		/// <param name="path">An absolute path to a file or folder.</param>
		/// <param name="size"></param>
		public static Bitmap ExtractImage(string path,Size size)
		{
			return ExtractImage(path, size, DefaultColorDepth, 0);
		}

		/// <summary>
		/// Used to request an image from an object, such as an item in a Shell folder.
		/// </summary>
		/// <param name="path">An absolute path to a file or folder.</param>
		/// <param name="size"></param>
		/// <param name="recommendedColorDepth">The recommended color depth in units of bits per pixel. The default is 32.</param>
		public static Bitmap ExtractImage(string path, Size size, int recommendedColorDepth)
		{
			return ExtractImage(path, size, recommendedColorDepth, 0);
		}

		/// <summary>
		/// Used to request an image from an object, such as an item in a Shell folder.
		/// </summary>
		/// <param name="path">An absolute path to a file or folder.</param>
		/// <param name="size"></param>
		/// <param name="recommendedColorDepth">The recommended color depth in units of bits per pixel. The default is 32.</param>
		private static Bitmap ExtractImage(string path, Size size, int recommendedColorDepth, IEIFLAG flags)
		{
			Bitmap oResult = null;

			IShellFolder oDesktopFolder = null;
			IntPtr hParentIDL = default(IntPtr);
			IntPtr hIDL = default(IntPtr);

			IShellFolder oParentFolder = default(IShellFolder);
			IntPtr hParentFolder = default(IntPtr);
			IExtractImage oExtractImage = default(IExtractImage);
			IntPtr hExtractImage = default(IntPtr);

			//Divide the file name into a path and file/folder name.
			string sFolderName = Path.GetDirectoryName(path);
			string sBaseName = Path.GetFileName(path);

			//Get the desktop IShellFolder.
			if (SHGetDesktopFolder(oDesktopFolder) != Missico.Win32.S_OK)
			{
				throw new System.Runtime.InteropServices.COMException();
			}

			//Get the parent folder for the specified path.
			oDesktopFolder.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, sFolderName, 0, hParentIDL, 0);
			oDesktopFolder.BindToObject(hParentIDL, IntPtr.Zero, ShellGUIDs.IID_IShellFolder, hParentFolder);
			oParentFolder = (IShellFolder)Marshal.GetTypedObjectForIUnknown(hParentFolder, typeof(IShellFolder));

			//Get the file/folder's IExtractImage
			oParentFolder.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, sBaseName, 0, hIDL, 0);
			oParentFolder.GetUIObjectOf(IntPtr.Zero, 1, new IntPtr[] { hIDL }, ShellGUIDs.IID_IExtractImage, IntPtr.Zero, hExtractImage);

			//Free the pidls. The Runtime Callable Wrappers (RCW) should automatically release the COM objects.
			Marshal.FreeCoTaskMem(hParentIDL);
			Marshal.FreeCoTaskMem(hIDL);

			Marshal.FinalReleaseComObject(oParentFolder);
			Marshal.FinalReleaseComObject(oDesktopFolder);


			if (hExtractImage == IntPtr.Zero)
			{
				//There is no handler for this file, which is odd. I believe we should default the file's type icon.
				Debug.WriteLine(string.Format("There is no thumbnail for the specified file '{0}'.", path), "ShellThumbnail.ExtractImage");


			}
			else
			{
				oExtractImage = (IExtractImage)Marshal.GetTypedObjectForIUnknown(hExtractImage, typeof(IExtractImage));

				//Set the size and flags
				Missico.Win32.SIZE oSize = default(Missico.Win32.SIZE);
				//must specify a size
				IEIFLAG iFlags = flags | IEIFLAG.IEIFLAG_ORIGSIZE | IEIFLAG.IEIFLAG_QUALITY | IEIFLAG.IEIFLAG_ASPECT;


				if (size.IsEmpty)
				{
					oSize.cx = DefaultThumbnailSize.Width;
					oSize.cy = DefaultThumbnailSize.Height;


				}
				else
				{
					oSize.cx = size.Width;
					oSize.cy = size.Height;

				}


				IntPtr hBitmap = default(IntPtr);
				System.Text.StringBuilder sPath = new System.Text.StringBuilder(Missico.Win32.MAX_PATH, Missico.Win32.MAX_PATH);


				oExtractImage.GetLocation(sPath, sPath.Capacity, 0, oSize, recommendedColorDepth, iFlags);

				//if the specified path is to a folder then IExtractImage.Extract fails.


				try
				{
					oExtractImage.Extract(hBitmap);


				}
				catch (System.Runtime.InteropServices.COMException ex)
				{
					//clear the handle since extract failed
					hBitmap = IntPtr.Zero;

					Debug.WriteLine(string.Format("There is no thumbnail for the specified folder '{0}'.", path), "ShellThumbnail.ExtractImage");


				}
				finally
				{
					Marshal.FinalReleaseComObject(oExtractImage);

				}


				if (!hBitmap.Equals(IntPtr.Zero))
				{
					//create the image from the handle
					oResult = System.Drawing.Bitmap.FromHbitmap(hBitmap);

					//dump the properties to determine what kind of bitmap is returned
					//Missico.Diagnostics.ObjectDumper.Write(oResult)

					//Tag={ }
					//PhysicalDimension={Width=96, Height=96}
					//Size={Width=96, Height=96}
					//Width=96
					//Height=96
					//HorizontalResolution=96
					//VerticalResolution=96
					//Flags=335888
					//RawFormat={ }
					//PixelFormat=Format32bppRgb
					//Palette={ }
					//FrameDimensionsList=...
					//PropertyIdList=...
					//PropertyItems=...

					Missico.Win32.DeleteObject(hBitmap);
					//release the handle

				}

			}

			return oResult;
		}

	}

}
*/