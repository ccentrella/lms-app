using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO;
using Record_Pro_Functions;

namespace RecordPro
{
	public class UserNameValidationRule : ValidationRule
	{

		public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
		{
			string usersLocation = (string)App.Current.Properties["Users Location"];
			if (usersLocation == null || !Directory.Exists(usersLocation))
				return new ValidationResult(false, "Record Pro has not been configured properly.");

			// Ensure a valid user-name has been entered
			if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
				return new ValidationResult(false, "The user-name is invalid. Please enter a valid user-name.");

			// Ensure no other user exists with the same user-name
			try
			{
				foreach (var directory in Directory.EnumerateDirectories(usersLocation))
				{
					var configFile = Path.Combine(directory, "config.txt");
					string configData = File.ReadAllText(configFile);
					if (BasicFunctions.GetValue(configData, "UserName") == value.ToString())
					{
						return new ValidationResult(false, "The user-name already exists. Please enter a different user-name.");
					}
				}
			}
			catch (System.IO.IOException)
			{
				return new ValidationResult(false, "Record Pro has not been configured properly.");
			}
			catch (ArgumentException)
			{
				return new ValidationResult(false, "Record Pro has not been configured properly.");
			}
			catch (UnauthorizedAccessException)
			{
				return new ValidationResult(false, "Record Pro has not been configured properly.");
			}
			catch (SecurityException)
			{
				return new ValidationResult(false, "Record Pro has not been configured properly.");
			}
			catch (NotSupportedException)
			{
				return new ValidationResult(false, "Record Pro has not been configured properly.");
			}

			// If we've made it this far, we are good to go!
			return new ValidationResult(true, "");
		}
	}
}
