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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RecordPro
{
	/// <summary>
	/// Interaction logic for HomePane.xaml
	/// </summary>
	public partial class HomePane : Page
	{
		public HomePane()
		{
			InitializeComponent();
		}

		private void Home_Click(object sender, RoutedEventArgs e)
		{
			// Get the main window
			MainWindow window = App.mWindow as MainWindow;
			if (window == null)
				return;

			window.AvatarPopup.IsOpen = false; // Close the small pane

			// Warn anyone attempting to update the record
			if (window.MainFrame.NavigationService.Content.GetType() == typeof(UpdateRecord) &&
				NativeMethods.TaskDialog(new WindowInteropHelper(window).Handle, IntPtr.Zero, "Warning - Record Pro",
					"If you go home, your changes will not be saved.", "Are you sure you would like to continue?",
					NativeMethods.TaskDialogButtons.Yes | NativeMethods.TaskDialogButtons.No,
					 NativeMethods.TaskDialogIcon.Warning) == NativeMethods.TaskDialogResult.No)
			{
				return;
			}

			// Navigate to the home page
			var newHome = new Home();
			window.MainFrame.Navigate(newHome);
		}




		private void SignOut_Click(object sender, RoutedEventArgs e)
		{
			App.Logout();
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			if ((bool?)App.Current.Properties["Validated"] != true)
				homeButton.Visibility = Visibility.Hidden;
		}

	}
}
