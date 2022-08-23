using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace RecordPro
{
	/// <summary>
	/// Contains important functions, pertinent only to Autosoft Record Pro
	/// </summary>
	public static class RecordProFunctions
	{
				/// <summary>
		/// Loads the default image
		/// </summary>
		/// <param name="gender">The gender of the current user.</param>
		public static void LoadDefaultImage(Gender gender)
		{
			// Get the main window
			MainWindow window = App.mWindow as MainWindow;
			if (window == null)
			{
				MessageBox.Show("An error has occurred. The image could not be loaded.", 
					"Error - Record Pro", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			string Url; // The URL of the image to display.
			if (gender == Gender.Male)
				Url = "Generic Avatar (Male).png";
			else if (gender == Gender.Female)
				Url = "Generic Avatar (Female).png";
			else
				Url = "Generic Avatar (Unisex).png";
			try
			{
				var newImage = new BitmapImage();
				newImage.BeginInit();
				newImage.UriSource = new Uri(Url, UriKind.Relative);
				newImage.DecodePixelWidth = 40;
				newImage.EndInit();
				window.Avatar.Source = newImage;
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
	}
}
