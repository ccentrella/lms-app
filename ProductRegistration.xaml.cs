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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RecordPro
{
	/// <summary>
	/// Interaction logic for ProductRegistration.xaml
	/// </summary>
	public partial class ProductRegistration : Page
	{
		string _data;
		string _folderLocation;

		/// <summary>
		/// Show the product registration screen
		/// </summary>
		/// <param name="data">The data for the user</param>
		/// <param name="folderLocation">The user's folder</param>
		public ProductRegistration(string data, string folderLocation)
		{
			_data = data;
			_folderLocation = folderLocation;
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (guidLabel.Text == null)
			{
				MessageBox.Show("An error has occurred. Please try logging in again.", "Error - Record Pro", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			string guid = System.IO.Path.GetFileName(_folderLocation);
			if (guid.Length != 38)
			{
				MessageBox.Show("Your account was not created correctly. Please contact Autosoft.",
					"Error - Record Pro", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			// Attempt to get the product key
			char[] guidArray = guid.ToCharArray();

			List<char> guidList = new List<char> { guidArray[3], guidArray[15], 'b',
				guidArray[22], '-', guidArray[29],guidArray[26],guidArray[2],'F','-',
				guidArray[35],guidArray[21] ,guidArray[5],guidArray[8],'-',
			guidArray[18],guidArray[32],guidArray[6],guidArray[27]};
			string productKey = string.Join("", guidList.ToArray());

			// If the user entered the correct product key, activate the user
			if (textBox.Text == productKey)
				ActivateUser();
			else
				if (MessageBox.Show("The product key is incorrect. Please enter a valid product key.",
					"Incorrect Product Key", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
					App.Logout();
		}

		// Activates the user
		private void ActivateUser()
		{
			// Create the validation file. 
			// If the date changes just as the file is saved, just make a new file
			CreateValidationFile(_folderLocation);
			if (!App.ValidateUser(_folderLocation))
				CreateValidationFile(_folderLocation);

			// Show the user's home window
			MainWindow window = App.mWindow as MainWindow;
			if (window == null)
			{
				MessageBox.Show("An error has occurred. Please try logging in again.",
					"Error - Record Pro", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			var newHome = new Home();
			window.MainFrame.Navigate(newHome);
			App.LoadTheme(_data); // Load the theme
		}

		/// <summary>
		/// Creates a validation file for the specified user.
		/// </summary>
		/// <param name="folderLocation">The folder where the user's information is stored.</param>
		private void CreateValidationFile(string folderLocation)
		{
			string validationFile = System.IO.Path.Combine(folderLocation, "Validation.txt");
			string folderName = System.IO.Path.GetFileName(folderLocation);
			char[] folderArray = folderName.ToCharArray();
			string modificationDateString = DateTime.Today.ToString();
			string separator = modificationDateString[6].ToString() + modificationDateString[0].ToString()
			+ modificationDateString[5].ToString() + modificationDateString[4].ToString();
			string data = string.Join(separator, folderArray);
			try
			{
				using (var newReader = new StreamWriter(validationFile))
					newReader.Write(data);
			}
			catch (IOException)
			{
				MessageBox.Show("Your account could not be activated.", "Activation Error - Record Pro",
					MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (ArgumentException)
			{
				MessageBox.Show("Your account could not be activated.", "Activation Error - Record Pro",
					MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (UnauthorizedAccessException)
			{
				MessageBox.Show("Your account could not be activated.", "Activation Error - Record Pro",
					MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (NotSupportedException)
			{
				MessageBox.Show("Your account could not be activated.", "Activation Error - Record Pro",
					MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (SecurityException)
			{
				MessageBox.Show("Your account could not be activated.", "Activation Error - Record Pro",
					MessageBoxButton.OK, MessageBoxImage.Error);

			}
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			string folder = System.IO.Path.GetFileName(_folderLocation);

			// Update the guid label
			if (folder != null)
				guidLabel.Text = String.Format("If you need a product key, please contact Autosoft "
				+ "and give them the following information: {0}", folder);
		}
	}
}
