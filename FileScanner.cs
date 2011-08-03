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
					path.GetDirectoryCount(extensions);
				}));
				counter.Name = "Directory counting thread";
				counter.Start();
				counter.Join();

				statusProgress.Dispatcher.BeginInvoke(UpdateProgress, statusProgress, WantedDirectoriesCounter, UpdateProgressType.ProgressMaximum);

				WantedDirectoriesCounter = 0;
			}

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
				statusProgress.Dispatcher.BeginInvoke(UpdateProgress, statusProgress, WantedDirectoriesCounter, UpdateProgressType.ProgressValue);
			}

			foreach (FileInfo file in allFiles)
			{
				// If the file's type is among the wanted ones :
				if (extensions.Contains(file.Extension.Replace(".", "").ToLowerInvariant()))
				{
					if (statusDisplayer != null)
						statusProgress.Dispatcher.BeginInvoke(UpdateProgress, statusDisplayer, file.FullName, UpdateProgressType.TextBlockText);

					statusProgress.Dispatcher.BeginInvoke(UpdateProgress, statusItems, file, UpdateProgressType.ListBoxItems);
					Thread.Sleep(1);
				}
			}

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

		/// <summary>
		/// A handler for the updating of any object that can display progress somehow.
		/// </summary>
		/// <param name="progressDisplayer">The object that can display progress.</param>
		/// <param name="value">The value to be assigned to the object.</param>
		/// <param name="action">Determining what to do by means of a switch statement.</param>
		public delegate void UpdateProgressHandler(DispatcherObject progressDisplayer, object value, UpdateProgressType action);
		/// <summary>
		/// A method for the updating of any object that can display progress somehow.
		/// </summary>
		/// <param name="progressDisplayer">The object that can display progress.</param>
		/// <param name="value">The value to be assigned to the object.</param>
		/// <param name="action">A UpdateProgressType for deciding on a situation.</param>
		public static void UpdateProgressMethod(DispatcherObject progressDisplayer, object value, UpdateProgressType action)
		{
			try
			{
				switch (action)
				{
					case UpdateProgressType.ProgressMaximum:
						((ProgressBar)progressDisplayer).Maximum = (int)value;
						break;
					case UpdateProgressType.ProgressValue:
						((ProgressBar)progressDisplayer).Value = (int)value;
						break;
					case UpdateProgressType.TextBlockText:
						((TextBlock)progressDisplayer).Text = (string)value;
						break;
					case UpdateProgressType.ListBoxItems:
						if (value is IEnumerable<object>)
						{
							foreach (object item in (IEnumerable<object>)value)
							{
								((ListBox)progressDisplayer).Items.Add(item);
							}
						}
						else
						{
							((ListBox)progressDisplayer).Items.Add(value);
						}
						break;
					default:
						break;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					"Something went wrong here, in the UpdateProgress method. Not cool, dude, not cool at all.\n" +
					ex.Message,
					ex.InnerException.Message,
					MessageBoxButton.OK,
					MessageBoxImage.Error,
					MessageBoxResult.OK,
					MessageBoxOptions.ServiceNotification);
			}
		}
		/// <summary>
		/// A enumeration for describing the type of progress update to be done.
		/// </summary>
		public enum UpdateProgressType
		{
			/// <summary>
			/// Updates the Maximum property of a ProgressBar.
			/// </summary>
			ProgressMaximum,
			/// <summary>
			/// Updates the Value property of a ProgressBar.
			/// </summary>
			ProgressValue,
			/// <summary>
			/// Updates the Text property of a TextBlock.
			/// </summary>
			TextBlockText,
			/// <summary>
			/// Adds to the Items property of a ListBox.
			/// </summary>
			ListBoxItems
		}

		public static UpdateProgressHandler UpdateProgress = new UpdateProgressHandler(UpdateProgressMethod);
	}
}