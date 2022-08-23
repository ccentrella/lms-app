namespace RecordPro
{
	using Record_Pro_Functions;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using System.Resources;
	using System.Security;
	using System.Windows;
	using System.Windows.Interop;
	using System.Windows.Shell;
	using System.Windows.Threading;
	using Microsoft.Win32;
	using System.Diagnostics;
	using System.Linq;
	using System.Windows.Markup;

	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		internal static MainWindow mWindow; // This will be used to refer to the main window.

		delegate void mainDelegate(); // Used for asynchronous programming

		private static string DefaultFileLocation = Path.Combine(Environment.GetFolderPath(
			Environment.SpecialFolder.CommonApplicationData), "Autosoft", "Record Pro", "2017");

		// This represents the registry location.
		public const string RegistryLocation = @"Software\Autosoft\Record Pro\2017";

		// This represents the full registry location.
		public const string FullRegistryLocation = @"HKEY_Current_User\Software\Autosoft\Record Pro\2017";


		[STAThread]
		public static void Main(string[] args)
		{
			bool mutexIsNew;
			using (var newMutex = new System.Threading.Mutex(true, "{E3A90280-B185-4917-812C-6354A2E28FC2}", out mutexIsNew))
			{
				// If the program is already open, get out!
				if (!mutexIsNew)
				{
					return;
				}
				var splashscreen = new SplashScreen("Splashscreen.png");
				splashscreen.Show(true);
				var app = new RecordPro.App();
				app.InitializeComponent();

				// Start the application
				app.Run();
			}
		}

		/// <summary>
		/// Changes the default theme
		/// </summary>
		/// <param name="newThemeName">The name of the new theme</param>
		private static void ChangeTheme(string newThemeName)
		{
			try
			{
				using (var registryKey = Registry.CurrentUser.CreateSubKey(RegistryLocation))
					registryKey.SetValue("Default Theme", newThemeName);
			}
			catch (IOException)
			{
				NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Theme Error",
					"The theme could not be saved. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				return;
			}
			catch (ArgumentException)
			{
				NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Theme Error",
					"The theme could not be saved. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				return;
			}
			catch (UnauthorizedAccessException)
			{
				NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Theme Error",
					"The theme could not be saved. Record Pro does not have the appropriate permission. Please contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				return;
			}
			catch (SecurityException)
			{
				NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Theme Error",
					"The theme could not be saved. Record Pro does not have the appropriate permission. Please contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				return;
			}

			// Notify the user that the change was successful
			MessageBox.Show("The theme had successfully been updated.", "Theme Update Successful",
				MessageBoxButton.OK, MessageBoxImage.Information);
		}

		/// <summary>
		/// Update the default theme
		/// </summary>
		private static void UpdateDefaultTheme()
		{
			string theme = "Crystal 2017 Modern";
			try
			{
				using (var registryKey = Registry.CurrentUser.CreateSubKey(RegistryLocation))
				{
					theme = (string)registryKey.GetValue("Default Theme", "Crystal 2017 Modern");
				}
			}
			catch (IOException)
			{
				NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Theme Error",
					"The theme could not be accessed. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (UnauthorizedAccessException)
			{
				NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Theme Error",
					"The theme could not be accessed. Record Pro does not have the appropriate permission. Please contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (SecurityException)
			{
				NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Theme Error",
					"The theme could not be accessed. Record Pro does not have the appropriate permission. Please contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}

			App.Current.Properties["Default Theme"] = theme;
		}

		/// <summary>
		/// Check the registry for any start arguments and execute them
		/// </summary>
		private static void CompleteRegistryArguments()
		{
			try
			{
				string data = Registry.GetValue(App.FullRegistryLocation, "StartArguments", "").ToString();
				if (data.Length >= 8 && data.Substring(0, 6) == "Delete")
				{
					var deletionLocation = data.Substring(7);
					Directory.Delete(deletionLocation, true);
				}
				else if (data.Length >= 8 && data.Substring(0, 6) == "Rename")
				{
					var commaStart = data.IndexOf(",");
					if (commaStart == -1 || commaStart + 1 >= data.Length)
						return;
					var oldLocation = data.Substring(7, commaStart - 7);
					var newLocation = data.Substring(commaStart + 1);
					if (oldLocation != newLocation)
						Directory.Move(oldLocation, newLocation);
				}
			}
			catch (IOException)
			{
				MessageBox.Show("A file system error has occurred. No accounts could be updated.",
					"File Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (UnauthorizedAccessException)
			{
				MessageBox.Show("A file system error has occurred. Access has been denied. No accounts could be updated.",
					"File Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (SecurityException)
			{
				MessageBox.Show("A file system error has occurred. The application does not have "
				+ "the appropriate permissions. No accounts could be updated.",
					"File Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			try
			{
				// Reset all start arguments
				Registry.SetValue(App.FullRegistryLocation, "StartArguments", "");
			}
			catch (UnauthorizedAccessException)
			{
				MessageBox.Show("A file system error has occurred. Access has been denied. The start arguments could not be reset.",
					"File Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (SecurityException)
			{
				MessageBox.Show("A file system error has occurred. The application does not have "
				+ "the appropriate permissions. The start arguments could not be reset.",
					"File Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>
		/// Changes the file location used for records
		/// </summary>
		/// <param name="newLocation">The new folder location</param>
		private static void ChangeFileLocation(string newLocation)
		{
			if (!Directory.Exists(newLocation))
			{
				MessageBox.Show("This location is inaccessible. Please choose a location that is accessible.");
				return;
			}

			// Retrieve the old location
			string fileLocation = (string)App.Current.Properties["File Location"];
			if (fileLocation == null)
				return;

			// Prepare the new location
			newLocation += @"\Autosoft\Record Pro";
			Directory.CreateDirectory(newLocation);
			newLocation += "\\2017";

			if (Directory.Exists(newLocation))
			{
				MessageBox.Show("The folder could not be moved. The destination source already contains a matching folder.",
									"Error - Record Pro", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			// Attempt to move all information to the new location
			try
			{
				Directory.Move(fileLocation, newLocation);
			}
			catch (IOException)
			{
				MessageBox.Show("The folder could not be moved. The operation could not be completed.",
					"Error - Record Pro", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			catch (UnauthorizedAccessException)
			{
				MessageBox.Show("The folder could not be moved. Record Pro does not have the appropriate permission. "
				+ "The operation could not be completed.", "Error - Record Pro", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			catch (ArgumentException)
			{
				MessageBox.Show("The folder could not be moved. The operation could not be completed.",
					"Error - Record Pro", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			// Now update the location in the registry
			try
			{
				using (var newKey = Registry.CurrentUser.CreateSubKey(RegistryLocation))
					newKey.SetValue("File Location", newLocation);
			}
			catch (IOException)
			{
				MessageBox.Show("The location could not be saved. The operation could not be completed.",
				"Error - Record Pro", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			catch (UnauthorizedAccessException)
			{
				MessageBox.Show("The location could not be saved. The operation could not be completed."
					+ "/n/nRecord pro does not have the appropriate permission.",
				"Error - Record Pro", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			catch (SecurityException)
			{
				MessageBox.Show("The location could not be saved. The operation could not be completed./n/n"
				 + "Record pro does not have the appropriate permission.",
				"Error - Record Pro", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			catch (ArgumentException)
			{
				MessageBox.Show("The location could not be saved. The operation could not be completed.",
							"Error - Record Pro", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			// Now update the program
			App.Current.Properties["File Location"] = newLocation;
			UpdateLocations();

			// Finally, notify the user
			MessageBox.Show("The file location was successfully updated.",
				"Update Successful", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		/// <summary>
		/// Removes the administrator tag from a user
		/// </summary>
		/// <param name="userName">The user name where this tag will be removed</param>
		/// <param name="userPassword">The password where this tag will be removed</param>
		private static void DisableAdministrator(string userName, string userPassword)
		{
			string usersLocation = App.Current.Properties["Users Location"].ToString();
			if (usersLocation == null)
				return;

			foreach (var folder in Directory.EnumerateDirectories(usersLocation))
			{
				var fileLocation = folder + "\\config.txt";
				string data;

				// Attempt to change the administrator tag
				try
				{
					data = File.ReadAllText(fileLocation);
					var dataUserName = BasicFunctions.GetValue(data, "UserName").ToUpperInvariant();
					var dataPassword = BasicFunctions.GetValue(data, "Password");
					if (dataUserName == userName.ToUpperInvariant() & dataPassword == userPassword)
					{
						File.WriteAllText(fileLocation, BasicFunctions.ReplaceValue(data, "IsAdministrator", "False"));
						MessageBox.Show("The administrator tag has been removed.", "Information - Record Pro", MessageBoxButton.OK, MessageBoxImage.Information);
						return;
					}
				}
				catch (IOException)
				{
					NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Invalid User Path",
						"An error has occurred. The users path is invalid. If the problem continues, contact the Administrator.",
						NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				}
				catch (ArgumentException)
				{
					NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
						"An error has occurred. Invalid characters have been found in the users path. If the problem continues, contact the Administrator.",
						NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				}
				catch (NotSupportedException)
				{
					NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
						"An error has occurred. The users path is not formatted correctly. If the problem continues, contact the Administrator.",
						NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				}
				catch (SecurityException)
				{
					NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Access Denied",
										"An error has occurred. Access to the users path has been denied. If the problem persists, please contact the Administrator.",
								NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				}
				catch (UnauthorizedAccessException)
				{
					NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Access Denied",
										"An error has occurred. Access to the users path has been denied. If the problem persists, please contact the Administrator.",
								NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				}
			}

			// If the code reaches this point, the tag could not be applied.
			MessageBox.Show("The administrator tag could not be modified.", "Warning - Record Pro", MessageBoxButton.OK, MessageBoxImage.Warning);

		}

		/// <summary>
		/// Adds the administrator tag to a user
		/// </summary>
		/// <param name="userName">The user name where this tag will be added</param>
		/// <param name="userPassword">The password where this tag will be added</param>		
		private static void EnableAdministrator(string userName, string userPassword)
		{
			string usersLocation = (string)App.Current.Properties["Users Location"];

			// Ensure the users location is not null
			if (usersLocation == null)
				return;

			foreach (var folder in Directory.EnumerateDirectories(usersLocation))
			{
				var fileLocation = folder + "\\config.txt";
				string data;

				// Attempt to change the administrator tag
				try
				{
					data = File.ReadAllText(fileLocation);
					var dataUserName = BasicFunctions.GetValue(data, "UserName").ToUpperInvariant();
					var dataPassword = BasicFunctions.GetValue(data, "Password");
					if (dataUserName == userName.ToUpperInvariant() & dataPassword == userPassword)
					{
						File.WriteAllText(fileLocation, BasicFunctions.ReplaceValue(data, "IsAdministrator", "True"));
						MessageBox.Show("The administrator tag has been added.", "Information - Record Pro", MessageBoxButton.OK, MessageBoxImage.Information);
						return;
					}
				}
				catch (IOException)
				{
					NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Invalid User Path",
						"An error has occurred. The users path is invalid. If the problem continues, contact the Administrator.",
						NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				}
				catch (ArgumentException)
				{
					NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
						"An error has occurred. Invalid characters have been found in the users path. If the problem continues, contact the Administrator.",
						NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				}
				catch (NotSupportedException)
				{
					NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
						"An error has occurred. The users path is not formatted correctly. If the problem continues, contact the Administrator.",
						NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				}
				catch (SecurityException)
				{
					NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Access Denied",
										"An error has occurred. Access to the users path has been denied. If the problem persists, please contact the Administrator.",
								NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				}
				catch (UnauthorizedAccessException)
				{
					NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Access Denied",
										"An error has occurred. Access to the users path has been denied. If the problem persists, please contact the Administrator.",
								NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				}
			}

			// If the code reaches this point, the tag could not be applied.
			MessageBox.Show("The administrator tag could not be modified.", "Warning - Record Pro", MessageBoxButton.OK, MessageBoxImage.Warning);

		}

		/// <summary>
		/// Updates locations based on the File Location.
		/// </summary>
		public static void UpdateLocations()
		{
			string fileLocation = (string)App.Current.Properties["File Location"];

			if (fileLocation == null)
			{
				fileLocation = DefaultFileLocation;
				App.Current.Properties["File Location"] = DefaultFileLocation;
			}

			var usersLocation = Path.Combine(fileLocation, "Users");
			var themeLocation = Path.Combine(fileLocation, "Themes");
			App.Current.Properties["Users Location"] = usersLocation;
			App.Current.Properties["Theme Location"] = themeLocation;

			// Create important folders
			Directory.CreateDirectory(fileLocation);
			Directory.CreateDirectory(usersLocation);
			Directory.CreateDirectory(themeLocation);
		}

		private void App_Started(object sender, StartupEventArgs e)
		{
			// Initialize all locations
			UpdateFileLocation();

			var args = Environment.GetCommandLineArgs().ToList();
			args.RemoveAt(0); // We don't want the file location of the program, which is the first argument

			// Check each argument
			args.ForEach((arg) =>
			{
				int i = args.IndexOf(arg);
				arg = arg.ToUpper();
				if (arg == "/NT")
					App.Current.Properties["Enable Theming"] = false;
				else if (arg == "/ENABLEADMIN" && args.Count > i + 2)
					EnableAdministrator(args[i + 1], args[i + 2]);
				else if (arg == "/DISABLEADMIN" && args.Count > i + 2)
					DisableAdministrator(args[i + 1], args[i + 2]);
				else if (arg == "/CHANGELOCATION" && args.Count > i + 1)
					ChangeFileLocation(args[i + 1]);
				else if (arg == "/CHANGETHEME" && args.Count > i + 1)
					ChangeTheme(args[i + 1]);
			});

			// Check the registry for arguments
			CompleteRegistryArguments();

			// Load the default theme
			UpdateDefaultTheme();

			ResetTheme();
		}

		/// <summary>
		/// Update the file location
		/// </summary>
		private static void UpdateFileLocation()
		{
			// Update the default file location if the directory exists.
			string savedFileLocation = "";
			string fileLocation = (string)App.Current.Properties["File Location"];

			// Ensure the file location is not null
			if (fileLocation == null)
				fileLocation = DefaultFileLocation;
			try
			{
				using (var registryKey = Registry.CurrentUser.CreateSubKey(RegistryLocation))
				{
					savedFileLocation = registryKey.GetValue("File Location", fileLocation).ToString();
				}
			}
			catch (IOException)
			{
				NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "File Location Error",
					"The file location could not be accessed. The default location will be used. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (UnauthorizedAccessException)
			{
				NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. Access to the file location has been denied. The default file location will be used. "
				+ "If the problem persists, please contact the Administrator.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (SecurityException)
			{
				NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. Access to the file location has been denied. The default file location will be used. "
				+ "If the problem persists, please contact the Administrator.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}

			// Update the file location
			if (Directory.Exists(savedFileLocation))
			{
				App.Current.Properties["File Location"] = savedFileLocation;
			}

			// Update all locations.
			UpdateLocations();
		}

		/// <summary>
		/// Loads the user's theme
		/// </summary>
		/// <param name="data">The data which contains the theme attribute</param>
		public async static void LoadTheme(string data)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			string theme = BasicFunctions.GetValue(data, "Theme");
			int oldThemeCount = App.Current.Resources.MergedDictionaries.Count;
			string themePath = (string)App.Current.Properties["Theme Location"];

			// Only continue if a valid theme has been entered and the theme location is not null
			if (string.IsNullOrWhiteSpace(theme) | themePath == null)
				return;

			string themeLocation = System.IO.Path.Combine(themePath, theme);
			string themeName = System.IO.Path.Combine(themeLocation);

			// Only continue if the selected theme has not been currently loaded
			string currentTheme = (string)App.Current.Properties["Current Theme"];
			if (currentTheme != null && currentTheme == themeName)
				return;

			// Only continue if themes are enabled
			if ((bool?)App.Current.Properties["Enable Theming"] == false)
				return;

			App.Current.Properties["Current Theme"] = themeName;
			Debug.WriteLine("It took {0} Ms for prep theme work", stopwatch.ElapsedMilliseconds);
			stopwatch.Restart();
			try
			{
				// Add all of the new themes, which will be placed at the end of the collection
				foreach (var themeFile in Directory.EnumerateFiles(themeLocation))
				{
					// Add the new theme
					await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new mainDelegate(() =>
						{
							var newDictionary = new ResourceDictionary() { Source = new Uri(themeFile, UriKind.Absolute) };
							App.Current.Resources.MergedDictionaries.Add(newDictionary);
						}));
				}
				Debug.WriteLine("It took {0} Ms to load the new theme", stopwatch.ElapsedMilliseconds);
				stopwatch.Restart();

				// Now remove all of the old themes
				for (int i = 0; i < oldThemeCount; i++)
				{
					await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new mainDelegate(() =>
						{
							App.Current.Resources.MergedDictionaries.RemoveAt(0);
						}));
				}
			}

			catch (IOException)
			{
				NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Invalid Theme Path",
					"An error has occurred. The theme could not be loaded. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (UriFormatException)
			{
				NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Invalid Theme Path",
					"An error has occurred. The theme path is not formatted correctly and could not be loaded. "
		+ "If the problem continues, contact the Administrator.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (ArgumentException)
			{
				NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
					"An error has occurred. Invalid characters have been found in the theme path. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (UnauthorizedAccessException)
			{
				NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. Access to the theme path has been denied. If the problem persists, please contact the Administrator.",
									NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (SecurityException)
			{
				NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. Access to the theme path has been denied. If the problem persists, please contact the Administrator.",
									NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			stopwatch.Stop();
			Debug.WriteLine("It took {0} Ms to remove the old theme", stopwatch.ElapsedMilliseconds);
		}

		/// <summary>
		/// Resets the theme
		/// </summary>
		public static void ResetTheme()
		{
			// Get the default theme
			string defaultTheme = App.Current.Properties["Default Theme"].ToString();

			// Ensure the theme is not null
			if (defaultTheme == null)
				defaultTheme = "Crystal 2017 Modern";

			// Reset the theme
			string resetTheme = string.Format("Theme = \"{0}\"", defaultTheme);
			LoadTheme(resetTheme);
		}

		private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			if ((e.Exception.Message == "Unable to find an entry point named 'TaskDialog' in DLL 'comctl32.dll'.") ||
				(e.Exception.InnerException != null &&
				e.Exception.InnerException.Message == "Unable to find an entry point named 'TaskDialog' in DLL 'comctl32.dll'."))
			{
				MessageBox.Show("TaskDialog could not be loaded. Please ensure that the program is properly installed.",
					"Warning - Record Pro", MessageBoxButton.OK, MessageBoxImage.Error);
				e.Handled = true;
			}
			else if (e.Exception.GetType() == typeof(XamlParseException))
			{
				MessageBox.Show("A theming error occurred. If this continues to happen, try a different theme."
					+ "\n\nIf this error occurs no matter which theme you are using, immediately contact Autosoft.",
					"Theming Error", MessageBoxButton.OK, MessageBoxImage.Warning);
				e.Handled = true;
			}
			else if (e.Exception.GetType() == typeof(TargetInvocationException))
			{
				e.Handled = true;
				var exceptionType = e.Exception.InnerException.GetType();
				if (exceptionType == typeof(ArgumentException))
					MessageBox.Show("An invalid search path has been entered. The courses could not be located." +
									"To correct this problem, change the search path under Account.",
									"Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
				else if (exceptionType == typeof(NotSupportedException))
					MessageBox.Show("An invalid search path has been entered. The courses could not be located." +
									"To correct this problem, change the search path under Account.",
									"Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
				else if (exceptionType == typeof(IOException))
					MessageBox.Show("The search path could not be located.",
								"Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
				else if (exceptionType == typeof(SecurityException))
					MessageBox.Show("The search location is inaccessible. The courses could not be located." +
								"If the problem persists, try running the program as an Administrator.", "Warning",
								MessageBoxButton.OK, MessageBoxImage.Warning);
				else if (exceptionType == typeof(UnauthorizedAccessException))
					MessageBox.Show("The search location is inaccessible. The courses could not be located." +
								"Access is denied.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
				else if (exceptionType == typeof(XamlParseException))
					MessageBox.Show("A theming error occurred. If this continues to happen, try a different theme."
						+ "\n\nIf this error occurs no matter which theme you are using, immediately contact Autosoft.",
						"Theming Error", MessageBoxButton.OK, MessageBoxImage.Warning);
				else
					Environment.FailFast("An unknown error has occurred.", e.Exception.InnerException);
			}
		}

		/// <summary>
		/// Logs on the specified user
		///	</summary>
		///	<param name="data">The data containing information for the user</param>
		///	<param name="folderLocation">The location of the user folder</param>
		public static void LogOn(string data, string folderLocation)
		{
			// Get the main window of the application
			MainWindow window = App.mWindow as MainWindow;
			if (window == null)
				return;

			#region Variables
			string userName = BasicFunctions.GetValue(data, "Name");
			string usersLocation = (string)App.Current.Properties["Users Location"];
			if (usersLocation == null)
				return;
			#endregion

			// Close the popup
			window.AvatarPopup.IsOpen = false;

			// Update user information
			Application.Current.Properties["Current User"] = userName;
			if (BasicFunctions.GetValue(data, "IsAdministrator") == "True")
			{
				Application.Current.Properties["IsAdministrator"] = true;
				Application.Current.Properties["Students"] = BasicFunctions.EnumerateStrings(data, "Students");
			}
			else
			{
				Application.Current.Properties["IsAdministrator"] = false;
			}
			string gradeLocation = System.IO.Path.Combine(folderLocation, "Grades");
			App.Current.Properties["Current User Location"] = folderLocation;
			App.Current.Properties["Grade File Location"] = gradeLocation;
			try
			{
				Directory.CreateDirectory(folderLocation);
				Directory.CreateDirectory(gradeLocation);
			}
			catch (IOException)
			{
				NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Invalid User Path",
					"An error has occurred. The user path is invalid. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (ArgumentException)
			{
				NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
					"An error has occurred. Invalid characters have been found in the user path. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (UnauthorizedAccessException)
			{
				NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. Access to the user path has been denied. If the problem persists, please contact the Administrator.",
									NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (NotSupportedException)
			{
				NativeMethods.TaskDialog(IntPtr.Zero, IntPtr.Zero, "Error - Record Pro", "Formatting Error",
									"An error has occurred. The user path is not formatted correctly. Please contact the Administrator.",
									NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}

			// Validate the user
			if (ValidateUser(folderLocation))
			{
				// Load the user window.
				var newHome = new Home();
				window.MainFrame.Navigate(newHome);
				App.LoadTheme(data); // Load the theme
			}
			else
			{
				var newRegistration = new ProductRegistration(data, folderLocation);
				window.MainFrame.Navigate(newRegistration);
			}
		}

		/// <summary>
		/// Logs out the specified user
		/// </summary>
		public static void Logout()
		{
			// Get the main window
			MainWindow window = App.mWindow as MainWindow;
			if (window == null)
				return;

			// Warn anyone attempting to update the record
			if (window.MainFrame.NavigationService.Content.GetType() == typeof(UpdateRecord) &&
				NativeMethods.TaskDialog(new WindowInteropHelper(window).Handle, IntPtr.Zero, "Warning - Record Pro",
					"If you go sign out, your changes will not be saved.", "Are you sure you would like to continue?",
					NativeMethods.TaskDialogButtons.Yes | NativeMethods.TaskDialogButtons.No,
					 NativeMethods.TaskDialogIcon.Warning) == NativeMethods.TaskDialogResult.No)
			{
				return;
			}

			window.AvatarPopup.IsOpen = false; // Close the small pane
			RecordProFunctions.LoadDefaultImage(Gender.Unknown);
			window.UserHeader.Content = "Sign In"; // Update text
			Application.Current.Properties["Current User"] = "None";
			Application.Current.Properties["IsAdministrator"] = false;
			Application.Current.Properties["Validated"] = false;

			// Navigate to the welcome screen.
			var newWelcome = new Welcome();
			window.MainFrame.Navigate(newWelcome);

			// Reset the theme
			App.ResetTheme();
		}

		/// <summary>
		/// Validates the user
		/// </summary>
		/// <param name="folderLocation">The location containing the user's files</param>
		/// <returns>Whether or not the user has paid for this product</returns>
		public static bool ValidateUser(string folderLocation)
		{
			string validationFile = Path.Combine(folderLocation, "Validation.txt");
			string folderName = System.IO.Path.GetFileName(folderLocation);
			char[] folderArray = folderName.ToCharArray();
			DateTime modificationDate;

			// If the validation file does not exist, then the user hasn't activated his account.
			if (!File.Exists(validationFile))
				return false;

			string data;
			try
			{
				using (var newReader = new StreamReader(validationFile))
					data = newReader.ReadToEnd();

				modificationDate = File.GetLastWriteTime(validationFile);
			}
			catch (IOException)
			{
				return false;
			}
			catch (ArgumentException)
			{
				return false;
			}
			catch (UnauthorizedAccessException)
			{
				return false;
			}
			catch (NotSupportedException)
			{
				return false;
			}

			string modificationDateString = modificationDate.ToString();
			string separator = modificationDateString[6].ToString() + modificationDateString[0].ToString()
				+ modificationDateString[5].ToString() + modificationDateString[4].ToString();
			string correctData = string.Join(separator, folderArray);

			// Validate the data
			if (data != correctData)
				return false;
			else
			{
				Application.Current.Properties["Validated"] = true;
				return true;
			}
		}
	}
}
