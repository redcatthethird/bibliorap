using System;
using System.IO;
using System.Collections;
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
using D = System.Drawing;

namespace BiblioRap
{
	/// <summary>
	/// Interaction logic for Recreation.xaml
	/// </summary>
	public partial class Recreation : Window
	{
		private ListBox refList = null;
		private TFileInfo[] flz;
		int n;

		public Recreation(ListBox lb)
		{
			refList = lb;
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			n = refList.Items.Count;
			flz = new TFileInfo[n];
			refList.Items.CopyTo(flz, 0);

			foreach (OriginMode mode in Enum.GetValues(typeof(OriginMode)))
			{
				OriginModer.Items.Add(mode.ToString());
			}
			OriginModer.SelectedIndex = 0;
		}

		private void BringUpDuplicates()
		{
			int j;
			ConflictList.Items.Clear();
			OriginMode mode = (OriginMode)Enum.Parse(typeof(OriginMode), OriginModer.SelectedItem.ToString());
			switch (mode) 
			{
				case OriginMode.Name:
					IOrderedEnumerable<TFileInfo> sl = flz.OrderBy(t => t.Name);
					flz = sl.ToArray();
					for (int i = 0; i < n; i++)
					{
						if (i+1 < n && flz[i].Name == flz[i+1].Name)
						{
							ConflictList.Items.Add(flz[i]);
							ConflictList.Items.Add(flz[i+1]);
							for (j = 2; i + j < n && flz[i].Name == flz[i+j].Name; j++)
								ConflictList.Items.Add(flz[i+j]);
							i += j - 1;
						}
					}

					break;
				case OriginMode.Bytecheck:
					break;
				case OriginMode.MD5:
					break;
				default:
					break;
			}
		}

		private void Fixer_Click(object sender, RoutedEventArgs e)
		{
			BringUpDuplicates();
		}

		private void ConflictList_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			ContextMenu ctxM = new ContextMenu();
			FileInfo[] fis = new FileInfo[ConflictList.SelectedItems.Count];
			int i = 0;
			foreach (TFileInfo fi in ConflictList.SelectedItems)
			{
				fis[i] = fi;
				i++;
			}
			Point p0 = Mouse.GetPosition(Application.Current.MainWindow);
			D.Point p = new D.Point((int)p0.X, (int)p0.Y);
			ctxM.ShowContextMenu(fis, p);
		}
	}

	public enum OriginMode
	{
		Name,
		Bytecheck,
		MD5
	}
}
