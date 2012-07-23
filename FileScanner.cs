using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

namespace BiblioRap
{
	public struct ScannerSettings
	{
		public ScannerSettings(bool s, TextBlock t, ProgressBar p, ItemsControl i, string[] st)
		{
			recur = s;
			displayer = t;
			progress = p;
			items = i;
			ext = st;
		}
		public bool recur; // scan recursively or not
		public TextBlock displayer;
		public ProgressBar progress;
		public ItemsControl items;
		public string[] ext;
	}

	public static class FileScanner
	{
		static Thread scan = null;
		static Thread inter = null;

		public static void GetAllFilesSelectively(ScannerSettings SS)
		{
			string[] driveStrings = Directory.GetLogicalDrives();
			int len = driveStrings.Length;
			DirectoryInfo[] drives = new DirectoryInfo[len];
			for (int i = 0; i < len; i++)
			{
				drives[i] = new DirectoryInfo(driveStrings[i]);
			}

			inter = new Thread(() =>
			{
				foreach (DirectoryInfo drive in drives)
				{
					scan = new Thread(new ThreadStart(delegate()
					{
						drive._GetFilesSelectively(SS);
					}));
					scan.Name = "File scanning thread";
					scan.IsBackground = true;
					scan.Start();
					scan.Join();
				}
			});
			inter.Name = "Intermediate thread-starting thread";
			inter.IsBackground = true;
			inter.Start();
		}
		public static void GetFilesSelectively(this DirectoryInfo path, ScannerSettings SS)
		{
			scan = new Thread(() =>
			{
				path.__GetFilesSelectively(SS);
			});
			scan.Name = "File scanning thread";
			scan.IsBackground = true;
			scan.Start();
		}
		public static void __GetFilesSelectively(this DirectoryInfo path, ScannerSettings SS)
		{
			SS.displayer.Dispatcher.BeginInvoke(UpdateProgress, SS.progress, true, UpdateProgressType.Progress);
			path._GetFilesSelectively(SS);
			SS.displayer.Dispatcher.BeginInvoke(UpdateProgress, SS.progress, false, UpdateProgressType.Progress);
		}
		public static void _GetFilesSelectively(this DirectoryInfo path, ScannerSettings SS)
		{
			FileInfo[] allFiles;
			try
			{
				allFiles = path.GetFiles();
			}
			catch (Exception ex)
			{
				if (ex is UnauthorizedAccessException || ex is IOException || ex is SecurityException)
					return;
				throw;
			}

			foreach (FileInfo file in allFiles)
			{
				try {
					// If the file's type is among the wanted ones :
					if (SS.ext.Contains(file.Extension.Replace(".", "").ToLowerInvariant()) && file.FullName.Length < 260 && file.DirectoryName.Length < 248)
					{
						SS.items.Dispatcher.BeginInvoke(UpdateProgress, SS.items, file.FullName, UpdateProgressType.ListBoxItems);
						Thread.Sleep(6);
					}
				} catch (Exception) { }
			}

			if (SS.recur)
				foreach (DirectoryInfo directory in path.GetDirectories())
					directory._GetFilesSelectively(SS);
		}

		/// <summary>
		/// A handler for the updating of any object that can display progress somehow.
		/// </summary>
		/// <param name="progressDisplayer">The object that can display progress.</param>
		/// <param name="value">The value to be assigned to the object.</param>
		/// <param name="action">Determining what to do by means of a switch statement.</param>
		delegate void UpdateProgressHandler(DispatcherObject progressDisplayer, object value, UpdateProgressType action);
		/// <summary>
		/// A method for the updating of any object that can display progress somehow.
		/// </summary>
		/// <param name="progressDisplayer">The object that can display progress.</param>
		/// <param name="value">The value to be assigned to the object.</param>
		/// <param name="action">A UpdateProgressType for deciding on a situation.</param>
		static void UpdateProgressMethod(DispatcherObject progressDisplayer, object value, UpdateProgressType action)
		{
			try
			{
				switch (action)
				{
					case UpdateProgressType.Progress:
						((ProgressBar)progressDisplayer).IsIndeterminate = (bool)value;
						break;
					case UpdateProgressType.TextBlockText:
						((TextBlock)progressDisplayer).Text = (string)value;
						break;
					case UpdateProgressType.ListBoxItems:
							((ListBox)progressDisplayer).Items.Add(new TFileInfo(value as string));
						break;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					"Something went wrong here, in the UpdateProgress method. Not cool, dude, not cool at all.\n" +
					ex.Message,
					"Not very good",
					MessageBoxButton.OK,
					MessageBoxImage.Error,
					MessageBoxResult.OK,
					MessageBoxOptions.ServiceNotification);
			}
		}
		static UpdateProgressHandler UpdateProgress = new UpdateProgressHandler(UpdateProgressMethod);
		/// <summary>
		/// A enumeration for describing the type of progress update to be done.
		/// </summary>
		enum UpdateProgressType
		{
			/// <summary>
			/// Updates the style of a ProgressBar.
			/// </summary>
			Progress,
			/// <summary>
			/// Updates the Text property of a TextBlock.
			/// </summary>
			TextBlockText,
			/// <summary>
			/// Adds to the Items property of a ListBox.
			/// </summary>
			ListBoxItems
		}

		public static void Abort()
		{
			if (scan != null)
				if (scan != null && scan.IsAlive && scan.ThreadState != ThreadState.AbortRequested)
					scan.Abort();
			scan = null;
			if (inter != null)
				if (inter != null && inter.IsAlive && inter.ThreadState != ThreadState.AbortRequested)
					inter.Abort();
			inter = null;
		}
	}
}