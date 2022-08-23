using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RecordPro
{
	/// <summary>
	/// Interaction logic for Welcome.xaml
	/// </summary>
	public partial class Welcome : Page
	{
		public Welcome()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			// Show the new user page
			var newUser = new NewUser();
			this.NavigationService.Navigate(newUser);
		}
	}
}
