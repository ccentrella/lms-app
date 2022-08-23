using Record_Pro_Functions;
using System;
using System.IO;
using System.Security;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using IO = System.IO;
using System.Runtime;
namespace RecordPro
{
	/// <summary>
	/// Interaction logic for SettingsDialog.xaml
	/// </summary>
	public partial class SettingsDialog : Page
	{
		delegate void mainDelegate(); // Used for asynchronous operations
		OpenFileDialog openFileDialog1 = new OpenFileDialog()
		{
			Title = "Upload Image - Record Pro",
			ValidateNames = true,
			CheckPathExists = true,
			Filter = "Image Files | *.png; *.ico; *.jpg; *.jpeg; *.tiff; *.gif; *.bmp; *.wmf | All Files | *.*"
		};

		/// <summary>
		/// The location where the configuration file for the current user is stored
		/// </summary>
		string configLocation = System.IO.Path.Combine((string)App.Current.Properties["Current User Location"], "config.txt");

		public SettingsDialog()
		{
			InitializeComponent();
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			string data = "";
			string dateOfBirthStr;
			string genderStr;
			Gender gender;
			DateTime dateOfBirth;
			string themeName;
			string newThemeLocation;

			try
			{
				using (StreamReader newReader = new StreamReader(configLocation))
				{
					data = await newReader.ReadToEndAsync();
				}

				// Attempt to load all themes
				themeName = BasicFunctions.GetValue(data, "Theme");
				string themeLocation =(string) App.Current.Properties["Theme Location"];
				newThemeLocation = System.IO.Path.Combine(themeLocation, themeName);
				foreach (var theme in Directory.EnumerateDirectories(themeLocation))
				{
					// Add the new theme
					var newInfo = new DirectoryInfo(theme);
					var newItem = new ComboBoxItem() { Content = newInfo.Name };
					ThemeComboBox.Items.Add(newItem);

					// Select the correct theme
					if (newInfo.Name == themeName)
						ThemeComboBox.SelectedItem = newItem;
				}
			}
			catch (IOException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid User Path",
					"An error has occurred. The settings path is invalid. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (ArgumentException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
					"An error has occurred. Invalid characters have been found in the settings path. If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (UnauthorizedAccessException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. Access to the settings path is denied. Some settings may have been loaded incorrectly. If the problem persists, please contact the Administrator.",
									NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (SecurityException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. Access to the user path has been denied. Some settings may have been loaded incorrectly. If the problem persists, please contact the Administrator.",
							NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}

			// Update all settings
			NameTextBox.Text = App.Current.Properties["Current User"].ToString();
			UserNameTextBox.Text = BasicFunctions.GetValue(data, "UserName");
			PasswordTextBox.Password = BasicFunctions.GetValue(data, "Password");
			confirmPasswordTextBox.Password = BasicFunctions.GetValue(data, "Password");
			ImageLabel.Content = BasicFunctions.GetValue(data, "Image");
			dateOfBirthStr = BasicFunctions.GetValue(data, "BirthDate");
			genderStr = BasicFunctions.GetValue(data, "Gender");

			// Attempt to show the date of birth
			if (DateTime.TryParse(dateOfBirthStr, out dateOfBirth))
				BirthDate.SelectedDate = dateOfBirth.Date;
			else
				BirthDate.SelectedDate = DateTime.Today;

			// Attempt to show the gender
			if (Enum.TryParse<Gender>(genderStr, out gender))
				GenderCombobox.Text = genderStr;

			// Update check-boxes
			if (BasicFunctions.GetValue(data, "ExpandUser") == "True")
				ExpandUser.IsChecked = true;
			if (BasicFunctions.GetValue(data, "ExpandCourses") == "True")
				ExpandCourses.IsChecked = true;
			if (BasicFunctions.GetValue(data, "ShowCalendar") == "True")
				showCalendarCheckBox.IsChecked = true;
			if (BasicFunctions.GetValue(data, "ShowCalculator") == "True")
				showCalculatorCheckBox.IsChecked = true;
			if (BasicFunctions.GetValue(data, "ViewRecordEnabled") == "True")
				showViewRecordButton.IsChecked = true;
		}

		// Save all settings when the OK button is clicked.
		private async void OKButton_Click(object sender, RoutedEventArgs e)
		{
			#region Verification
			// Ensure the user entered valid information
			if (PasswordTextBox.Password != confirmPasswordTextBox.Password)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle,
					IntPtr.Zero, "Incorrect Passwords", "The passwords do not match.", "Please retype the password.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Warning);
				PasswordTextBox.Clear();
				confirmPasswordTextBox.Clear();
				return;
			}

			if (string.IsNullOrWhiteSpace(UserNameTextBox.Text))
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle,
								IntPtr.Zero, "Incorrect User Name", "The user name is invalid.", "Please insert a valid user name.",
								NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Warning);
				UserNameTextBox.Clear();
				return;
			}

