namespace RecordPro
{
	using System;
	using System.ComponentModel;
	using System.IO;
	using System.Security;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using System.Windows.Interop;
	using System.Windows.Threading;
	using System.Windows.Shell;
	using System.Text;
	using System.Linq;
	using System.Collections.Generic;
	using System.Windows.Data;
	using Record_Pro_Functions;

	/// <summary>
	/// Interaction logic for UpdateRecord.xaml
	/// </summary>
	public partial class UpdateRecord : Page
	{
		// Delegates used for asynchronous programming
		DataGridRow previousRow;
		delegate void mainDelegate();

		public UpdateRecord()
		{
			InitializeComponent();
		}
		/// <summary>
		/// Adds the names of all the students to the popup.
		/// </summary>
		private async void AddStudents()
		{
			// Ensure the users directory exists
			string myLocation = (string)App.Current.Properties["Current User Location"];
			string usersLocation = (string)App.Current.Properties["Users Location"];
			string[] studentList = (string[])App.Current.Properties["Students"];
			bool errorOccurred = false; // True if one or more errors occurred

			// Ensure that the student list and usersLocation are not null
			if (usersLocation == null | studentList == null | myLocation == null)
			{
				MessageBox.Show("Record Pro has not been installed properly. Please reinstall Record Pro.",
					"Error - Record Pro", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			// Add the administrator
			await Dispatcher.BeginInvoke(DispatcherPriority.Background,
				new mainDelegate(() =>
				{
					User.Items.Add(new ComboBoxItem() { Content = "(Me)", Tag = myLocation });
				}));

			// Show the student box
			User.Visibility = Visibility.Visible;

			// Now Add each user to the list
			foreach (var item in studentList)
			{
				// Get the location and ensure that it exists
				string location = Path.Combine(usersLocation, item);
				string configLocation = Path.Combine(location, "config.txt");
				if (!Directory.Exists(location))
					continue;

				// Add the user
				string data = "";
				try
				{
					using (var newReader = new StreamReader(configLocation))
						data = await newReader.ReadToEndAsync();
				}
				catch (IOException)
				{
					errorOccurred = true;
				}
				catch (ArgumentException)
				{
					errorOccurred = true;
				}

				// Add the user to the list
				if (data != null)
				{
					string name = BasicFunctions.GetValue(data, "Name");
					await Dispatcher.BeginInvoke(DispatcherPriority.Background,
						new mainDelegate(() =>
						{
							User.Items.Add(new ComboBoxItem() { Content = name, Tag = location });
						}));
				}
			}

			// Sort the items.
			if (User.Items.CanSort)
				User.Items.SortDescriptions.Add(new SortDescription("Content", ListSortDirection.Ascending));


			// Now warn the user if an error occurred
			if (errorOccurred)
				MessageBox.Show("An error has occurred. Not all users may have been loaded.",
					"Error - Record Pro", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			// Only load all students and subjects if the record directory exists
			if (!Directory.Exists((string)App.Current.Properties["Users Location"]))
				return;

			// Make today the selected calendar date
			RecordDate.SelectedDate = DateTime.Today;

			// If the current user is an administrator, load all user names. Otherwise, load the current user.
			if ((bool?)Application.Current.Properties["IsAdministrator"] == true)
				AddStudents();
			else
				PopulateUser((string)App.Current.Properties["Current User Location"]);
		}

		/// <summary>
		/// Populates the specified user.
		/// </summary>
		/// <param name="userPath">The user's location</param>
		private async void PopulateUser(string userPath)
		{
			string gradePath = Path.Combine(userPath, "Grades");

			// Clear all grades
			Grade.Items.Clear();

			// Load all grades if the user folder exists.
			if (!Directory.Exists(gradePath))
				return;
			try
			{

				foreach (var item in Directory.EnumerateFiles(gradePath))
					await Dispatcher.BeginInvoke(DispatcherPriority.Background, new mainDelegate(() =>
					{
						Grade.Items.Add(new ComboBoxItem()
						{
							Content = System.IO.Path.GetFileNameWithoutExtension(item),
							Tag = item
						});
					}));
				Grade.SelectedIndex = 0;
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
			catch (SecurityException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. Access to the user path has been denied. If the problem persists, please contact the Administrator.",
									NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
		}


		private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			// Determine whether or not the save button should be enabled.
			if (previousRow == null || !Validation.GetHasError(previousRow))
				e.CanExecute = true;
			else
				e.CanExecute = false;
		}

		private void data_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			previousRow = e.Row;
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			// Return to the home dialog.
			var newHome = new Home();
			this.NavigationService.Navigate(newHome);
		}

		private void User_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBoxItem sComboBoxItem = User.SelectedItem as ComboBoxItem;
			if (sComboBoxItem == null || sComboBoxItem.Tag == null)
				return;

			PopulateUser(sComboBoxItem.Tag.ToString()); // Populate the selected user
		}

		private async void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			// If data errors are found, don't proceed
			if (previousRow != null && Validation.GetHasError(previousRow))
				return;

			// Attempt to save the user's record asynchronously
			await Dispatcher.BeginInvoke(DispatcherPriority.Send, new mainDelegate(SaveRecord));

		}

		/// <summary>
		/// Save the current user's record
		/// </summary>
		private void SaveRecord()
		{
			#region Variables
			string recordPath;
			string grade = Grade.Text + ".txt";
			var stringList = new List<string>();
			DateTime? selectedDate = RecordDate.SelectedDate;
			DateTime newDate;
			double progressIncrementValue;
			ComboBoxItem selectedItem = Grade.SelectedItem as ComboBoxItem;
			#endregion

			// Only continue if a grade is selected
			if (selectedItem == null)
			{
				if (NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Warning - Record Pro",
					"No grade has been selected.", "Please select a valid grade in order to save this record.",
					NativeMethods.TaskDialogButtons.OK | NativeMethods.TaskDialogButtons.Cancel,
					NativeMethods.TaskDialogIcon.Warning) == NativeMethods.TaskDialogResult.Cancel)
					return;
			}


			// Only continue if there is at least one item.
			if (data.Items.Count < 2)
			{
				if (NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Warning - Record Pro",
									"No assignments have been completed", "Please add at least one assignment in order to save the record.",
									NativeMethods.TaskDialogButtons.OK | NativeMethods.TaskDialogButtons.Cancel,
									NativeMethods.TaskDialogIcon.Warning) == NativeMethods.TaskDialogResult.Cancel)
					return;
			}

