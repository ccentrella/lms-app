using Record_Pro_Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
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
using System.Windows.Shapes;
using System.Windows.Shell;

namespace RecordPro
{
	/// <summary>
	/// Interaction logic for AddStudentsDialog.xaml
	/// </summary>
	public partial class AddStudentsDialog : Window
	{
		private string selectedUser;

		public string SelectedUser
		{
			get {return selectedUser;}
		}

		public string SelectedLocation
		{
			get;
			protected set;
		}
		public AddStudentsDialog()
		{
			InitializeComponent();
		}

		private void dialog_Loaded(object sender, RoutedEventArgs e)
		{
			LoadStudents();
		}

		private void SystemCommands_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
		{
			if (e.Command == SystemCommands.CloseWindowCommand | e.Command == SystemCommands.MinimizeWindowCommand)
				e.CanExecute = true;
		}

		private void SystemCommands_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			if (e.Command == SystemCommands.CloseWindowCommand)
				SystemCommands.CloseWindow(this);
		}

		// Loads the user's grades
		private void LoadStudents()
		{
			string usersLocation = (string)App.Current.Properties["Users Location"];
			if (usersLocation == null)
				return;

			// Only continue if an administrator is logged in
			if (!(bool)App.Current.Properties["IsAdministrator"])
			{
				this.DialogResult = false;
				this.Close();
				return;
			}

			// Update the progress
			if (App.mWindow != null)
			{
				App.mWindow.Progress.Value = 0;
				App.mWindow.Progress.ToolTip = "Loading Students";
				App.mWindow.Progress.Visibility = Visibility.Visible;
				App.mWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;

			}

			try
			{
				DirectoryInfo newDirectoryInfo = new DirectoryInfo(usersLocation);
				int count = newDirectoryInfo.GetDirectories().Length;

				// Only continue if there is at least one user
				if (count == 0)
					return;

				string currentUser = App.Current.Properties["Current User"].ToString();
				string[] userList = (string[])App.Current.Properties["Students"];
				double progressUpdateValue = 1 / count;
				foreach (var folder in newDirectoryInfo.EnumerateDirectories())
				{
					// Only proceed if the user is not part of the administrator's profile
					if (userList.Contains(folder.Name))
						continue;

					string data;
					string location = System.IO.Path.Combine(folder.FullName,"config.txt");
					using (var newReader = new StreamReader(location))
						data = newReader.ReadToEnd();
					string name = BasicFunctions.GetValue(data, "Name");

					Button newButton = new Button() { Content = name, Tag = folder.Name};
					newButton.Click += newButton_Click;
					students.Children.Add(newButton);

					// Update progress
					if (App.mWindow != null)
						App.mWindow.Progress.Value += progressUpdateValue;
				}
			}
			catch (IOException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid User Path",
					"An error has occurred. The users path is invalid. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (ArgumentException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
					"An error has occurred. Invalid characters have been found in the users path. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (SecurityException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. Access to the users path has been denied. If the problem persists, please contact the Administrator.",
							NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}

			// Update the progress
			if (App.mWindow != null)
			{
				App.mWindow.Progress.Visibility = Visibility.Collapsed;
				App.mWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
			}
		}

		void newButton_Click(object sender, RoutedEventArgs e)
		{
			Button button = sender as Button;
			if (button != null)
			{
				selectedUser = button.Content.ToString();
				SelectedLocation = button.Tag.ToString();
				this.DialogResult = true;
				this.Close();
			}
		}

	}
}