			if (string.IsNullOrWhiteSpace(NameTextBox.Text))
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle,
								IntPtr.Zero, "Incorrect Name", "The name is invalid.", "Please insert a valid name.",
								NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Warning);
				NameTextBox.Clear();
				return;
			}
			if (GenderCombobox.SelectedIndex == 0)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle,
								IntPtr.Zero, "No Gender Selected", "No gender has been selected.", "Please select the gender.",
								NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Warning);
				return;

			}

			if (BirthDate.SelectedDate == null)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle,
								IntPtr.Zero, "No Birth Date", "No birth date has been entered.", "Please select a valid birth date.",
								NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Warning);
				return;

			}
			// Ensure the user did not enter an invalid name
			foreach (var invalidChar in System.IO.Path.GetInvalidFileNameChars())
			{
				if (NameTextBox.Text.Contains(invalidChar.ToString()))
				{
					NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
					"The name contains invalid characters. Please enter a new name.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
					NameTextBox.Clear();
					return;
				}
			}

			// Ensure no other user exists with the same name
			string newUserDirectory = Path.Combine((string)App.Current.Properties["Users Location"], NameTextBox.Text);
			if (newUserDirectory != (string)App.Current.Properties["Current User Location"] && IO.Directory.Exists(newUserDirectory))
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero,
					"User Already Exists", "A user already exists with the specified name.",
					"Please enter a different name. It is recommended to include your full name.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				NameTextBox.Clear();
				return;
			}

			// Ensure no other user exists with the same user name
			try
			{
				foreach (var directory in IO.Directory.EnumerateDirectories((string)App.Current.Properties["Users Location"]))
				{
					var configFile = IO.Path.Combine(directory, "config.txt");

					// Its okay for the user to leave the old user name
					if (directory == (string)App.Current.Properties["Current User Location"])
						continue;

					string configData = IO.File.ReadAllText(configFile);
					if (BasicFunctions.GetValue(configData, "UserName") == UserNameTextBox.Text)
					{

						NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero,
							"User Name Already Exists", "The specified user name already exists.",
							"Please enter a different name. If you are having trouble, contact Autosoft.",
							NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
						return;
					}
				}
			}
			catch (IO.IOException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "File Error",
					"An error has occurred The user name could not be verified. The user could not be saved.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				return;
			}
			catch (ArgumentException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
					"An error has occurred. The user name could not be verified. "
				+ "Invalid characters have been found in the users path. Please contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				return;
			}
			catch (UnauthorizedAccessException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle,
					IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. The user name could not be verified. "
				+ "Access to the users path has been denied. If the problem persists, please contact the Administrator. "
								+ "The settings could not be saved.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				return;
			}
			catch (SecurityException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. The user name could not be verified. "
				+ "Access to the users path has been denied. If the problem persists, please contact the Administrator. "
								+ "The settings could not be saved.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				return;
			}
			catch (NotSupportedException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Formatting Error",
									"An error has occurred. The user name could not be verified. " +
									"The users path is not formatted correctly or an invalid name has been entered. The user could not be saved. "
					+ "If the problem persists, please contact the administrator.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				return;
			}
			#endregion

			#region Prep
			// Update the user location
			string imageLocation = Path.Combine((string)App.Current.Properties["Current User Location"], ImageLabel.Content.ToString());
			string data = " ";

			try
			{
				using (var newReader = new StreamReader(configLocation))
				{
					data = newReader.ReadToEnd();
				}
			}
			catch (IOException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "File Error",
					"An error has occurred. The settings could not be saved.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (ArgumentException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
					"An error has occurred. Invalid characters have been found in the settings path. The settings could not be saved. "
				+ "If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			#endregion

			#region CreateData
			data = data.ReplaceValue("Name", NameTextBox.Text);
			data = data.ReplaceValue("UserName", UserNameTextBox.Text);
			data = data.ReplaceValue("Password", PasswordTextBox.Password);
			data = data.ReplaceValue("BirthDate", BirthDate.SelectedDate.ToString());
			data = data.ReplaceValue("Gender", GenderCombobox.Text);
			data = data.ReplaceValue("ShowCalendar", showCalendarCheckBox.IsChecked.Value.ToString());
			data = data.ReplaceValue("ShowCalculator", showCalculatorCheckBox.IsChecked.Value.ToString());
			data = data.ReplaceValue("ViewRecordEnabled", showViewRecordButton.IsChecked.Value.ToString());
			data = data.ReplaceValue("ExpandUser", ExpandUser.IsChecked.Value.ToString());
			data = data.ReplaceValue("ExpandCourses", ExpandCourses.IsChecked.Value.ToString());
			data = data.ReplaceValue("Image", ImageLabel.Content.ToString());
			if (ThemeComboBox.SelectedIndex > -1)
				data = data.ReplaceValue("Theme", ThemeComboBox.Text);
			#endregion

			#region SaveData
			try
			{
				// Write to the configuration file
				using (var newWriter = new StreamWriter(configLocation))
				{
					await newWriter.WriteAsync(data);
				}

			}
			catch (IOException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "File Error",
					"An error has occurred. The settings could not be saved.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (ArgumentException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Invalid Characters Found",
					"An error has occurred. Invalid characters have been found in the settings path. The settings could not be saved. "
				+ "If the problem continues, contact the Administrator.",
					NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (UnauthorizedAccessException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. Access to the user path has been denied. If the problem persists, please contact the Administrator. "
								+ "The settings could not be saved.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (SecurityException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. Access to the user path has been denied. If the problem persists, please contact the Administrator. "
								+ "The settings could not be saved.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (NotSupportedException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Formatting Error",
									"An error has occurred. The settings path is not formatted correctly. The settings could not be saved. Please contact the Administrator.",
									NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}

			// Now save the name
			App.Current.Properties["Current User"] = NameTextBox.Text;
			#endregion

			#region UpdateInterface
			try
			{
				// Load the image
				if (App.mWindow != null && File.Exists(imageLocation))
				{
					var newImage = new BitmapImage();
					newImage.BeginInit();
					newImage.UriSource = new Uri(imageLocation, UriKind.Absolute);
					newImage.DecodePixelWidth = 40;
					newImage.EndInit();
					App.mWindow.Avatar.Source = newImage;
				}
				else
					LoadDefaultImage(); // Load the default image
			}
			catch (FileNotFoundException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "The image could not be updated.",
				"An error has occurred. The image could not be found. Please contact the Administrator.",
				NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);

				LoadDefaultImage(); // Load the default image
			}
			catch (UriFormatException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "The image could not be updated.",
									"An error has occurred. The image could not be loaded. Please contact the Administrator.",
									NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				LoadDefaultImage(); // Load the default image
			}
			catch (UnauthorizedAccessException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Image Error - Record Pro", "The image could not be updated.",
					"An error has occurred. Access to the image location is denied. The image could not be update. If the problem continues, please contact an administrator.",
				NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				LoadDefaultImage(); // Load the default image
			}

			GoHome(); // Navigate to the home page		
			App.LoadTheme(data); // Load the theme
			#endregion
		}

		/// <summary>
		/// Loads the default image
		/// </summary>
		private void LoadDefaultImage()
		{
			string Url; // The URL of the image to display.
			if (GenderCombobox.Text == "Male")
				Url = "Generic Avatar (Male).png";
			else if (GenderCombobox.Text == "Female")
				Url = "Generic Avatar (Female).png";
			else
				Url = "Generic Avatar (Unisex).png";
			try
			{
				// Update the window
				if (App.mWindow != null)
				{
					var newImage = new BitmapImage();
					newImage.BeginInit();
					newImage.UriSource = new Uri(Url, UriKind.Relative);
					newImage.DecodePixelWidth = 40;
					newImage.EndInit();
					App.mWindow.Avatar.Source = newImage;
				}
			}
			catch (FileNotFoundException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "The default image could not be loaded.",
						"An error has occurred. The default image could not be loaded. If the problem continues, please contact the Administrator.",
						NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (UriFormatException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "The default image could not be loaded.",
						"An error has occurred. The default image could not be loaded. If the problem continues, please contact the Administrator.",
						NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			GoHome(); // Navigate to the home page
		}

		/// <summary>
		/// Navigates to the home page
		/// </summary>
		private void GoHome()
		{
			var newHome = new Home();
			this.NavigationService.Navigate(newHome);
		}

		private void ChangeImage_Click(object sender, RoutedEventArgs e)
		{
			// Prompt the user to enter a file name and update the image if it exists
			if (openFileDialog1.ShowDialog() == true)
			{
				string fileName = Path.GetFileName(openFileDialog1.FileName);
				string newImage = Path.Combine((string)App.Current.Properties["Current User Location"], fileName);
				string Extension = Path.GetExtension(fileName);
				try
				{
					if (fileName == ImageLabel.Content.ToString())
					{
						fileName = Path.GetFileNameWithoutExtension(fileName) + " (2)" + Extension;
						newImage = Path.Combine((string)App.Current.Properties["Current User Location"], fileName);
					}

					// Upload the image, unless it already exists in the same location
					if (openFileDialog1.FileName != newImage)
						File.Copy(openFileDialog1.FileName, newImage);

					// Update the image label
					ImageLabel.Content = fileName;
				}
				catch (IOException)
				{
					NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "File Error",
						"An error has occurred. The image could not be uploaded.",
						NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				}

				catch (UnauthorizedAccessException)
				{
					NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Access Denied",
										"An error has occurred. Access to the user path has been denied. The image could not be updated. "
					+ "If the problem persists, please contact the Administrator. "
									+ "The settings could not be saved.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				}

				catch (NotSupportedException)
				{
					NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Formatting Error",
										"An error has occurred. The user path is not formatted correctly. The image could not be updated. Please contact the Administrator.",
										NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
				}
			}
		}

		private void ClearImage_Click(object sender, RoutedEventArgs e)
		{
			ImageLabel.Content = "";
		}

		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			// Warn the user before continuing
			if (NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Delete Account?",
				"Are you sure you want to delete your account?", "All of your records will be permanently deleted.",
				NativeMethods.TaskDialogButtons.Yes | NativeMethods.TaskDialogButtons.No, NativeMethods.TaskDialogIcon.Warning) != NativeMethods.TaskDialogResult.Yes)
				return;

			if (MessageBox.Show("The application will now shut down. When the application re-opens, your account will be deleted.",
				"Update Name", MessageBoxButton.OKCancel, MessageBoxImage.Information) != MessageBoxResult.OK)
				return;

			// Close the program
			try
			{
				App.Current.Shutdown();
				Registry.SetValue(App.FullRegistryLocation, "StartArguments",
					"Delete " + (string)App.Current.Properties["Current User Location"]);
			}
			catch (UnauthorizedAccessException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. The registry could not be accessed. The account will not be deleted. "
				+ "If the problem persists, please contact the Administrator. "
								+ "The settings could not be saved.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}
			catch (SecurityException)
			{
				NativeMethods.TaskDialog(new WindowInteropHelper(App.Current.MainWindow).Handle, IntPtr.Zero, "Error - Record Pro", "Access Denied",
									"An error has occurred. The registry could not be accessed. The account will not be deleted. "
				+ "If the problem persists, please contact the Administrator. "
								+ "The settings could not be saved.", NativeMethods.TaskDialogButtons.OK, NativeMethods.TaskDialogIcon.Error);
			}

		}
	}

}