			if (selectedItem.Tag != null)
				recordPath = selectedItem.Tag.ToString();
			else
			{
				MessageBox.Show("An error has occurred. Please contact Autosoft.",
					"Error - Record Pro", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			// Only continue if the file exists
			if (!File.Exists(recordPath))
			{
				MessageBox.Show("The user grade file doesn't exist. It is recommended to repair the user.",
					"User Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			// Update the date, making it today, if no date is selected
			if (selectedDate == null)
				newDate = DateTime.Today;
			else
				newDate = (DateTime)selectedDate;

			// Set the main window progress bar.
			if (App.mWindow != null)
			{
				App.mWindow.Progress.ToolTip = "Saving record";
				App.mWindow.Progress.Visibility = Visibility.Visible;
				App.mWindow.Progress.Value = 0;
				App.mWindow.WindowProgress.ProgressState = TaskbarItemProgressState.Normal;

			}
			progressIncrementValue = (data.Items.Count - 1) / 2;

			// Enumerate through each item
			foreach (var item in data.Items)
			{
				StringBuilder newBuilder = new StringBuilder();
				Assignment record = item as Assignment;

				// If the item cannot be converted to a record, reject it.
				if (record == null)
					continue;

				// Trim all fields
				if (record.Course != null)
					record.Course = record.Course.Trim();
				if (record.Data != null)
					record.Data = record.Data.Trim();
				if (record.Notes != null)
					record.Notes = record.Notes.Trim();

				// Ensure fields are not null, which would result in an exception
				if (record.Course == null)
					record.Course = " ";
				if (record.Data == null)
					record.Data = " ";
				if (record.Notes == null)
					record.Notes = " ";

				// Parse the info and add the item
				byte? newGrade = record.Grade;
				if (newGrade > 100)
					newGrade = 100;
				newBuilder.AppendFormat("Date = \"{0}\", Course = \"{1}\", Data = \"{2}\", Time = \"{3}\", Grade = \"{4}\", Notes = \"{5}\"",
					newDate.ToShortDateString(), record.Course.Replace("\\", "\\\\").Replace("\"", "\\\""), record.Data.Replace("\\", "\\\\").Replace("\"", "\\\""),
					record.Time, newGrade, record.Notes.Replace("\\", "\\\\").Replace("\"", "\\\""));
				string newString = newBuilder.ToString();
				stringList.Add(newString);
				if (App.mWindow != null)
					App.mWindow.Progress.Value += progressIncrementValue;
			}

			// Attempt to update the record
			try
			{
				using (var newStreamWriter = new StreamWriter(recordPath, true))
				{
					stringList.ForEach((s) =>
					{
						newStreamWriter.WriteLine(s);
						if (App.mWindow != null)
							App.mWindow.Progress.Value += progressIncrementValue;
					});
				}

				// If we've made it this far, the record has been successfully saved.
				// The user can now go home
				var newHome = new Home();
				this.NavigationService.Navigate(newHome);
			}
			catch (IOException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					"Invalid User Grade File", "An error has occurred. The user grade file is invalid and the record could not be updated. "
					+ "If the problem continues, contact the Administrator.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (ArgumentException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					"Invalid Characters Found", "An error has occurred. Invalid characters have been found in the user grade file "
				+ "and the record could not be updated. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (UnauthorizedAccessException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					"Access Denied", "An error has occurred. Access to the user grade file has been denied and the record could not be updated. "
				+ "If the problem persists, please contact the Administrator.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (SecurityException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					"Access Denied", "An error has occurred. Access to the user grade file has been denied and the record could not be updated. "
				+ "If the problem persists, please contact the Administrator.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			finally
			{
				// Now update the progress bar
				if (App.mWindow != null)
				{
					App.mWindow.Progress.Value = 0;
					App.mWindow.Progress.Visibility = Visibility.Collapsed;
					App.mWindow.WindowProgress.ProgressState = TaskbarItemProgressState.None;
				}
			}
		}

		private void data_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
		{
			// Adjust the width of certain columns
			if (e.PropertyName == "Course")
			{
				var newColumn = new DataGridComboBoxColumn() { SelectedItemBinding = new Binding("Course"), Header = "Course" };
				newColumn.EditingElementStyle = (Style)App.Current.Resources["DataColumnComboBoxStyle"];
				newColumn.SortDirection = ListSortDirection.Ascending;
				newColumn.Width = 130;
				e.Column = newColumn;
			}
			else if (e.PropertyName == "Data")
				e.Column.Width = 350;
			else if (e.PropertyName == "Grade")
				e.Column.Width = 50;

		}

		private async void Grade_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			List<string> courseList = new List<string>();
			string path;

			// Only continue if at least one column has been loaded
			if (data.Columns.Count == 0)
				return;

			// Ensure the course column is in the correct place
			DataGridComboBoxColumn dColumn = data.Columns[0] as DataGridComboBoxColumn;
			if (dColumn == null)
				return;

			// Attempt to get the new grade
			ComboBoxItem selectedItem = Grade.SelectedItem as ComboBoxItem;
			if (selectedItem == null || selectedItem.Tag == null)
				return;

			path = selectedItem.Tag.ToString();
			try
			{
				using (var reader = new StreamReader(path))
				{
					// Get the line containing attributes
					string userData = await reader.ReadToEndAsync();

					// Find the courses
					string[] currentList = BasicFunctions.EnumerateStrings(userData, "Courses");
					foreach (string item in currentList)
						courseList.Add(item);
				}

			}
			catch (IOException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					"Invalid User Grade File", "An error has occurred. The user grade file is invalid and the record could not be updated. "
					+ "If the problem continues, contact the Administrator.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (ArgumentException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					"Invalid Characters Found", "An error has occurred. Invalid characters have been found in the user grade file "
				+ "and the record could not be updated. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			dColumn.ItemsSource = courseList;
		}

	}
}
