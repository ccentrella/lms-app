namespace RecordPro
{
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
	using System.IO;
	using System.Windows.Interop;
	using System.Security;
	using Record_Pro_Functions;
	using System.Windows.Threading;


	/// <summary>
	/// Interaction logic for ViewRecord.xaml
	/// </summary>
	public partial class ViewRecord : Page
	{
		delegate void mainDelegate();
		public ViewRecord()
		{
			InitializeComponent();
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			// Update the welcome text
			userLabel.Content = BasicFunctions.GetWelcomeMessage(App.Current.Properties["Current User"].ToString());
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
			if (BasicFunctions.GetValue(data, "ShowStatisticsPopup") != "False")
				StatisticsPopup.IsOpen = true;
			try
			{
				// Load all grades only if the grade folder exists.
				if (!Directory.Exists((string)App.Current.Properties["Grade File Location"]))
					return;

				foreach (var gradeFile in Directory.EnumerateFiles((string)App.Current.Properties["Grade File Location"]))
				{
					await Dispatcher.BeginInvoke(DispatcherPriority.Background,
						new mainDelegate(() => { LoadGrade(gradeFile, "Me"); })); // Load the current grade
				}
				if ((bool)Application.Current.Properties["IsAdministrator"])
				{
					string[] studentList = (string[])Application.Current.Properties["Students"];
					var sList = new List<string>(studentList);
					sList.Sort();
					foreach (var student in sList)
					{
						string configPath = System.IO.Path.Combine((string)App.Current.Properties["Users Location"],
							student, "config.txt");
						string path = System.IO.Path.Combine((string)App.Current.Properties["Users Location"], 
							student, "Grades");
						string studentData;
						using (var newReader = new StreamReader(configPath))
							studentData = await newReader.ReadToEndAsync();
						string name = BasicFunctions.GetValue(studentData, "Name");

						foreach (var sGradeFile in Directory.EnumerateFiles(path))
						{
							await Dispatcher.BeginInvoke(DispatcherPriority.Background,
								new mainDelegate(() => { LoadGrade(sGradeFile, name); })); // Load the current grade
						}
					}
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
			catch (SecurityException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. Access to the user path has been denied. If the problem persists, please contact the Administrator.",
							NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
		}

		/// <summary>
		/// Loads the results for the selected grade, including the user name in the header
		/// </summary>
		/// <param name="filePath">The grade file to load</param>
		/// <param name="userName">The user name to show in the tab header</param>
		private void LoadGrade(string filePath, string userName)
		{
			#region Variables
			int bestGrade = 0; // The average grade for the student's best course
			int worstGrade = 100; // The average grade for the student's worse course
			var bestCourses = new List<string>(); // The student's best courses
			var worstCourses = new List<string>(); // The student's worse courses
			int minimumCourses = 4; // The minimum amount of courses that can count as a day
			int extraCourses = 0; // This is used for partial days
			int days = 0; // The amount of days completed
			double progress = 0; // The percentage of how much the user has done for the year
			double roundedProgress; // The progress rounded to an integral value
			int requiredDays = 180; // The amount of days that must be completed
			StackPanel averagePanel = new StackPanel();
			int gradeCount = 0; // The total number of grades
			int timeCount = 0; // The total number of times
			double averageGrade = 0; // The average grade value
			double totalTimeValue = 0; // The total time of all assignments
			StackPanel courses = new StackPanel();
			ScrollViewer coursesScrollViewer = new ScrollViewer() { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
			Label avgGradeLabel = new Label();
			avgGradeLabel.SetResourceReference(Label.StyleProperty, "GenericGradeReportLabels");
			Label avgTimeLabel = new Label();
			avgTimeLabel.SetResourceReference(Label.StyleProperty, "GenericGradeReportLabels");
			Label avgDailyTimeLabel = new Label();
			avgDailyTimeLabel.SetResourceReference(Label.StyleProperty, "GenericGradeReportLabels");
			Label totalTimeLabel = new Label();
			totalTimeLabel.SetResourceReference(Label.StyleProperty, "GenericGradeReportLabels");
			ProgressBar newProgressBar = new ProgressBar() { Maximum = 1, Height = 20 };
			var courseList = new List<string>();
			var dateDictionary = new Dictionary<DateTime, List<string>>();
			var gradeDictionary = new Dictionary<string, List<byte>>();
			var timeDictionary = new Dictionary<string, List<TimeSpan>>();
			#endregion

			#region MainOperation

			// Parse all important information
			try
			{
				using (var newReader = new StreamReader(filePath))
				{
					while (newReader.Peek() > -1)
					{
						#region Variables
						var currentLine = newReader.ReadLine();
						string course = BasicFunctions.GetValue(currentLine, "Course");
						string timeString = BasicFunctions.GetValue(currentLine, "Time");
						string gradeString = BasicFunctions.GetValue(currentLine, "Grade");
						string DateString = BasicFunctions.GetValue(currentLine, "Date");
						TimeSpan time; // The current time
						byte grade; // The current grade
						DateTime date; // The current date
						#endregion

						// Attempt to convert all strings
						bool validTime = TimeSpan.TryParse(timeString, out time);
						bool validGrade = Byte.TryParse(gradeString, out grade);
						bool validDate = DateTime.TryParse(DateString, out date);

						// If any of the items is invalid, this line will be ignored
						if (string.IsNullOrWhiteSpace(course) | !validTime | (!validGrade & !String.IsNullOrWhiteSpace(gradeString)) | !validDate)
							continue;

						// If the time is more than 0 seconds, count it as valid.
						if (time.Ticks > 0)
						{
							// If the key already exists, add an additional time.
							if (timeDictionary.Keys.Contains(course))
							{
								for (int i = 0; i < timeDictionary.Count; i++)
								{
									if (timeDictionary.Keys.ElementAt(i) == course)
									{
										timeDictionary.Values.ElementAt(i).Add(time);
										break;
									}
								}
							}

							// If the key doesn't exist, create a new one.
							else
							{
								timeDictionary.Add(course, new List<TimeSpan>() { time });
							}
							timeCount++; // Updates the total number of times
							totalTimeValue += time.TotalSeconds; // Update the average time

						}

						// If the grade is not null, count it as valid.
						if (validGrade)
						{
							// If the key exists, add a new item.
							if (gradeDictionary.Keys.Contains(course))
							{
								for (int i = 0; i < gradeDictionary.Count; i++)
								{
									if (gradeDictionary.Keys.ElementAt(i) == course)
									{
										gradeDictionary.Values.ElementAt(i).Add(grade);
										break;
									}
								}
							}

							// If the key doesn't exist, create a new one
							else
							{
								gradeDictionary.Add(course, new List<byte>() { grade });
							}

							gradeCount++; // Update the total number of grades
							averageGrade += grade; // Update the average grade
						}

						// Add the item to the date list if the key already exists
						if (dateDictionary.Keys.Contains(date))
						{
							for (int i = 0; i < dateDictionary.Count; i++)
							{
								if (dateDictionary.Keys.ElementAt(i) == date)
								{
									dateDictionary.Values.ElementAt(i).Add(course);
									break;
								}
							}
						}

						// If the key doesn't exist, create a new one
						else
						{
							dateDictionary.Add(date, new List<string>() { course });
						}

						// Now add the course if necessary
						if (!courseList.Contains(course))
							courseList.Add(course);
					}
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
			catch (NotSupportedException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
					"An error has occurred. Invalid characters have been found in the user path. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			#endregion

			#region CalculateCurrentCourse
			courseList.Sort(); // Sort the list of items
			foreach (var item in courseList)
			{
				#region UIElements
				TextBlock textBlock1 = new TextBlock() { Text = item };
				StackPanel averageCoursePanel = new StackPanel();
				textBlock1.SetResourceReference(TextBlock.StyleProperty, "CourseReportLabels");
				Expander currentExpander = new Expander() { Header = textBlock1 };
				currentExpander.SetResourceReference(Expander.StyleProperty, "CourseReportExpanders");
				Label currentAverageGradeLabel = new Label();
				currentAverageGradeLabel.SetResourceReference(Label.StyleProperty, "GenericGradeReportLabels");
				Label currentAverageTimeLabel = new Label();
				currentAverageTimeLabel.SetResourceReference(Label.StyleProperty, "GenericGradeReportLabels");
				Label currentTotalTimeLabel = new Label();
				currentTotalTimeLabel.SetResourceReference(Label.StyleProperty, "GenericGradeReportLabels");
				#endregion
				long currentAverageGrade = 0; // The average grade of the course
				double currentTotalTime = 0; // The total time spent on this course
				double currentAverageSeconds = 0; // The average time of the course in seconds
				int currentGradeCount = 0; // The count of all grades for the current course

				// Calculate the average grade
				for (int i = 0; i < gradeDictionary.Count; i++)
				{
					if (gradeDictionary.Keys.ElementAt(i) == item)
					{
						int totalGrade = 0;
						gradeDictionary.Values.ElementAt(i).ForEach((value) =>
						{
							totalGrade += value;
							currentGradeCount++;
						});
						if (currentGradeCount > 0)
						{
							currentAverageGrade = totalGrade / currentGradeCount;
							int roundedGrade = (int)Math.Round((double)currentAverageGrade, MidpointRounding.AwayFromZero);

							// If the grade is much better than the best grade, clear the list and add a new item. 
							// If the grade is similar to the best grade, add a new item.
							if (roundedGrade > bestGrade & Math.Abs(roundedGrade - bestGrade) > 1)
							{
								bestCourses.Clear();
								bestGrade = roundedGrade;
								bestCourses.Add(item);

							}
							else if (Math.Abs(roundedGrade - bestGrade) <= 1)
							{
								bestCourses.Add(item);
							}

							// If the grade is much worse than the worst grade, clear the list and add a new item.
							// If the grade is similar to the worst grade, add an item.
							if (roundedGrade < worstGrade & Math.Abs(roundedGrade - worstGrade) > 1)
							{
								worstCourses.Clear();
								worstGrade = roundedGrade;
								worstCourses.Add(item);
							}
							else if (Math.Abs(roundedGrade - worstGrade) <= 1)
							{
								worstCourses.Add(item);
							}
						}
						break;
					}
				}

				// Calculate the average time
				for (int i = 0; i < timeDictionary.Count; i++)
				{
					if (timeDictionary.Keys.ElementAt(i) == item)
					{
						double totalTime = 0;
						int currentTimeCount = 0;
						timeDictionary.Values.ElementAt(i).ForEach((value) =>
						{
							totalTime += value.TotalSeconds;
							currentTimeCount++;
						});
						if (currentTimeCount > 0)
						{
							currentAverageSeconds = totalTime / currentTimeCount;
							currentTotalTime = totalTime; // The total time spent on this course
						}
						break;
					}
				}

				// Update the interface
				currentAverageGradeLabel.Content = String.Format("Average Grade: {0}", Math.Round((decimal)currentAverageGrade));
				if (currentGradeCount > 0)
				{
					if (currentAverageGrade == 100)
						currentAverageGradeLabel.SetResourceReference(Label.StyleProperty, "GradeReportLabels1");
					else if (currentAverageGrade >= 95)
						currentAverageGradeLabel.SetResourceReference(Label.StyleProperty, "GradeReportLabels2");
					else if (currentAverageGrade >= 85)
						currentAverageGradeLabel.SetResourceReference(Label.StyleProperty, "GradeReportLabels3");
					else if (currentAverageGrade >= 75)
						currentAverageGradeLabel.SetResourceReference(Label.StyleProperty, "GradeReportLabels4");
					else if (currentAverageGrade >= 0)
						currentAverageGradeLabel.SetResourceReference(Label.StyleProperty, "GradeReportLabels5");
				}
				else
				{
					currentAverageGradeLabel.Content = "Average Grade: None";
				}

				// Show how much time the user spent on this course
				if (currentAverageSeconds > 0)
				{
					currentTotalTimeLabel.Content = String.Format("Total Time: {0}", BasicFunctions.GetTimeLabel(currentTotalTime));
					currentAverageTimeLabel.Content = String.Format("Average Assignment Time: {0}", BasicFunctions.GetTimeLabel(currentAverageSeconds));
				}
				else
				{
					currentTotalTimeLabel.Content = "Total Time: None";
					currentAverageTimeLabel.Content = "Average Assignment Time: None";
				}

				averageCoursePanel.Children.Add(currentAverageGradeLabel);
				averageCoursePanel.Children.Add(currentAverageTimeLabel);
				averageCoursePanel.Children.Add(currentTotalTimeLabel);
				currentExpander.Content = averageCoursePanel;
				courses.Children.Add(currentExpander);
			}
			coursesScrollViewer.Content = courses;
			#endregion

			foreach (var item in dateDictionary.Values)
			{
				if (item.Count >= minimumCourses)
					days++;
				else
					extraCourses += item.Count; // Allow for partial days
			}
			days += extraCourses / minimumCourses; // Add the extra days to the amount
			progress = (double)days / requiredDays;
			roundedProgress = Math.Round((double)days / requiredDays * 100);
			newProgressBar.Value = progress;
			newProgressBar.ToolTip = String.Format("{0}% completed", roundedProgress);

			if (gradeDictionary.Count > 0)
			{
				averageGrade /= gradeCount; // Calculate the average grade
				avgGradeLabel.Content = String.Format("Average Grade: {0}", Math.Round(averageGrade));
				if (averageGrade == 100)
					avgGradeLabel.SetResourceReference(Label.StyleProperty, "GradeReportLabels1");
				else if (averageGrade >= 95)
					avgGradeLabel.SetResourceReference(Label.StyleProperty, "GradeReportLabels2");
				else if (averageGrade >= 85)
					avgGradeLabel.SetResourceReference(Label.StyleProperty, "GradeReportLabels3");
				else if (averageGrade >= 75)
					avgGradeLabel.SetResourceReference(Label.StyleProperty, "GradeReportLabels4");
				else if (averageGrade >= 0)
					avgGradeLabel.SetResourceReference(Label.StyleProperty, "GradeReportLabels5");
			}
			else
			{
				avgGradeLabel.Content = "Average Grade: None";
			}

			// Show the student's best course
			var bestCourseLabel = new Label() { Content = "My Favorite Course: None" };
			bestCourseLabel.SetResourceReference(Label.StyleProperty, "GenericGradeReportLabels");
			if (bestCourses.Count == 1 & bestGrade - worstGrade > 3 & courseList.Count > 1)
			{
				bestCourseLabel.Content = string.Format("My Favorite Course: {0}", bestCourses[0]);
			}
			else if (bestCourses.Count > 1 & bestGrade - worstGrade > 3 & courseList.Count > 1)
			{
				bestCourses.Sort();
				bestCourseLabel.Content = string.Format("My Favorite Courses: {0}", bestCourses.ToArray().GetFriendlyText());
			}

			// Show the student's worst course
			var worstCourseLabel = new Label() { Content = "My Worst Course: None" };
			worstCourseLabel.SetResourceReference(Label.StyleProperty, "GenericGradeReportLabels");
			if (worstCourses.Count == 1 & bestGrade - worstGrade > 3 & courseList.Count > 1)
			{
				worstCourseLabel.Content = string.Format("My Worst Course: {0}", worstCourses[0]);
			}
			else if (worstCourses.Count > 1 & bestGrade - worstGrade > 3 & courseList.Count > 1)
			{
				worstCourses.Sort();
				worstCourseLabel.Content = string.Format("My Worst Courses: {0}", worstCourses.ToArray().GetFriendlyText());
			}

			// Add these new labels
			averagePanel.Children.Add(bestCourseLabel);
			averagePanel.Children.Add(worstCourseLabel);

			// Show how many hours the student completed
			if (timeDictionary.Count > 0)
			{
				// Show the total time
				totalTimeLabel.Content = String.Format("Total Time: {0}", BasicFunctions.GetTimeLabel(totalTimeValue));

				// Show the average assignment time
				avgTimeLabel.Content = String.Format("Average Assignment Time: {0}",
					BasicFunctions.GetTimeLabel(totalTimeValue / timeCount));

				// Show the average daily time
				avgDailyTimeLabel.Content = String.Format("Average Daily Time: {0}",
					BasicFunctions.GetTimeLabel(totalTimeValue / days));

			}
			else
			{
				avgTimeLabel.Content = "Average Assignment Time: None";
				avgDailyTimeLabel.Content = "Average Daily Time: None";
				totalTimeLabel.Content = "Total Time: None";
			}

			// Add the grade
			averagePanel.Children.Add(avgGradeLabel);
			averagePanel.Children.Add(avgTimeLabel);
			averagePanel.Children.Add(avgDailyTimeLabel);
			averagePanel.Children.Add(totalTimeLabel);
			averagePanel.Children.Insert(0, newProgressBar);
			TextBlock statisticsHeader = new TextBlock() { Text = "Average Statistics" };
			statisticsHeader.SetResourceReference(TextBlock.StyleProperty, "CourseReportLabels");
			Expander newExpander = new Expander() { Header = statisticsHeader, IsExpanded = true };
			newExpander.Content = averagePanel;
			newExpander.SetResourceReference(Expander.StyleProperty, "CourseReportExpanders");
			courses.Children.Insert(0, newExpander);
			string headerContent = String.Format("{0} ({1})", userName, System.IO.Path.GetFileNameWithoutExtension(filePath));
			TabItem newPage = new TabItem() { Content = coursesScrollViewer, Header = headerContent };
			Grades.Items.Add(newPage); // Add the item to the tab control

		}

		private void userLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (App.mWindow != null)
				App.mWindow.DragMove(); // Move the window
		}

		private void StatisticsPopupOKButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				StatisticsPopup.IsOpen = false;
				string configLocation = System.IO.Path.Combine((string)App.Current.Properties["Current User Location"], "config.txt");
				var data = File.ReadAllText(configLocation);
				File.WriteAllText(configLocation, BasicFunctions.ReplaceValue(data, "ShowStatisticsPopup", "False"));
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
	}
}
