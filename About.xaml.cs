using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
	/// Interaction logic for About.xaml
	/// </summary>
	public partial class About : Page
	{
		public About()
		{
			InitializeComponent();
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{

			AssemblyName info = System.Reflection.Assembly.GetExecutingAssembly().GetName();
			Version.Content = info.Version.ToString();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			// Go home
			var newHome = new Home();
			this.NavigationService.Navigate(newHome);
		}
	}
}
