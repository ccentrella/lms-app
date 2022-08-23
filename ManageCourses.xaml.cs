using Record_Pro_Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Shell;
using System.Windows.Threading;

namespace RecordPro
{
	/// <summary>
	/// Interaction logic for ManageCourses.xaml
	/// </summary>
	public partial class ManageCourses : Page
	{
		string usersLocation = (string)App.Current.Properties["Users Location"];
		string gradeLocation = (string)App.Current.Properties["Grade File Location"];

		public ManageCourses()
		{
			InitializeComponent();
		}
		delegate void mainDelegate();

		private void NewGrade_Click(object sender, RoutedEventArgs e)
		{
			// Prompt the user to enter the name of the new grade.
			var newDialog = new InputDialog("Enter Grade Name", "Please enter the name of the new grade:");
			if (App.mWindow != null)
				newDialog.Owner = App.mWindow;
			if (newDialog.ShowDialog() == true)
				CreateGrade(newDialog.userInput.Text);
		}

		/// <summary>
		/// Add the new grade to the user's profile.
		/// </summary>
		/// <param name="name">The name of the new course</param>
		private void CreateGrade(string name)
		{
			string fileLocation = System.IO.Path.Combine(gradeLocation, name + ".txt"); ;
			if (App.Current.Properties["IsAdministrator"].ToString() == "True")
			{
				ComboBoxItem selectedItem = UserComboBox.SelectedItem as ComboBoxItem;
				if (selectedItem != null && selectedItem.Tag != null)
					fileLocation = Path.Combine(usersLocation, selectedItem.Tag.ToString(), "Grades", name + ".txt");
				else
				{
					MessageBox.Show("An error has occurred. Please try again.", "Error - Record Pro",
										MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}

			// Ensure the grade doesn't already exist
			if (File.Exists(fileLocation))
			{
				MessageBox.Show("The grade already exists. Please enter a new grade name.", "Grade already exists",
					MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			try
			{
				File.WriteAllText(fileLocation, "");
				grades.Items.Add(new ListBoxItem() { Content = name });
			}
			catch (IOException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid Path",
					"An error has occurred. The user path is invalid. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (ArgumentException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
					"An error has occurred. Invalid characters have been found in the name or user path. Ensure the name contains no invalid characters. "
				+ "If the problem continues, contact the Administrator.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (NotSupportedException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
					"An error has occurred. Invalid characters have been found in the name or user path. Ensure the name contains no invalid characters. "
				+ "If the problem continues, contact the Administrator.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
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

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			if (usersLocation == null | gradeLocation == null)
				return;
			await LoadUsers();
		}

		/// <summary>
		/// Asynchronously load all users
		/// </summary>
		/// <returns>A task object, used to manipulate the method</returns>
		private async Task LoadUsers()
		{
			// Either load the administrator combo box, or load the user
			if (App.Current.Properties["IsAdministrator"].ToString() == "True")
			{
				// Acquire the list of students
				string[] students = (string[])App.Current.Properties["Students"];

				// Add each student to the combo box
				foreach (var student in students)
				{
					try
					{
						string location = Path.Combine(usersLocation, student, "config.txt");
						string data;
						using (var newReader = new StreamReader(location))
							data = await newReader.ReadToEndAsync();
						string name = BasicFunctions.GetValue(data, "Name");
						await Dispatcher.BeginInvoke(DispatcherPriority.Background, new mainDelegate(() =>
						{
							UserComboBox.Items.Add(new ComboBoxItem() { Content = name, ToolTip = name, Tag = student });
						}));
					}
					catch (IOException)
					{
						MessageBox.Show("An error has occurred. A student could not be loaded.",
							"Error - Record Pro", MessageBoxButton.OK, MessageBoxImage.Error);
					}
					catch (ArgumentException)
					{
						MessageBox.Show("An error has occurred. A student could not be loaded.",
							"Error - Record Pro", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}

				// Add the current user to the combobox
				string currentUser = App.Current.Properties["Current User"].ToString();
				await Dispatcher.BeginInvoke(DispatcherPriority.Background, new mainDelegate(() =>
				{
					string currentLocation = (string)App.Current.Properties["Current User Location"];
					if (currentLocation == null)
						return;
					string tag = Path.GetFileName(currentLocation);
					UserComboBox.Items.Add(new ComboBoxItem() { Content = currentUser, ToolTip = currentUser, Tag=tag });
				}));

				// Now sort the items.
				UserComboBox.Items.SortDescriptions.Add(new SortDescription("Content", ListSortDirection.Ascending));

				// Show the combobox
				UserComboBox.SelectedIndex = 0;
				UserComboBox.Visibility = Visibility.Visible;
			}

			// Load the user
			else
			{
				string userName = (string)App.Current.Properties["Current User"];
				string userLocation = (string)App.Current.Properties["Current User Location"];
				if (userName != null & userLocation != null)
					await Dispatcher.BeginInvoke(DispatcherPriority.Background,
						new mainDelegate(() => { LoadGrades(userLocation, userName); }));
			}
		}

		/// <summary>
		/// Load the user's grade
		/// </summary>
		/// <param name="folderLocation">The folder that contain's the user's information</param>
		/// <param name="userName">The name of the user to load</param>
		private async void LoadGrades(string folderLocation, string userName)
		{
			// Update the progress
			if (App.mWindow != null)
			{
				App.mWindow.Progress.Value = 0;
				App.mWindow.Progress.ToolTip = "Loading Grades";
				App.mWindow.Progress.Visibility = Visibility.Visible;
				App.mWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;

			}

			try
			{
				string path = System.IO.Path.Combine(folderLocation, "Grades");

				// Only continue if the directory exists
				if (!Directory.Exists(path))
				{
					// Update the progress
					if (App.mWindow != null)
					{
						App.mWindow.Progress.Visibility = Visibility.Collapsed;
						App.mWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
					}
					return;
				}

				DirectoryInfo newDirectoryInfo = new DirectoryInfo(path);
				int count = newDirectoryInfo.GetFiles().Length;

				// Only continue if there is at least one grade
				if (count == 0)
				{
					// Update the progress
					if (App.mWindow != null)
					{
						App.mWindow.Progress.Visibility = Visibility.Collapsed;
						App.mWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
					}
					return;
				}

				double progressUpdateValue = 1 / count;

				// Clear all current items
				grades.Items.Clear();
				grades.Items.SortDescriptions.Clear();
				foreach (var file in newDirectoryInfo.EnumerateFiles())
				{
					// Add the item
					string newContent = System.IO.Path.GetFileNameWithoutExtension(file.FullName);
					await Dispatcher.BeginInvoke(DispatcherPriority.Background, new mainDelegate(() =>
					{
						grades.Items.Add(new ListBoxItem() { Content = newContent });

						// Update progress
						if (App.mWindow != null)
							App.mWindow.Progress.Value += progressUpdateValue;
					}));
				}

				// Enable sorting
				if (grades.Items.CanSort)
					grades.Items.SortDescriptions.Add(new SortDescription("Content", ListSortDirection.Ascending));
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
			catch (SecurityException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. Access to the user path has been denied. If the problem persists, please contact the Administrator.",
							NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}

			// Update the progress
			if (App.mWindow != null)
			{
				App.mWindow.Progress.Visibility = Visibility.Collapsed;
				App.mWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
			}
		}

		private void grades_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Clear all courses from the list
			courses.Items.Clear();

			// Animate the grade panel and show all courses for the current grade
			DoubleAnimation newAnimation = new DoubleAnimation() { Duration = TimeSpan.Parse("0:0:0.5") };
			if (grades.SelectedItems.Count > 0)
			{
				newAnimation.To = gradeOptions.ActualHeight;
				ListBoxItem selectedItem = grades.SelectedItem as ListBoxItem;

				// Load all courses if the conversion succeeded
				if (selectedItem != null)
					LoadCourses(selectedItem.Content.ToString());
			}

			// Close the grade panel
			else
			{
				newAnimation.To = 0;
			}
			gradeOptions.BeginAnimation(StackPanel.HeightProperty, newAnimation);
		}

		/// <summary>
		/// Load all courses for the specified grade
		/// </summary>
		/// <param name="gradeName">The grade which contains the courses to load</param>
		private async void LoadCourses(string gradeName)
		{
			string path = Path.Combine(gradeLocation, gradeName + ".txt");

			// Update the location if an administrator is logged in
			if (App.Current.Properties["IsAdministrator"].ToString() == "True")
			{
				ComboBoxItem selectedItem = UserComboBox.SelectedItem as ComboBoxItem;
				if (selectedItem != null && selectedItem.Tag != null)
					path = Path.Combine(usersLocation, selectedItem.Tag.ToString(), "Grades", gradeName + ".txt");
				else
				{
					MessageBox.Show("An error has occurred. Please try again.", "Error - Record Pro",
														MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}
			try
			{
				using (var reader = new StreamReader(path))
				{
					// Get the line containing attributes
					string data = await reader.ReadToEndAsync();

					// Find the courses
					string[] currentList = BasicFunctions.EnumerateStrings(data, "Courses");

					// Add each course to the course list
					foreach (string item in currentList)
						courses.Items.Add(new ListBoxItem() { Content = item, ToolTip = item });

					grades.Items.SortDescriptions.Clear();

					// Enable sorting
					if (courses.Items.CanSort)
						courses.Items.SortDescriptions.Add(new SortDescription("Content", ListSortDirection.Ascending));

				}
			}
			catch (IOException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					"Invalid User Grade File", "An error has occurred. The user grade file is invalid and the grade could not be loaded. "
					+ "If the problem continues, contact the Administrator.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (ArgumentException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					"Invalid Characters Found", "An error has occurred. Invalid characters have been found in the user grade file "
				+ "and the grade could not be updated. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
		}

		private void RenameButton_Click(object sender, RoutedEventArgs e)
		{
			var renameDialog = new InputDialog("Enter Grade Name", "Please enter the new grade name:");
			if (App.mWindow != null)
				renameDialog.Owner = App.mWindow;
			if (renameDialog.ShowDialog() == true)
			{
				if (string.IsNullOrWhiteSpace(renameDialog.userInput.Text))
				{
					NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Invalid Name - Record Pro", "The  grade name is invalid.",
					"Please enter a valid name.",
			NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
					return;
				}
				RenameGrade(renameDialog.userInput.Text); // Rename the file

			}
		}

		/// <summary>
		/// Renames the appropriate grade
		/// </summary>
		/// <param name="fileName">The name of the grade</param>
		private void RenameGrade(string gradeName)
		{
			ListBoxItem selectedGrade = grades.SelectedItem as ListBoxItem;
			if (selectedGrade == null)
				return;
			string oldFileLocation = System.IO.Path.Combine(gradeLocation, selectedGrade.Content + ".txt");
			string newFileLocation = System.IO.Path.Combine(gradeLocation, gradeName + ".txt");

			// Update the locations if the user is an administrator
			if (App.Current.Properties["IsAdministrator"].ToString() == "True")
			{
				ComboBoxItem selectedItem = UserComboBox.SelectedItem as ComboBoxItem;
				if (selectedItem != null && selectedItem.Tag != null)
				{
					string userLocation = System.IO.Path.Combine(usersLocation, selectedItem.Tag.ToString(), "Grades");
					oldFileLocation = System.IO.Path.Combine(userLocation, selectedGrade.Content + ".txt");
					newFileLocation = System.IO.Path.Combine(userLocation, gradeName + ".txt");

				}
				else
				{
					MessageBox.Show("An error has occurred. Please try again.", "Error - Record Pro",
														MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}

			// Ensure the file doesn't already exist. If the user enters the same name as before, silently ignore it.
			if (File.Exists(newFileLocation))
			{
				if (newFileLocation != oldFileLocation)
					NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Grade Already Exists - Record Pro", "The selected grade already exists.",
						"The new grade already exists. The file name could not be changed.",
				NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				return;
			}

			try
			{
				// Rename the file and update the list box
				File.Move(oldFileLocation, newFileLocation);
				selectedGrade.Content = gradeName;
				grades.Items.SortDescriptions.Clear();

				// Enable sorting
				if (grades.Items.CanSort)
					grades.Items.SortDescriptions.Add(new SortDescription("Content", ListSortDirection.Ascending));

			}
			catch (IOException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid Path",
					"An error has occurred. The user path is invalid. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (ArgumentException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
					"An error has occurred. Invalid characters have been found in the name or user path. Ensure the name contains no invalid characters. "
				+ "If the problem continues, contact the Administrator.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (NotSupportedException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
					"An error has occurred. Invalid characters have been found in the name or user path. Ensure the name contains no invalid characters. "
				+ "If the problem continues, contact the Administrator.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (UnauthorizedAccessException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. Access to the user path has been denied. If the problem persists, please contact the Administrator.",
									NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
		}

		private void DeleteButton_Click(object sender, RoutedEventArgs e)
		{
			DeleteSelectedGrade(); // Delete the selected grade
		}

		/// <summary>
		/// Deletes the selected file
		/// </summary>
		private void DeleteSelectedGrade()
		{
			ListBoxItem selectedGrade = grades.SelectedItem as ListBoxItem;
			if (selectedGrade == null)
				return;
			string location = System.IO.Path.Combine(gradeLocation, selectedGrade.Content + ".txt");

			// Change the location if the user is selected
			if (App.Current.Properties["IsAdministrator"].ToString() == "True")
			{
				ComboBoxItem selectedItem = UserComboBox.SelectedItem as ComboBoxItem;
				if (selectedItem != null && selectedItem.Tag != null)
				{
					string userLocation = Path.Combine(usersLocation, selectedItem.Tag.ToString(), "Grades");
					location = System.IO.Path.Combine(userLocation, selectedGrade.Content + ".txt");
				}
				else
				{
					MessageBox.Show("An error has occurred. Please try again.", "Error - Record Pro",
														MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}

			// Remove the selected grade only if the user agrees
			if (NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Delete Grade?",
				"Are you sure you want to delete this grade?", "Deleting a grade is permanent and cannot be undone.",
				NativeMethods.TaskDialogButtons.Yes | NativeMethods.TaskDialogButtons.No, NativeMethods.TaskDialogIcon.Warning) != NativeMethods.TaskDialogResult.Yes)
				return;

			try
			{
				File.Delete(location);
				grades.Items.Remove(selectedGrade);
				grades.Items.SortDescriptions.Clear();

				// Enable sorting
				if (grades.Items.CanSort)
					grades.Items.SortDescriptions.Add(new SortDescription("Content", ListSortDirection.Ascending));
			}
			catch (IOException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid Path",
					"An error has occurred. The user path is invalid. The grade could not removed. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (ArgumentException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
					"An error has occurred. Invalid characters have been found in the name or user path. The grade could not be removed. "
				+ "If the problem continues, contact the Administrator.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (NotSupportedException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
					"An error has occurred. Invalid characters have been found in the name or user path. The grade could not be removed. Ensure the name contains no invalid characters. "
				+ "If the problem continues, contact the Administrator.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (UnauthorizedAccessException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Access Denied",
		"An error has occurred. Access to the user path has been denied. The grade could not be removed. If the problem persists, please contact the Administrator.",
			NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
		}

		private void grades_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Delete)
				DeleteSelectedGrade(); // Remove the selected grade
		}

		private void courses_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Animate the course panel
			DoubleAnimation newAnimation = new DoubleAnimation() { Duration = TimeSpan.Parse("0:0:0.5") };
			if (courses.SelectedItems.Count > 0)
				newAnimation.To = courseOptions.ActualHeight;
			else
				newAnimation.To = 0;
			courseOptions.BeginAnimation(StackPanel.HeightProperty, newAnimation);
		}

		private void NewCourseButton_Click(object sender, RoutedEventArgs e)
		{
			// Ensure there is a valid selected item
			ListBoxItem selectedItem = grades.SelectedItem as ListBoxItem;
			if (selectedItem == null)
				return;

			// Prompt the user to enter the name of the new course.
			var newDialog = new InputDialog("Enter Course Name", "Please enter the name of the new course:");
			if (App.mWindow != null)
				newDialog.Owner = App.mWindow;

			// If the user clicks OK, add the new course
			if (newDialog.ShowDialog() == true)
				CreateCourse(newDialog.userInput.Text, selectedItem.Content.ToString());
		}

		/// <summary>
		/// Adds a new course
		/// </summary>
		/// <param name="courseName">The name of the course to add</param>
		private async void CreateCourse(string courseName, string gradeName)
		{
			var newItem = new ListBoxItem();
			List<string> courseList = new List<string>();
			string path = Path.Combine(gradeLocation, gradeName + ".txt");

			// Update the location if an administrator is logged in
			if (App.Current.Properties["IsAdministrator"].ToString() == "True")
			{
				ComboBoxItem selectedItem = UserComboBox.SelectedItem as ComboBoxItem;
				if (selectedItem != null && selectedItem.Tag != null)
					path = Path.Combine(usersLocation, selectedItem.Tag.ToString(), "Grades", gradeName + ".txt");
				else
				{
					MessageBox.Show("An error has occurred. Please try again.", "Error - Record Pro",
														MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}

			// Enumerate through each course
			foreach (var item in courses.Items)
			{
				ListBoxItem lItem = item as ListBoxItem;

				// Ensure the item is a valid ListBoxItem
				if (lItem == null)
					continue;

				// If a matching ListBoxItem is found, immediately terminate the operation
				else if (lItem.Content.ToString() == courseName)
				{
					MessageBox.Show("The specified course already exists. Please try again.",
						"Error - Record Pro", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				// Add the item to the course list
				else
					courseList.Add(lItem.Content.ToString());
			}
			courseList.Add(courseName.Replace("\\", "\\\\").Replace("\"", "\\\"")); // Add the new course
			courseList.Sort(); // Attempt to sort the list
			try
			{
				string newData;
				using (var reader = new StreamReader(path))
				{
					string data = await reader.ReadToEndAsync(); // Get the line containing attributes
					string newInput = string.Join(",", courseList.ToArray());
					newData = BasicFunctions.ReplaceValue(data, "Courses", newInput, true); // Update the list of courses
				}
				File.WriteAllText(path, newData); // Update the file
				courses.Items.Add(new ListBoxItem() { Content = courseName, ToolTip = courseName });
				courses.Items.SortDescriptions.Clear();

				// Enable sorting
				if (courses.Items.CanSort)
					courses.Items.SortDescriptions.Add(new SortDescription("Content", ListSortDirection.Ascending));
			}

			catch (IOException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					"Invalid User Grade File", "An error has occurred. The user grade file is invalid and the course could not be added. "
					+ "If the problem continues, contact the Administrator.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (ArgumentException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					"Invalid Characters Found", "An error has occurred. Invalid characters have been found in the user grade file "
				+ "and the course could not be added. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (NotSupportedException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
								"Invalid Characters Found", "An error has occurred. Invalid characters have been found in the user grade file "
							+ "and the course could not be added. If the problem continues, contact the Administrator.",
								NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (SecurityException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
												"Access Denied", "An error has occurred. Access was denied "
											+ "and the course could not be added. If the problem continues, contact the Administrator.",
												NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (UnauthorizedAccessException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
												"Access Denied", "An error has occurred. Access was denied "
											+ "and the course could not be added. If the problem continues, contact the Administrator.",
												NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}

		}

		private void UserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBoxItem selectedItem = UserComboBox.SelectedItem as ComboBoxItem;

			// Ensure a valid ComboBoxItem is selected
			if (selectedItem == null || selectedItem.Tag == null)
				return;

			LoadGrades(System.IO.Path.Combine(usersLocation,selectedItem.Tag.ToString()),selectedItem.Content.ToString()); // Load the all grades for the user
		}

		private void RenameCourse_Click(object sender, RoutedEventArgs e)
		{
			ListBoxItem selectedGrade = grades.SelectedItem as ListBoxItem;

			// Only continue if the item is not null
			if (selectedGrade == null)
				return;

			// Prompt the user for a new name
			var newDialog = new InputDialog("Rename Course", "Please enter the new course name:");
			if (App.mWindow != null)
				newDialog.Owner = App.mWindow;

			// If OK was clicked, rename the course
			if (newDialog.ShowDialog() == true)
				RenameCourse(selectedGrade.Content.ToString(), newDialog.userInput.Text);
		}

		/// <summary>
		/// Renames the selected course
		/// </summary>
		/// <param name="gradeName">The grade which contains the course to be renamed</param>
		/// <param name="newCourseName">The new name of the course</param>
		private async void RenameCourse(string gradeName, string newCourseName)
		{
			var courseList = new List<string>();
			string path = Path.Combine(gradeLocation, gradeName + ".txt");
			string courseName = "";

			// Update the location if an administrator is logged in
			if (App.Current.Properties["IsAdministrator"].ToString() == "True")
			{
				ComboBoxItem selectedItem = UserComboBox.SelectedItem as ComboBoxItem;
				if (selectedItem != null && selectedItem.Tag != null)
					path = Path.Combine(usersLocation, selectedItem.Tag.ToString(), "Grades", gradeName + ".txt");
				else
				{
					MessageBox.Show("An error has occurred. Please try again.", "Error - Record Pro",
														MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}

			ListBoxItem selectedCourse = courses.SelectedItem as ListBoxItem;

			// Only continue if the item is not null
			if (selectedCourse == null)
				return;

			courseName = selectedCourse.Content.ToString(); // Get the text of the item to be renamed

			// Enumerate through each course
			foreach (var item in courses.Items)
			{
				ListBoxItem lItem = item as ListBoxItem;

				// Ensure the item is a valid ListBoxItem and is not the old item
				if (lItem == null || lItem == selectedCourse)
					continue;

				// If a matching ListBoxItem is found, immediately terminate the operation
				else if (lItem.Content.ToString() == newCourseName)
				{
					MessageBox.Show("The specified course already exists. Please try again.",
						"Error - Record Pro", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				// Add the item to the course list
				else
					courseList.Add(lItem.Content.ToString());
			}
			courseList.Add(newCourseName.Replace("\\", "\\\\").Replace("\"", "\\\"")); // Add the new course
			courseList.Sort(); // Attempt to sort the list
			try
			{
				string newData;
				StringBuilder builder = new StringBuilder();
				using (var reader = new StreamReader(path))
				{
					// Check each line to make sure no instances of the course exist
					while (reader.Peek() > -1)
					{
						string data = await reader.ReadLineAsync(); // Get the line containing attributes
						string course = BasicFunctions.GetValue(data, "Course"); // Get the course

						// If the line is valid, add it to the string
						if (string.IsNullOrWhiteSpace(data))
							continue;
						else if (course == courseName)
							data = BasicFunctions.ReplaceValue(data, "Course", newCourseName); // Rename the course

						// Add the line
						builder.AppendLine(data);
					}
				}
				string newInput = string.Join(",", courseList.ToArray()); // Join all the new courses
				newData = BasicFunctions.ReplaceValue(builder.ToString(), "Courses", newInput, true);

				File.WriteAllText(path, newData); // Update the file
				selectedCourse.Content = newCourseName; // Update the ListBox
				courses.Items.SortDescriptions.Clear();

				// Enable sorting
				if (courses.Items.CanSort)
					courses.Items.SortDescriptions.Add(new SortDescription("Content", ListSortDirection.Ascending));
			}
			catch (IOException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					"Invalid User Grade File", "An error has occurred. The user grade file is invalid and the course could not be renamed. "
					+ "If the problem continues, contact the Administrator.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (ArgumentException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					"Invalid Characters Found", "An error has occurred. Invalid characters have been found in the user grade file "
				+ "and the course could not be renamed. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (NotSupportedException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
								"Invalid Characters Found", "An error has occurred. Invalid characters have been found in the user grade file "
							+ "and the course could not be renamed. If the problem continues, contact the Administrator.",
								NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (SecurityException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
												"Access Denied", "An error has occurred. Access was denied "
											+ "and the course could not be renamed. If the problem continues, contact the Administrator.",
												NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (UnauthorizedAccessException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
												"Access Denied", "An error has occurred. Access was denied "
											+ "and the course could not be renamed. If the problem continues, contact the Administrator.",
												NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
		}

		private void courses_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Delete)
			{
				ListBoxItem selectedGrade = grades.SelectedItem as ListBoxItem;

				// Only continue if the item is not null
				if (selectedGrade == null)
					return;

				// Remove the selected course
				DeleteSelectedCourse(selectedGrade.Content.ToString());
			}
		}

		private void DeleteCourse_Click(object sender, RoutedEventArgs e)
		{
			ListBoxItem selectedGrade = grades.SelectedItem as ListBoxItem;

			// Only continue if the item is not null
			if (selectedGrade == null)
				return;

			// Remove the selected course
			DeleteSelectedCourse(selectedGrade.Content.ToString());
		}

		/// <summary>
		/// Deletes the selected course
		/// </summary>
		/// <param name="gradeName">The name of the grade which contains the course to be deleted</param>
		private async void DeleteSelectedCourse(string gradeName)
		{
			// Warn the user.
			if (NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero,
				"Warning - Record Pro", "Are you would like to delete this course?", "This is permanent and cannot be undone.",
				NativeMethods.TaskDialogButtons.Yes | NativeMethods.TaskDialogButtons.No,
			NativeMethods.TaskDialogIcon.Warning) == NativeMethods.TaskDialogResult.No)
				return;

			var courseList = new List<string>();
			string path = Path.Combine(gradeLocation, gradeName + ".txt");
			string courseName = "";

			// Update the location if an administrator is logged in
			if (App.Current.Properties["IsAdministrator"].ToString() == "True")
			{
				ComboBoxItem selectedItem = UserComboBox.SelectedItem as ComboBoxItem;
				if (selectedItem != null && selectedItem.Tag != null)
					path = Path.Combine(usersLocation, selectedItem.Tag.ToString(), "Grades", gradeName + ".txt");
				else
				{
					MessageBox.Show("An error has occurred. Please try again.", "Error - Record Pro",
														MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}

			ListBoxItem selectedCourse = courses.SelectedItem as ListBoxItem;

			// Only continue if the item is not null
			if (selectedCourse == null)
				return;

			courseName = selectedCourse.Content.ToString(); // Get the text of the item marked for deletion

			// Enumerate through each course
			foreach (var item in courses.Items)
			{
				ListBoxItem lItem = item as ListBoxItem;

				// Ensure the item is a valid ListBoxItem and is not the item that will be deleted
				if (lItem == null || lItem == selectedCourse)
					continue;

					// Add the item to the course list
				else
					courseList.Add(lItem.Content.ToString());
			}
			try
			{
				string newData;
				StringBuilder builder = new StringBuilder();
				using (var reader = new StreamReader(path))
				{
					// Check each line to make sure no instances of the course exist
					while (reader.Peek() > -1)
					{
						string data = await reader.ReadLineAsync(); // Get the line containing attributes
						string course = BasicFunctions.GetValue(data, "Course"); // Get the course

						// If the line is valid, add it to the string
						if (course != courseName && !string.IsNullOrWhiteSpace(data))
							builder.AppendLine(data);
					}
				}
				courses.Items.Remove(selectedCourse); // Remove the course from the ListBox
				string newInput = string.Join(",", courseList.ToArray()); // Join all the new courses
				newData = BasicFunctions.ReplaceValue(builder.ToString(), "Courses", newInput, true);

				File.WriteAllText(path, newData); // Update the file
			}

			catch (IOException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					"Invalid User Grade File", "An error has occurred. The user grade file is invalid and the course could not be deleted. "
					+ "If the problem continues, contact the Administrator.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (ArgumentException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					"Invalid Characters Found", "An error has occurred. Invalid characters have been found in the user grade file "
				+ "and the course could not be deleted. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (NotSupportedException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
								"Invalid Characters Found", "An error has occurred. Invalid characters have been found in the user grade file "
							+ "and the course could not be deleted. If the problem continues, contact the Administrator.",
								NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (SecurityException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
												"Access Denied", "An error has occurred. Access was denied "
											+ "and the course could not be deleted. If the problem continues, contact the Administrator.",
												NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (UnauthorizedAccessException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
												"Access Denied", "An error has occurred. Access was denied "
											+ "and the course could not be deleted. If the problem continues, contact the Administrator.",
												NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
		}
	}
}
