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
using System.Windows.Shapes;
using System.IO;
using System.Windows.Threading;
using System.Windows.Interop;
using Record_Pro_Functions;
using System.ComponentModel;
using System.Diagnostics;
using System.Security;

namespace RecordPro
{
	/// <summary>
	/// Interaction logic for Home.xaml
	/// </summary>
	public partial class Home : Page
	{
		delegate void mainDelegate();
		public Home()
		{
			InitializeComponent();
		}

		/// <summary>
		/// The location where the configuration file for the current user is stored
		/// </summary>
		string configLocation = System.IO.Path.Combine((string)App.Current.Properties["Current User Location"], "config.txt");

		private void SettingsButtonClick(object sender, RoutedEventArgs e)
		{
			// Navigate to the settings dialog
			var newSettings = new SettingsDialog();
			this.NavigationService.Navigate(newSettings);
		}

		private void UpdateRecordButton_Click(object sender, RoutedEventArgs e)
		{
			// Navigate to the update record dialog.
			var updateRecord = new UpdateRecord();
			this.NavigationService.Navigate(updateRecord);
		}

		private void CalendarButton_Click(object sender, RoutedEventArgs e)
		{
			// Load the calendar
			Calendar newCalendar = new Calendar();
			this.NavigationService.Navigate(newCalendar);
		}

		private void ViewRecordButton_Click(object sender, RoutedEventArgs e)
		{
			// Load the view record page
			var newRecord = new ViewRecord();
			this.NavigationService.Navigate(newRecord);
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			// Load all applications
			await Dispatcher.BeginInvoke(DispatcherPriority.Background, new mainDelegate(LoadApps));
		}

		/// <summary>
		/// Loads all apps in the background
		/// </summary>
		private void LoadApps()
		{
			string data = ""; // The data contained in the user file

			// Attempt to open the user file
			try
			{
				using (var newReader = new StreamReader(configLocation))
				{
					data = newReader.ReadToEnd();
				}
			}
			catch (IOException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid User Path",
					"An error has occurred. The user path is invalid. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (ArgumentException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
					"An error has occurred. Invalid characters have been found in the user path. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (UnauthorizedAccessException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. Access to the user path has been denied. If the problem persists, please contact the Administrator.",
									NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (NotSupportedException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Formatting Error",
									"An error has occurred. The user path is not formatted correctly. Please contact the Administrator.",
									NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}

			// Show the appropriate apps
			if (BasicFunctions.GetValue(data, "ShowCalendar") == "True")
			{
				Image calendarImage = new Image();
				calendarImage.Source = new BitmapImage(new Uri("View Calendar.png", UriKind.Relative));
				Button CalendarButton = new Button() { Content = calendarImage };
				CalendarButton.SetResourceReference(Button.StyleProperty, "ViewCalendarStyle");
				CalendarButton.Click += CalendarButton_Click;
				Apps.Children.Add(CalendarButton);
			}
			if (BasicFunctions.GetValue(data, "ShowCalculator") == "True")
			{
				Image calculatorImage = new Image();
				calculatorImage.Source = new BitmapImage(new Uri("Calculator.png", UriKind.Relative));
				Button CalculatorButton = new Button() { Content = calculatorImage };
				CalculatorButton.SetResourceReference(Button.StyleProperty, "CalculatorStyle");
				CalculatorButton.Click += CalculatorButton_Click;
				Apps.Children.Add(CalculatorButton);
			}
			if (BasicFunctions.GetValue(data, "ViewRecordEnabled") == "True")
			{
				Image viewRecordImage = new Image();
				viewRecordImage.Source = new BitmapImage(new Uri("View Record.png", UriKind.Relative));
				Button viewRecordButton = new Button() { Content = viewRecordImage };
				viewRecordButton.SetResourceReference(Button.StyleProperty, "ViewReportStyle");
				viewRecordButton.Click += ViewRecordButton_Click;
				Apps.Children.Add(viewRecordButton);
			}
			if (BasicFunctions.GetValue(data, "IsAdministrator") == "True")
			{
				Image manageStudentsImage = new Image();
				manageStudentsImage.Source = new BitmapImage(new Uri("Manage Users.png", UriKind.Relative));
				Button ManageStudents = new Button() { Content = manageStudentsImage };
				ManageStudents.SetResourceReference(Button.StyleProperty, "ManageStudentsStyle");
				ManageStudents.Click += ManageStudents_Click;
				Apps.Children.Add(ManageStudents);
			}
			if (BasicFunctions.GetValue(data, "ShowUpdateRecordPopup") != "False")
				UpdateRecordPopup.IsOpen = true;
		}

		private void ManageCourses_Click(object sender, RoutedEventArgs e)
		{
			// Show the manage courses window
			var newPage = new ManageCourses();
			this.NavigationService.Navigate(newPage);
		}

		private void ManageStudents_Click(object sender, RoutedEventArgs e)
		{
			// Show the manage students window
			var newPage = new ManageStudents();
			this.NavigationService.Navigate(newPage);
		}

		private void AboutButton_Click(object sender, RoutedEventArgs e)
		{
			// Show the about dialog
			var newPage = new About();
			this.NavigationService.Navigate(newPage);
		}

		private void CalculatorButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				using (var proc = new Process())
				{
					proc.StartInfo = new ProcessStartInfo("calc.exe");
					proc.Start();
				}
			}
			catch (Win32Exception)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Critical Error",
					"Calculator could not be opened.", "This indicates a critical error in your system. "
				+ "It is highly recommended to immediately reinstall your OS.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
		}

		private void Apps_Drop(object sender, DragEventArgs e)
		{
			//e.Effects = DragDropEffects.Link;
			//Apps.Children.Add(new Button() { Content = e.Data });

			//// When an app is dropped, create a link under the user's folder
		}

		private void UpdateRecordPopupOKButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				UpdateRecordPopup.IsOpen = false;
				var data = File.ReadAllText(configLocation);
				File.WriteAllText(configLocation, BasicFunctions.ReplaceValue(data, "ShowUpdateRecordPopup", "False"));
			}
			catch (IOException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid User Path",
					"An error has occurred. The user path is invalid. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (ArgumentException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
					"An error has occurred. Invalid characters have been found in the user path. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (UnauthorizedAccessException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. Access to the user path has been denied. If the problem persists, please contact the Administrator.",
									NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (NotSupportedException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Formatting Error",
									"An error has occurred. The user path is not formatted correctly. Please contact the Administrator.",
									NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (SecurityException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. Access to the user path has been denied. If the problem persists, please contact the Administrator.",
									NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
		}

		private void ClockButton_Click(object sender, RoutedEventArgs e)
		{
			// Open a new instance of Autosoft Clock
			string path = "Autosoft Clock.exe";
			try
			{
				Process.Start(path);
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show("Clock could not be opened.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (Win32Exception)
			{
				MessageBox.Show("Clock could not be opened.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
