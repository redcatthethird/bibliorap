using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Threading;
using System.IO;
using System.Security;

namespace BiblioRap
{
	public static class FileScanner
	{
		public static int WantedDirectoriesCounter;
		public static uint WantedFilesCounter;

		public static void GetFilesSelectively(
			this DirectoryInfo path,
			bool scanRecursively,
			TextBlock statusDisplayer,
			ProgressBar statusProgress,
			ListBox statusItems,
			params string[] extensions)
		{
			if (statusProgress != null)
			{
				WantedDirectoriesCounter = 0;
				Thread counter = new Thread(new ThreadStart(delegate()
				{
					path.GetDirectoryCount( extensions);
				}));
				counter.Name = "Directory counting thread";
				counter.Start();
				counter.Join();
			}

			statusProgress.Dispatcher.BeginInvoke(new Action(delegate()
			{
				statusProgress.Maximum = WantedDirectoriesCounter;
			}));
			Thread.Sleep(4);

			WantedDirectoriesCounter = 0;

			Thread scanner = new Thread(new ThreadStart(delegate()
			{
				path._GetFilesSelectively(scanRecursively, statusDisplayer, statusProgress, statusItems, extensions);
			}));
			scanner.Name = "File scanning thread";
			scanner.Start();
		}
		public static void _GetFilesSelectively(
			this DirectoryInfo path,
			bool scanRecursively,
			TextBlock statusDisplayer,
			ProgressBar statusProgress,
			ListBox statusItems,
			params string[] extensions)
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

			if (statusProgress != null)
			{
				WantedDirectoriesCounter++;
				statusProgress.Dispatcher.BeginInvoke(
					DispatcherPriority.Normal,
					new Action(
						delegate()
						{
							statusProgress.Value = WantedDirectoriesCounter;
						}
				));
			}

			var files = new List<FileInfo>();
			foreach (FileInfo file in allFiles)
			{
				// If the file's type is among the wanted ones :
				if (extensions.Contains(file.Extension.Replace(".", "").ToLowerInvariant()))
				{
					files.Add(file);

					if (statusDisplayer != null)
					{
						statusDisplayer.Dispatcher.BeginInvoke(
							DispatcherPriority.Normal,
							new Action(
								delegate()
								{
									statusDisplayer.Text = file.FullName;
								}
						));
					}
					Thread.Sleep(4);
				}
			}
			statusItems.Dispatcher.BeginInvoke(
				DispatcherPriority.Normal,
				new Action(
					delegate()
					{
						foreach (FileInfo item in files)
							statusItems.Items.Add(item);
					}
			));
			Thread.Sleep(5);

			if (scanRecursively)
				foreach (DirectoryInfo directory in path.GetDirectories())
					directory._GetFilesSelectively(true, statusDisplayer, statusProgress, statusItems, extensions);
		}

		public static List<FileInfo> GetFilesSelectivelyFromDirectory(DirectoryInfo path, bool scanRecursively, params string[] extensions)
		{
			List<FileInfo> files = new List<FileInfo>();

			FileInfo[] allFiles;
			try
			{
				allFiles = path.GetFiles();
			}
			catch (Exception ex)
			{
				if (ex is UnauthorizedAccessException || ex is IOException || ex is SecurityException)
					return new List<FileInfo>();
				throw;
			}

			foreach (FileInfo file in allFiles)
				if (extensions.Contains(file.Extension.Replace(".", "").ToLowerInvariant()))
					files.Add(file);

			if (scanRecursively)
				foreach (DirectoryInfo directory in path.GetDirectories())
					files.AddRange(GetFilesSelectivelyFromDirectory(directory, true, extensions));

			return files;
		}

		public static void GetDirectoryCount(this DirectoryInfo path, params string[] extensions)
		{
			DirectoryInfo[] allDirectories;
			try
			{
				allDirectories = path.GetDirectories();
			}
			catch (Exception ex)
			{
				if (ex is UnauthorizedAccessException || ex is IOException || ex is SecurityException)
					return;
				throw;
			}
			WantedDirectoriesCounter += allDirectories.Length;

			foreach (DirectoryInfo directory in allDirectories)
				directory.GetDirectoryCount(extensions);
		}
		public static int GetDirectoryCountFromDirectory(DirectoryInfo path, params string[] extensions)
		{
			int directoryCount = 0;

			DirectoryInfo[] allDirectories;
			try
			{
				allDirectories = path.GetDirectories();
			}
			catch (Exception ex)
			{
				if (ex is UnauthorizedAccessException || ex is IOException || ex is SecurityException)
					return 0;
				throw;
			}
			directoryCount += allDirectories.Length;

			foreach (DirectoryInfo directory in allDirectories)
				directoryCount += GetDirectoryCountFromDirectory(directory, extensions);

			return directoryCount;
		}

		public static void GetFileCountSelectively(this DirectoryInfo path, bool scanRecursively, params string[] extensions)
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
				if (extensions.Contains(file.Extension.Replace(".", "").ToLowerInvariant()))
					WantedFilesCounter++;

			if (scanRecursively)
				foreach (DirectoryInfo directory in path.GetDirectories())
					directory.GetFileCountSelectively(true, extensions);
		}
		public static uint GetFileCountSelectivelyFromDirectory(DirectoryInfo path, bool scanRecursively, params string[] extensions)
		{
			uint fileCount = 0;

			FileInfo[] allFiles;
			try
			{
				allFiles = path.GetFiles();
			}
			catch (Exception ex)
			{
				if (ex is UnauthorizedAccessException || ex is IOException || ex is SecurityException)
					return 0;
				throw;
			}

			foreach (FileInfo file in allFiles)
				if (extensions.Contains(file.Extension.Replace(".", "").ToLowerInvariant()))
					fileCount++;

			if (scanRecursively)
				foreach (DirectoryInfo directory in path.GetDirectories())
					fileCount += GetFileCountSelectivelyFromDirectory(directory, true, extensions);

			return fileCount;
		}
	}
}