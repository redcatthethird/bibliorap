using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace BiblioRap
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public static int PeskyMenuItemClickCount = 0;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void AboutButton_Click(object sender, RoutedEventArgs e)
		{
			Window aboutDialog = new AboutDialog();
			aboutDialog.ShowDialog();
		}

		private void HelpCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void HelpHasExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			MessageBox.Show(this,
				"Did you really think anyone can help you ? "
				+ "Hah hah hah hah... you are on your own on this, kiddo'. "
				+ "Best wishes, Red. a.k.a your worst nightmare...",
				"Useless Info",
				MessageBoxButton.OK,
				MessageBoxImage.Information);
		}

		private void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void CloseHasExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			if (MessageBox.Show(this, "Do you really want to close \"" + this.Title + "\" ?",
					"Annoying prompt", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
				this.Close();
		}

		private void peskyMenuItem_Click(object sender, RoutedEventArgs e)
		{
			PeskyMenuItemClickCount++;
			if (PeskyMenuItemClickCount >= 7)
				peskyLittleThing.Visibility = Visibility.Collapsed;
		}

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
			string scanPath = scanDirectory.Text;
			if (!Directory.Exists(scanPath))
			{
				MessageBox.Show(this, "The directory doesn't exist! Please check if it is typed correctly",
					"Directory missing", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
				return;
			}		
		
			// TO DO: Generalize to include recursive search.
			string[] files = Directory.GetFiles(scanPath);

			foreach (string file in files)
			{
				mediaFileList.Items.Add(file);
			}
        }

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}
