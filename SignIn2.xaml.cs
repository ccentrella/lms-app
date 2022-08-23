using Record_Pro_Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RecordPro
{
	/// <summary>
	/// Interaction logic for SignIn2.xaml
	/// </summary>
	public partial class SignIn2 : Page
	{
		string folder = (string)App.Current.Properties["Folder"];
		string password = (string)App.Current.Properties["Password"];
		string data = (string)App.Current.Properties["Data"];
		public SignIn2()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			// Ensure that all required variables are not null
			if (folder == null | password == null || data == null)
			{
				MessageBox.Show("An error has occurred. The logon could not be completed.",
					"Logon Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			
			if (Password.Password == password)
			{
				App.LogOn(data, folder);
			}
			else
			{
				Password.Clear();
				status.Content = "Incorrect Password";
			}
		}


		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			Password.Focus();
		}

	}
}



