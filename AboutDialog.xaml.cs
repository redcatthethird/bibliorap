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
using System.Windows.Shapes;

namespace BiblioRap
{
	/// <summary>
	/// Interaction logic for AboutDialog.xaml
	/// </summary>
	public partial class AboutDialog : Window
	{
		public AboutDialog()
		{
			InitializeComponent();
		}

		private void AboutDialog_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.Title = "Source = " + e.Source.GetType().Name + ", Original Source = " + e.OriginalSource.GetType().Name + " @ " + e.Timestamp;

			Control source = e.Source as Control;

			if (source.BorderThickness != new Thickness(5))
			{
				source.BorderThickness = new Thickness(5);
				source.BorderBrush = Brushes.Black;
			}
			else
				source.BorderThickness = new Thickness(0);
		}
	}
}
