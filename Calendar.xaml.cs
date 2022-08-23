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
using System.Windows.Threading;
using System.Windows.Shell;
using System.IO;
using Record_Pro_Functions;
using System.Windows.Interop;
using System.Security;
using System.Threading;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace RecordPro
{
	/// <summary>
	/// Interaction logic for Calendar.xaml
	/// </summary>
	public partial class Calendar : Page
	{
		delegate void mainDelegate();

		// Prevent the same thread from being accessed at the same time.
		SemaphoreSlim newSempahore = new SemaphoreSlim(1, 1);

		bool disposed = false; // True if the semaphore has been disposed

		public Calendar()
		{
			InitializeComponent();
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			calendar.SelectedDate = DateTime.Today;
			string configLocation = System.IO.Path.Combine((string)App.Current.Properties["Current User Location"], "config.txt");
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

			// Show the statistics popup if necessary
			if (BasicFunctions.GetValue(data, "ShowCalendarPopup") != "False")
				CalendarPopup.IsOpen = true;
		}

		private async void calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
		{
			await newSempahore.WaitAsync();
			await UpdateRecords();
			if (!disposed)
				newSempahore.Release();
		}

		/// <summary>
		/// Update all records for the current day
		/// </summary>
		private async Task UpdateRecords()
		{
			#region SetProgressBar
			if (App.mWindow != null)
			{
				App.mWindow.Progress.Value = 0;
				App.mWindow.Progress.ToolTip = "Loading Records";
				App.mWindow.Progress.Visibility = Visibility.Visible;
				App.mWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
			}
			#endregion
			#region Variables
			string usersLocation = (string)App.Current.Properties["Users Location"];
			string currentUser = (string)App.Current.Properties["Current User"];
			string currentUserLocation = (string)App.Current.Properties["Current User Location"];
			bool isAdministrator = (bool)App.Current.Properties["IsAdministrator"];
			DateTime? selectedDate = calendar.SelectedDate;
			int count = 0;  // Count is used for the tag and is incremented each time
			#endregion

			if (usersLocation == null | currentUserLocation == null | currentUser == null)
				return;

			try
			{
				var usersDirectoryInfo = new DirectoryInfo(usersLocation);

				// Get the user properties
				string configData = File.ReadAllText(System.IO.Path.Combine(currentUserLocation, "config.txt"));
				double length = usersDirectoryInfo.GetDirectories().Length; // Get the total number of users

				// Only continue if at least one user has been found
				if (length < 1)
					return;

				Mouse.Capture(null); // Ensure the mouse focus is no longer on the calendar

				records.Items.Clear(); // Clear the list of items

				// Represents the amount to increment the progress bar by
				double progressUpdateValue = 1 / (double)usersDirectoryInfo.GetDirectories().Length;

				foreach (var directory in usersDirectoryInfo.EnumerateDirectories())
				{
					string configLocation = System.IO.Path.Combine(directory.FullName, "config.txt");
					string directoryData;
					using (var newReader = new StreamReader(configLocation))
						directoryData = await newReader.ReadToEndAsync();
					string name = BasicFunctions.GetValue(directoryData, "Name");
					string recordLocation = System.IO.Path.Combine(directory.FullName, "Grades");

					// Only proceed if the grade directory exists
					if (!Directory.Exists(recordLocation))
						continue;

					var currentDirectoryInfo = new DirectoryInfo(recordLocation);
					double currentUserProgressUpdateValue = currentDirectoryInfo.GetFiles().Length;

					// Ensure that the directory contains data for the current user
					// Administrators can view data for their students
					if (name != currentUser && !isAdministrator)
					{
						if (App.mWindow != null)
							App.mWindow.Progress.Value += progressUpdateValue;
						continue;
					}

					else if (isAdministrator & name != currentUser)
					{
						string[] studentList = (string[])Application.Current.Properties["Students"];
						if (!studentList.Contains(directory.Name))
						{
							if (App.mWindow != null)
								App.mWindow.Progress.Value += progressUpdateValue;
							continue;
						}
					}

					// Check every grade of the user
					foreach (var gradeFile in currentDirectoryInfo.EnumerateFiles())
					{
						int currentCount = 0;  // Count is used for the tag and is incremented each time
						string gradeFileName = System.IO.Path.GetFileNameWithoutExtension(gradeFile.Name);
						Grid grid1 = new Grid();
						grid1.ColumnDefinitions.Add(new ColumnDefinition());
						grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
						TextBlock textBlock1 = new TextBlock() { Text = name };
						textBlock1.SetResourceReference(TextBlock.StyleProperty, "UserLabels");
						Label label1 = new Label() { Content = gradeFileName };
						Grid.SetColumn(label1, 1);
						label1.SetResourceReference(Label.StyleProperty, "GLevelLabels");
						var converter1 = new UserToolTipConverter();
						Binding newBinding = new Binding("Items.Count") { Converter = converter1 };
						newBinding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(TreeViewItem), 1);
						label1.SetBinding(Label.ToolTipProperty, newBinding);
						grid1.Children.Add(textBlock1);
						grid1.Children.Add(label1);
						TreeViewItem newUserItem = new TreeViewItem() { Tag = gradeFile.FullName, Header = grid1 };
						ContextMenu newContextMenu = new ContextMenu() { Tag = count };
						newContextMenu.Opened += userContextMenu_Opened;
						newContextMenu.Items.Add(new MenuItem() { Header = "_Copy", Command = ApplicationCommands.Copy });
						newContextMenu.Items.Add(new Separator());
						newContextMenu.Items.Add(new MenuItem() { Header = "_Delete", Command = ApplicationCommands.Delete });
						newUserItem.ContextMenu = newContextMenu;
						using (var newReader = new StreamReader(gradeFile.FullName))
						{
							while (newReader.Peek() > -1)
							{
								#region Variables
								string data = await newReader.ReadLineAsync();
								string dateString = BasicFunctions.GetValue(data, "Date");
								string courseString = BasicFunctions.GetValue(data, "Course");
								string dataString = BasicFunctions.GetValue(data, "Data");
								string timeString = BasicFunctions.GetValue(data, "Time");
								string gradeString = BasicFunctions.GetValue(data, "Grade");
								string notesString = BasicFunctions.GetValue(data, "Notes");
								DateTime date;
								TimeSpan time;
								byte grade;
								bool hasGrade;
								#endregion

								// Only continue with the operation if the date is valid
								if (!DateTime.TryParse(dateString, out date))
									continue;
								if (date != selectedDate)
									continue;
								TimeSpan.TryParse(timeString, out time);
								hasGrade = byte.TryParse(gradeString, out grade);

								// Ensure there is readable text.
								if (dataString == "")
									dataString = "General";
								if (courseString == "")
									courseString = "Completed all required assignments";
								Grid newGrid = new Grid();
								newGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
								newGrid.ColumnDefinitions.Add(new ColumnDefinition());
								newGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
								Label courseLabel = new Label() { Content = courseString };
								courseLabel.SetResourceReference(Label.StyleProperty, "CourseLabels");
								string timeLabelContent = time.GetTimeLabel();
								TextBlock timeLabel = new TextBlock() { Text = timeLabelContent, Tag = time };
								Grid.SetColumn(timeLabel, 1);
								timeLabel.SetResourceReference(TextBlock.StyleProperty, "TimeStyle");
								Label gradeLabel = new Label() { Content = grade };
								Grid.SetColumn(gradeLabel, 2);

								// Show the grade label if it is not null and update the grade text
								if (!hasGrade)
								{
									gradeLabel.Visibility = Visibility.Collapsed;
									gradeLabel.Content = "";
								}

								// Update the styles
								if (grade == 100)
									gradeLabel.SetResourceReference(Label.StyleProperty, "GradeStyle1");
								else if (grade >= 95)
									gradeLabel.SetResourceReference(Label.StyleProperty, "GradeStyle2");
								else if (grade >= 85)
									gradeLabel.SetResourceReference(Label.StyleProperty, "GradeStyle3");
								else if (grade >= 75)
									gradeLabel.SetResourceReference(Label.StyleProperty, "GradeStyle4");
								else
									gradeLabel.SetResourceReference(Label.StyleProperty, "GradeStyle5");
								newGrid.Children.Add(courseLabel);
								newGrid.Children.Add(timeLabel);
								Binding courseHeaderBinding = new Binding("Children[0].Content") { RelativeSource = RelativeSource.Self };
								newGrid.SetBinding(Grid.TagProperty, courseHeaderBinding);

								// Add the grade
								newGrid.Children.Add(gradeLabel);
								TextBlock dataLabel = new TextBlock() { Text = dataString, Tag = notesString };


								if (!string.IsNullOrWhiteSpace(notesString))
								{
									Grid toolTipGrid = new Grid();
									toolTipGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
									toolTipGrid.RowDefinitions.Add(new RowDefinition());
									Label tooltipHeaderLabel = new Label() { Content = "Notes" };
									tooltipHeaderLabel.SetResourceReference(Label.StyleProperty, "NotesHeaderStyle");
									TextBlock toolTipBody = new TextBlock() { Text = notesString };
									toolTipBody.SetResourceReference(TextBlock.StyleProperty, "NotesStyle");
									Grid.SetRow(toolTipBody, 1);
									toolTipGrid.Children.Add(tooltipHeaderLabel);
									toolTipGrid.Children.Add(toolTipBody);
									ToolTip newToolTip = new ToolTip() { Content = toolTipGrid };
									newToolTip.SetResourceReference(System.Windows.Controls.ToolTip.StyleProperty, "NotesToolTip");
									dataLabel.ToolTip = newToolTip;
								}
								dataLabel.SetResourceReference(TextBlock.StyleProperty, "DataStyle");
								TreeViewItem newCourseItem = new TreeViewItem() { Header = newGrid, Tag = currentCount };
								int[] newArray = new[] { count, currentCount++ };
								ContextMenu courseContextMenu = new ContextMenu() { Tag = newArray };
								courseContextMenu.Opened += courseContextMenu_Opened;
								courseContextMenu.Items.Add(new MenuItem() { Header = "_Copy", Command = ApplicationCommands.Copy });
								courseContextMenu.Items.Add(new Separator());
								MenuItem modify = new MenuItem() { Header = "_Modify" };
								MenuItem courseMenuItem = new MenuItem() { Header = "_Course" };
								MenuItem dataMenuItem = new MenuItem() { Header = "_Data" };
								MenuItem gradeMenuItem = new MenuItem() { Header = "_Grade" };
								MenuItem notesMenuItem = new MenuItem() { Header = "_Notes" };
								MenuItem timeMenuItem = new MenuItem() { Header = "_Time" };
								courseMenuItem.Click += courseMenuItem_Click;
								dataMenuItem.Click += dataMenuItem_Click;
								gradeMenuItem.Click += gradeMenuItem_Click;
								notesMenuItem.Click += notesMenuItem_Click;
								timeMenuItem.Click += timeMenuItem_Click;
								modify.Items.Add(courseMenuItem);
								modify.Items.Add(dataMenuItem);
								modify.Items.Add(gradeMenuItem);
								modify.Items.Add(notesMenuItem);
								modify.Items.Add(timeMenuItem);
								courseContextMenu.Items.Add(modify);
								courseContextMenu.Items.Add(new MenuItem() { Header = "_Delete", Command = ApplicationCommands.Delete });
								newCourseItem.ContextMenu = courseContextMenu;
								newCourseItem.Items.Add(dataLabel);

								// Expand the course if it is enabled
								if (BasicFunctions.GetValue(configData, "ExpandCourses") == "True")
									newCourseItem.IsExpanded = true;

								// Add the item to the user item
								newUserItem.Items.Add(newCourseItem);

								// Sort the items if possible
								newUserItem.Items.SortDescriptions.Clear();
								if (newUserItem.Items.CanSort)
									newUserItem.Items.SortDescriptions.Add(new SortDescription("Header.Tag", ListSortDirection.Ascending));

								// Update the item tags, which is required after alphabetizing the items
								foreach (var item in newUserItem.Items)
								{
									TreeViewItem tItem = item as TreeViewItem;

									// Ensure the item is a tree view item and that it has a valid context menu
									if (tItem == null || tItem.ContextMenu == null)
										continue;
								}

								// Add the user item if it has not already been added
								if (!records.Items.Contains(newUserItem))
								{
									records.Items.Add(newUserItem);

									// Expand the user if it is enabled
									if (BasicFunctions.GetValue(configData, "ExpandUser") == "True")
										newUserItem.IsExpanded = true;
								}
							}

							// Increment count
							if (currentCount > 0)
								count++;

						}



						// Update the progress bar
						if (App.mWindow != null)
						{
							App.mWindow.Progress.Value += progressUpdateValue / currentUserProgressUpdateValue;
						}
					}
				}
			}
			catch (IOException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					"Invalid User Grade File", "An error has occurred. The users path is invalid and the record could not be updated. "
					+ "If the problem continues, contact the Administrator.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (ArgumentException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					"Invalid Characters Found", "An error has occurred. Invalid characters have been found in the users path. "
				+ "The record could not be updated. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (SecurityException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					"Access Denied", "An error has occurred. Access to the users path has been denied and the record could not be updated. "
				+ "If the problem persists, please contact the Administrator.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}

			#region SetProgressBar
			if (App.mWindow != null)
			{
				App.mWindow.Progress.Visibility = Visibility.Collapsed;
				App.mWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
			}
			#endregion
		}

		void courseContextMenu_Opened(object sender, RoutedEventArgs e)
		{
			ContextMenu menu = e.OriginalSource as ContextMenu;
			if (menu == null || menu.Tag == null)
				return;

			// Convert the tag to an array
			int[] tagArray = (int[])menu.Tag;

			int pindex = tagArray[0];
			int index = tagArray[1];
			TreeViewItem parentItem = records.Items[pindex] as TreeViewItem;

			if (parentItem == null)
				return;

			// Attempt to select the item
			foreach (var item in parentItem.Items)
			{
				var tItem = item as TreeViewItem;
				if (tItem != null && tItem.Tag.ToString() == index.ToString())
				{
					tItem.IsSelected = true;
					return;
				}
			}
		}

		void userContextMenu_Opened(object sender, RoutedEventArgs e)
		{
			ContextMenu menu = e.OriginalSource as ContextMenu;
			if (menu == null || menu.Tag == null)
				return;

			int index = int.Parse(menu.Tag.ToString());
			TreeViewItem tItem = records.Items[index] as TreeViewItem;
			if (tItem != null)
				tItem.IsSelected = true;
		}

		void timeMenuItem_Click(object sender, RoutedEventArgs e)
		{
			// Prompt the user for the new name
			var newDialog = new InputDialog("Modify Assignment - Record Pro", "Please enter the new time for this assignment:");
			if (App.mWindow != null)
				newDialog.Owner = App.mWindow;
			if (newDialog.ShowDialog() != true)
				return;
			else
			{
				string userInput = newDialog.userInput.Text.Replace("\\", "\\\\").Replace("\"", "\\\"");
				TimeSpan newTime;
				if (TimeSpan.TryParse(userInput, out newTime))
					Modify("Time", userInput);
				else
					NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero,
						"Invalid Data - Record Pro", "An invalid time has been entered.",
						"The assignment could not be modified.", NativeMethods.TaskDialogButtons.OK,
						NativeMethods.TaskDialogIcon.Warning);
			}
		}

		void notesMenuItem_Click(object sender, RoutedEventArgs e)
		{
			// Prompt the user for the new name
			var newDialog = new InputDialog("Modify Assignment - Record Pro", "Please enter the new notes for this assignment:");
			if (App.mWindow != null)
				newDialog.Owner = App.mWindow;
			if (newDialog.ShowDialog() != true)
				return;
			else
			{
				string userInput = newDialog.userInput.Text.Replace("\\", "\\\\").Replace("\"", "\\\"");
				Modify("Notes", userInput);
			}
		}

		void gradeMenuItem_Click(object sender, RoutedEventArgs e)
		{
			// Prompt the user for the new name
			var newDialog = new InputDialog("Modify Assignment - Record Pro", "Please enter the new numerical grade for this assignment:");
			if (App.mWindow != null)
				newDialog.Owner = App.mWindow;
			if (newDialog.ShowDialog() != true)
				return;
			else
			{
				string userInput = newDialog.userInput.Text.Replace("\\", "\\\\").Replace("\"", "\\\"");
				byte newGrade;
				if (string.IsNullOrWhiteSpace(userInput) || byte.TryParse(userInput, out newGrade) && newGrade <= 100)
					Modify("Grade", userInput);
				else
					NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero,
						"Invalid Data - Record Pro", "An invalid grade has been entered.",
						"The assignment could not be modified.", NativeMethods.TaskDialogButtons.OK,
						NativeMethods.TaskDialogIcon.Warning);
			}
		}

		void dataMenuItem_Click(object sender, RoutedEventArgs e)
		{
			// Prompt the user for the new name
			var newDialog = new InputDialog("Modify Assignment - Record Pro", "Please enter the new data for this assignment:");
			if (App.mWindow != null)
				newDialog.Owner = App.mWindow;
			if (newDialog.ShowDialog() != true)
				return;
			else
			{
				string userInput = newDialog.userInput.Text.Replace("\\", "\\\\").Replace("\"", "\\\"");
				Modify("Data", userInput);
			}
		}

		void courseMenuItem_Click(object sender, RoutedEventArgs e)
		{
			// Prompt the user for the new name
			var newDialog = new InputDialog("Modify Course - Record Pro", "Please enter the new course for this assignment:");
			if (App.mWindow != null)
				newDialog.Owner = App.mWindow;
			if (newDialog.ShowDialog() != true)
				return;
			else
			{
				string userInput = newDialog.userInput.Text.Replace("\\", "\\\\").Replace("\"", "\\\"");
				Modify("Course", userInput);
			}
		}

		private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true; // Enable commands
		}

		private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			// Copy the selected record to the clipboard
			if (e.Command == ApplicationCommands.Copy)
				Copy(e.Source);

			// Delete the selected record
			else if (e.Command == ApplicationCommands.Delete)
				Delete(e.Source);
		}

		/// <summary>
		/// Copies the selected record to the clipboard
		/// <param name="source">The source of the object to be copied</param>
		/// </summary>
		private void Copy(object source)
		{
			TreeViewItem selectedTreeViewItem = source as TreeViewItem;
			TextBlock selectedTextBlock = records.SelectedItem as TextBlock;

			try
			{

				// See if the data is selected
				if (selectedTextBlock != null)
				{
					Clipboard.SetText(selectedTextBlock.Text);
				}

				// If the data is not selected, copy the text from the TreeViewItem
				else if (selectedTreeViewItem != null)
				{
					Grid grid1 = selectedTreeViewItem.Header as Grid;
					if (grid1 != null)
					{
						Label lElement = grid1.Children[0] as Label;
						TextBlock tElement = grid1.Children[0] as TextBlock;
						if (lElement != null)
							Clipboard.SetText(lElement.Content.ToString());
						else if (tElement != null)
							Clipboard.SetText(tElement.Text);
					}
				}
			}
			catch (COMException)
			{
				MessageBox.Show("The data could not be copied. The clipboard is in use.",
					"Clipboard in use", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		/// <summary>
		/// Deletes the selected item
		/// </summary>
		/// <param name="source">The item to be deleted</param>
		private void Delete(object source)
		{
			#region Variables
			TreeViewItem selectedTreeViewItem = source as TreeViewItem; // The selected item
			TextBlock selectedTextBlock = source as TextBlock;
			DateTime? date = calendar.SelectedDate; // The date of the selected item
			var sList = new List<string>(); // Represents all lines that will be written to the file
			bool deleteUser = false; // Represents whether or not the entire user should be deleted
			string course = ""; // Represents the course for the selected item
			string data = ""; // Represents the data for the selected item
			string notes = ""; // Represents the notes for the selected item
			TimeSpan time = new TimeSpan(0); // Represents the time for the selected item
			byte grade = 0; // Represents the grade for the selected item
			bool validGrade = false; // True if the assignment has a grade
			string headerMessageText; // Represents the text to be displayed in the header of the warning message
			string path; // The file path for the assignment
			#endregion

			// If a text block is selected, get its parent
			if (selectedTextBlock != null)
				selectedTreeViewItem = selectedTextBlock.Parent as TreeViewItem;

			// Only continue if the selected item is not null
			if (selectedTreeViewItem == null)
				return;

			// Determine if a user or an individual assignment is selected and set the warning message
			if (selectedTreeViewItem.Parent.GetType() != typeof(TreeViewItem))
			{
				deleteUser = true;
				path = selectedTreeViewItem.Tag.ToString();
				headerMessageText = "Are you sure you want to delete these assignments?";
			}
			else
			{
				var parent = (TreeViewItem)selectedTreeViewItem.Parent;
				path = parent.Tag.ToString();
				headerMessageText = "Are you sure you want to delete this assignment?";
			}

			// Only continue if the user agrees to delete the files
			if (NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero,
				"Warning - Record Pro", headerMessageText, "This is permanent and cannot be undone.",
				NativeMethods.TaskDialogButtons.Yes | NativeMethods.TaskDialogButtons.No, NativeMethods.TaskDialogIcon.Warning)
				== NativeMethods.TaskDialogResult.No)
				return;

			Grid grid1 = selectedTreeViewItem.Header as Grid; // The header of the selected item
			TextBlock dataTextBlock = selectedTreeViewItem.Items[0] as TextBlock;

			// Get the course, time, and grade for the selected item
			if (!deleteUser && grid1 != null)
			{
				Label courseLabel = grid1.Children[0] as Label;
				TextBlock timeLabel = grid1.Children[1] as TextBlock;
				Label gradeLabel = grid1.Children[2] as Label;
				if (courseLabel != null)
					course = courseLabel.Content.ToString();
				if (timeLabel != null)
					TimeSpan.TryParse(timeLabel.Tag.ToString(), out time);
				if (gradeLabel != null)
					validGrade = byte.TryParse(gradeLabel.Content.ToString(), out grade);
			}

			// If there are notes, then get the data and notes for the selected item
			if (dataTextBlock != null && dataTextBlock.ToolTip != null)
			{
				notes = dataTextBlock.Tag.ToString();
				data = dataTextBlock.Text;
			}

			// If there are not notes, then only get the data for the selected item
			else if (dataTextBlock != null)
			{
				data = dataTextBlock.Text;
			}

			Stream newStream = null;
			try
			{
				// Read the file
				newStream = new FileStream(path, FileMode.Open);
				using (var newStreamReader = new StreamReader(newStream))
				{
					// Enumerate through each line
					while (newStreamReader.Peek() > -1)
					{
						string line = newStreamReader.ReadLine();
						string currentData = BasicFunctions.GetValue(line, "Data");
						string currentCourse = BasicFunctions.GetValue(line, "Course");
						TimeSpan currentTime;
						string currentNotes = BasicFunctions.GetValue(line, "Notes");
						DateTime currentDate;
						byte currentGrade;
						bool currentValidGrade;

						// Attempt to get the date, time, and grade of the current line
						DateTime.TryParse(BasicFunctions.GetValue(line, "Date"), out currentDate);
						currentValidGrade = byte.TryParse(BasicFunctions.GetValue(line, "Grade"), out currentGrade);
						TimeSpan.TryParse(BasicFunctions.GetValue(line, "Time"), out currentTime);

						// If the selected line is an entire user, delete all assignments for the selected date
						if (deleteUser & currentDate != date)
							sList.Add(line);

					   // If the selected item is not an entire user, only delete the assignment
						else if (!deleteUser & (currentData.Trim() != data.Trim() | currentCourse.Trim() != course.Trim() | currentTime != time
| currentNotes.Trim() != notes.Trim() | currentDate != date | currentValidGrade != validGrade | currentGrade != grade))
							sList.Add(line);
					}
				}

				// Now write to the file
				File.WriteAllLines(path, sList.ToArray());

			}
			catch (IOException)
			{
				// The main message to show
				string exceptionText = "The assignment could not be deleted.";
				if (deleteUser)
					exceptionText = "The assignments could not be deleted.";

				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					exceptionText, "If the problem continues, please contact the Administrator",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (ArgumentException)
			{
				// The main message to show
				string exceptionText = "The assignment could not be deleted.";
				if (deleteUser)
					exceptionText = "The assignments could not be deleted.";

				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					exceptionText, "Invalid characters were found in the file path, the file path is incorrect, "
				+ "or the file is in the wrong format. If the problem persists, please contact the administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (SecurityException)
			{
				// The main message to show
				string exceptionText = "The assignment could not be deleted.";
				if (deleteUser)
					exceptionText = "The assignments could not be deleted.";

				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					exceptionText, "Record Pro does not have the required permission. Please contact the administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (UnauthorizedAccessException)
			{
				// The main message to show
				string exceptionText = "The assignment could not be deleted.";
				if (deleteUser)
					exceptionText = "The assignments could not be deleted.";

				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					exceptionText, "The file or folder is not fully accessible. Please contact the administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (NotSupportedException)
			{
				// The main message to show
				string exceptionText = "The assignment could not be deleted.";
				if (deleteUser)
					exceptionText = "The assignments could not be deleted.";

				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					exceptionText, "The file path is incorrect. Please contact the administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			finally
			{
				if (newStream != null)
					newStream.Dispose();
			}

			// Now update the tree view
			if (deleteUser)
			{
				records.Items.Remove(selectedTreeViewItem);
			}
			else
			{
				TreeViewItem parent = selectedTreeViewItem.Parent as TreeViewItem;
				if (parent == null)
					return;

				// Remove the selected item if it is not a user
				parent.Items.Remove(selectedTreeViewItem);

				// Remove the user if no courses were completed
				if (parent.Items.Count == 0)
					records.Items.Remove(parent);
			}
		}

		/// <summary>
		/// Modifies the selected assignment
		/// </summary>
		/// <param name="propertyName">The name of the property that will be changed</param>
		/// <param name="newData">The new data for the property</param>
		private void Modify(string propertyName, string newData)
		{
			// Variables that will be used through this method
			#region Variables
			TreeViewItem selectedTreeViewItem = records.SelectedItem as TreeViewItem; // The selected treeview item
			TextBlock dataLabel = records.SelectedItem as TextBlock; // The textblock showing the data
			Label courseLabel = new Label(); // The label showing the course
			TextBlock timeLabel = new TextBlock(); // The textblock showing the time
			Label gradeLabel = new Label(); // The label showing the grade
			TextBlock notesLabel = new TextBlock(); // The textblock shown when hovering over the data
			DateTime? date = calendar.SelectedDate; // The date of the selected item
			var sList = new List<string>(); // Represents all lines that will be written to the file
			string course = ""; // Represents the course for the selected item
			string data = ""; // Represents the data for the selected item
			string notes = ""; // Represents the notes for the selected item
			TimeSpan time = new TimeSpan(0); // Represents the time for the selected item
			byte grade = 0; // Represents the grade for the selected item
			bool hasGrade = false; // True if a valid grade has been entered
			bool errorOccurred = false; // Represents whether the assignment was successfully modified
			string path; // Represents the file location where the assignment is kept
			#endregion

			// Acquire access to the assignment
			#region AcquireProperties
			if (dataLabel != null)
			{
				selectedTreeViewItem = dataLabel.Parent as TreeViewItem;
			}
			else if (selectedTreeViewItem != null)
			{
				dataLabel = selectedTreeViewItem.Items[0] as TextBlock;
			}
			else
				return;

			var parent = selectedTreeViewItem.Parent as TreeViewItem;
			if (parent == null)
				return;

			path = parent.Tag.ToString();

			// Get the controls and information for each item
			Grid grid1 = selectedTreeViewItem.Header as Grid; // The header of the course

			if (grid1 != null)
			{
				courseLabel = grid1.Children[0] as Label;
				timeLabel = grid1.Children[1] as TextBlock;
				if (grid1.Children.Count >= 3)
					gradeLabel = grid1.Children[2] as Label;
				if (courseLabel.Content != null)
					course = courseLabel.Content.ToString();
				if (timeLabel.Text != null)
					TimeSpan.TryParse(timeLabel.Tag.ToString(), out time);
				if (gradeLabel.Content != null)
					hasGrade = byte.TryParse(gradeLabel.Content.ToString(), out grade);
			}

			// If there are notes, then get the data and notes for the selected item.
			// Otherwise, only get the data for the selected item
			if (dataLabel != null && dataLabel.ToolTip != null)
			{
				ToolTip toolTip = (ToolTip)dataLabel.ToolTip;
				Grid toolTipGrid = toolTip.Content as Grid;
				if (toolTipGrid != null)
				{
					notesLabel = toolTipGrid.Children[1] as TextBlock;
					notes = notesLabel.Text;
				}
				data = dataLabel.Text;
			}
			else if (dataLabel != null)
			{
				data = dataLabel.Text;
			}
			#endregion

			// Save the record
			#region Save

			Stream newStream = null;
			try
			{
				// Ensure the course is valid
				string fileData = File.ReadAllText(path);
				string[] courseOptions = BasicFunctions.EnumerateStrings(fileData, "Courses");
				if (propertyName == "Course" & !courseOptions.Contains(newData, StringComparer.CurrentCulture))
				{
					NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero,
						"Invalid Course Name", "An invalid course name has been entered.",
						"Please enter a valid course name. If the specified course does not exist, add it using manage courses.",
						NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Warning);
					return;
				}

				// Read the record
				newStream = new FileStream(path, FileMode.Open);
				using (var newStreamReader = new StreamReader(newStream))
				{
					// Enumerate through each line
					while (newStreamReader.Peek() > -1)
					{
						#region Variables
						string line = newStreamReader.ReadLine();
						string currentData = BasicFunctions.GetValue(line, "Data");
						string currentCourse = BasicFunctions.GetValue(line, "Course");
						string currentNotes = BasicFunctions.GetValue(line, "Notes");
						TimeSpan currentTime;
						DateTime currentDate;
						byte currentGrade;
						bool hasCurrentGrade;
						#endregion

						// Attempt to get the date, time, and grade of the current line
						DateTime.TryParse(BasicFunctions.GetValue(line, "Date"), out currentDate);
						hasCurrentGrade = Byte.TryParse(BasicFunctions.GetValue(line, "Grade"), out currentGrade);
						TimeSpan.TryParse(BasicFunctions.GetValue(line, "Time"), out currentTime);

						// Replace the line if necessary
						if (currentData.Trim() == data.Trim() & currentCourse.Trim() == course.Trim()
							& currentTime == time & currentNotes.Trim() == notes.Trim()
							& currentDate == date & currentGrade == grade & hasCurrentGrade == hasGrade)
							line = BasicFunctions.ReplaceValue(line, propertyName, newData);

						// Now add the line
						sList.Add(line);
					}
				}

				// Write all the new data to the record
				File.WriteAllLines(path, sList.ToArray());
			}
			catch (IOException)
			{
				// The main message to show
				string exceptionText = "The assignment could not be modified.";
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					exceptionText, "If the problem continues, please contact the Administrator",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				errorOccurred = true;
			}
			catch (ArgumentException)
			{
				// The main message to show
				string exceptionText = "The assignment could not be modified.";
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					exceptionText, "Invalid characters were found in the file path, the file path is incorrect, "
				+ "or the file is in the wrong format. If the problem persists, please contact the administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				errorOccurred = true;
			}
			catch (SecurityException)
			{
				// The main message to show
				string exceptionText = "The assignment could not be modified.";
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					exceptionText, "Record Pro does not have the required permission. Please contact the administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				errorOccurred = true;
			}
			catch (UnauthorizedAccessException)
			{
				// The main message to show
				string exceptionText = "The assignment could not be modified.";
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					exceptionText, "The file or folder is not fully accessible. Please contact the administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				errorOccurred = true;
			}
			catch (NotSupportedException)
			{
				// The main message to show
				string exceptionText = "The assignment could not be modified.";

				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro",
					exceptionText, "The file path is incorrect. Please contact the administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				errorOccurred = true;
			}
			finally
			{
				if (newStream != null)
					newStream.Dispose();
			}
			#endregion

			// Update the user interface
			#region UpdateInterface

			// Only continue if no errors occurred
			if (errorOccurred)
				return;

			// Update the appropriate label or textblock
			if (propertyName == "Course" && courseLabel != null)
			{
				courseLabel.Content = newData.ConvertToQuotes();
			}
			else if (propertyName == "Data" && dataLabel != null)
			{
				dataLabel.Text = newData.ConvertToQuotes();
			}
			else if (propertyName == "Grade" && gradeLabel != null)
			{
				gradeLabel.Content = newData;
				byte newGrade;

				// Show the grade if it should be shown
				if (byte.TryParse(newData, out newGrade))
					gradeLabel.Visibility = Visibility.Visible;
				else
					gradeLabel.Visibility = Visibility.Collapsed;

				// Update the styles
				if (newGrade == 100)
					gradeLabel.SetResourceReference(Label.StyleProperty, "GradeStyle1");
				else if (newGrade >= 95)
					gradeLabel.SetResourceReference(Label.StyleProperty, "GradeStyle2");
				else if (newGrade >= 85)
					gradeLabel.SetResourceReference(Label.StyleProperty, "GradeStyle3");
				else if (newGrade >= 75)
					gradeLabel.SetResourceReference(Label.StyleProperty, "GradeStyle4");
				else
					gradeLabel.SetResourceReference(Label.StyleProperty, "GradeStyle5");
			}
			else if (propertyName == "Notes")
			{
				if (notesLabel != null && !string.IsNullOrWhiteSpace(notesLabel.Text) & !string.IsNullOrWhiteSpace(newData))
					notesLabel.Text = newData.ConvertToQuotes();
				else if (notesLabel != null & string.IsNullOrWhiteSpace(newData))
					dataLabel.ToolTip = null;
				else if (string.IsNullOrWhiteSpace(notesLabel.Text))
				{
					Grid toolTipGrid = new Grid();
					toolTipGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
					toolTipGrid.RowDefinitions.Add(new RowDefinition());
					Label tooltipHeaderLabel = new Label() { Content = "Notes" };
					tooltipHeaderLabel.SetResourceReference(Label.StyleProperty, "NotesHeaderStyle");
					TextBlock toolTipBody = new TextBlock() { Text = newData.ConvertToQuotes() };
					toolTipBody.SetResourceReference(TextBlock.StyleProperty, "NotesStyle");
					Grid.SetRow(toolTipBody, 1);
					toolTipGrid.Children.Add(tooltipHeaderLabel);
					toolTipGrid.Children.Add(toolTipBody);
					ToolTip newToolTip = new ToolTip() { Content = toolTipGrid };
					newToolTip.SetResourceReference(System.Windows.Controls.ToolTip.StyleProperty, "NotesToolTip");
					dataLabel.ToolTip = newToolTip;
				}
			}
			else if (propertyName == "Time" && timeLabel != null)
			{
				timeLabel.Text = BasicFunctions.GetTimeLabel(TimeSpan.Parse(newData));
				timeLabel.Tag = TimeSpan.Parse(newData);
			}

			// Sort all items
			parent.Items.SortDescriptions.Clear();
			if (parent.Items.CanSort)
				parent.Items.SortDescriptions.Add(new SortDescription("Header.Tag", ListSortDirection.Ascending));
			#endregion
		}

		private void records_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Delete)

				Delete(records.SelectedItem);
			else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control
				&& (e.Key == Key.C | e.SystemKey == Key.C))
				Copy(records.SelectedItem);
		}

		private void CalendarPopupOKButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				CalendarPopup.IsOpen = false;
				string configLocation = System.IO.Path.Combine((string)App.Current.Properties["Current User Location"], "config.txt");
				var data = File.ReadAllText(configLocation);
				File.WriteAllText(configLocation, BasicFunctions.ReplaceValue(data, "ShowCalendarPopup", "False"));
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

		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{
			disposed = true;
			newSempahore.Dispose();
		}


	}
}
