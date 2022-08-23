namespace Record_Pro_Functions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows;

	/// <summary>
	/// Important functions used by many Autosoft programs
	/// </summary>
	public static class BasicFunctions
	{
		/// <summary>
		/// Acquire the appropriate value from the user file.
		/// </summary>
		/// <param name="userData">The data for the current user</param>
		/// <param name="propertyName">The property to find</param>
		/// <returns>Returns the data for the property. If no data is found, a blank string is returned.</returns>
		public static string GetValue(string userData, string propertyName)
	
		{
			int propertyNameStartIndex;
			int propertyNameEndIndex;
			int propertyTextStartIndex;
			int propertyTextEndIndex;
			int propertyTextCurrentPosition;
			int length = 0;
			string result;
			bool isEndFound = false;

			// Find where the text starts
			if (userData.IndexOf(propertyName + " = ") == 0)
				propertyNameStartIndex = userData.IndexOf(propertyName + " = ");
			else
				propertyNameStartIndex = userData.IndexOf(" " + propertyName + " = ");

			if (propertyNameStartIndex == -1)
				return string.Empty;
			propertyNameEndIndex = propertyNameStartIndex + propertyName.Length + 1;
			propertyTextStartIndex = userData.IndexOf("\"", propertyNameEndIndex) + 1;
			propertyTextCurrentPosition = propertyTextStartIndex;
			if (propertyTextStartIndex == userData.Length)
				return string.Empty;

			// Enumerate until a valid quotation mark is found
			while (!isEndFound)
			{
				propertyTextEndIndex = userData.IndexOf("\"", propertyTextCurrentPosition);

				// If the string does not stop, act as if it stops at the end
				if (propertyTextEndIndex == -1)
					propertyTextEndIndex = userData.Length - 1;
				length = propertyTextEndIndex - propertyTextStartIndex;
				if (length == 0)
					return string.Empty;

				// If certain conditions are met, reject the quotation mark
				if (userData.ElementAt(propertyTextEndIndex - 1) == '\\')
				{
					int slashCount = 1;
					int slashIndex = propertyTextEndIndex - 1;
					char currentChar = '\\';
					while (currentChar == '\\')
					{
						// If we're at the end of the string, stop.
						if (slashIndex == 0)
							break;

						// Enumerate until there is a character that is not a slash
						currentChar = userData.ElementAt(slashIndex - 1);

						if (currentChar == '\\')
						{
							slashCount++;
							slashIndex--;
						}
					}

					// If the slash is an escape character, reject it
					if (slashCount % 2 != 0)
					{
						propertyTextCurrentPosition = propertyTextEndIndex + 1;
						continue;
					}
				}

				// The loop should now end
				isEndFound = true;
			}

			// Change slashes and quotation marks to match what the user entered
			result = userData.Substring(propertyTextStartIndex, length).Replace(@"\""", @"""").Replace("\\\\", "\\");
			return result;
		}

		/// <summary>
		/// Converts a formatted string to one appropriate for viewing
		/// </summary>
		/// <param name="text">The text to change</param>
		/// <returns>A string with the quotation marks updated</returns>
		public static string ConvertToQuotes(this string text)
		{
			// Change slashes and quotation marks to match what the user entered
			return text.Replace(@"\""", @"""").Replace("\\\\", "\\");

		}

		/// <summary>
		/// Enumerates through the string and returns a list of strings.
		/// </summary>
		/// <param name="data">The string containing data</param>
		/// <returns>Returns a list of strings that have been found</returns>
		public static string[] EnumerateStrings(string data)
		{
			var newList = new List<string>();
			int commaLocation = 0;
			int endLocation = 0;
			while (endLocation < data.Length)
			{
				string str;
				commaLocation = data.IndexOf(",", endLocation);
				if (commaLocation == -1)
					commaLocation = data.Length;
				var dataCount = commaLocation - endLocation;
				if (dataCount > 0)
				{
					str = data.Substring(endLocation, dataCount);
					newList.Add(str);
				}
				endLocation = commaLocation + 1;
			}
			return newList.ToArray();
		}

		/// <summary>
		/// Enumerates through the string and returns a list of strings.
		/// </summary>
		/// <param name="userData">The data for the current user</param>
		/// <param name="property">The property to find</param>
		/// <returns>Returns a list of strings that have been found</returns>
		public static string[] EnumerateStrings(string userData, string property)
		{
			string dataString = BasicFunctions.GetValue(userData, property);
			return EnumerateStrings(dataString);
		}

		/// <summary>
		/// Enumerates through the string and returns a list of integers.
		/// </summary>
		/// <param name="data">The string containing data</param>
		/// <returns>Returns a list of integers that have been found</returns>
		public static int[] EnumerateIntegers(string data)
		{
			var newList = new List<int>();
			int commaLocation = 0;
			int endLocation = 0;
			while (endLocation < data.Length)
			{
				string str;
				int intValue;
				commaLocation = data.IndexOf(",", endLocation);
				if (commaLocation == -1)
					commaLocation = data.Length;
				var dataCount = commaLocation - endLocation;
				if (dataCount > 0)
				{
					str = data.Substring(endLocation, dataCount);
					int.TryParse(str, out intValue);
					if (intValue != 0)
						newList.Add(intValue);
				}

				endLocation = commaLocation + 1;
			}

			return newList.ToArray();
		}

		/// <summary>
		/// Enumerates through the string and returns a list of integers.
		/// </summary>
		/// <param name="userData">The data for the current user</param>
		/// <param name="property">The property to find</param>
		/// <returns>Returns a list of integers that have been found</returns>
		public static int[] EnumerateIntegers(string userData, string property)
		{
			string dataString = BasicFunctions.GetValue(userData, property);
			return EnumerateIntegers(dataString);
		}

		/// <summary>
		/// Returns a welcome message, which will be seen by the user
		/// </summary>
		/// <param name="name">The name of the user</param>
		/// <returns>A welcome message, acontaining the user's name</returns>
		public static string GetWelcomeMessage(this string name)
		{
			int length = name.IndexOf(" ");
			if (length == -1)
				length = name.Length;

			// If the name is not blank, return welcome text. Else, return a null string.
			if (length > 0)
				return string.Join(" ", "Hi", name.Substring(0, length) + "!");
			else
				return "";
		}


		/// <summary>
		/// Replaces a string containing the function with the new data, returning a string
		/// </summary>
		/// <param name="userData">The data containing the function</param>
		/// <param name="propertyName">The name of the property</param>
		/// <param name="newData">The new data to be used</param>
		/// <returns>A string containing the new data</returns>
		public static string ReplaceValue(this string userData, string propertyName, string newData)
		{
			return ReplaceValue(userData, propertyName, newData, false);
		}

		/// <summary>
		/// Replaces a string containing the function with the new data, returning a string
		/// </summary>
		/// <param name="userData">The data containing the function</param>
		/// <param name="propertyName">The name of the property</param>
		/// <param name="newData">The new data to be used</param>
		/// <param name="createLine">Specifies whether a new line should be created if the name is not found</param>
		/// <returns>A string containing the new data</returns>
		public static string ReplaceValue(this string userData, string propertyName, string newData, bool createLine)
		{
			int propertyNameStartIndex;
			int propertyNameEndIndex;
			int propertyTextStartIndex;
			int propertyTextEndIndex;
			int propertyTextCurrentPosition;
			int length = 0;
			string result;
			bool isEndFound = false;

			// Find where the text starts
			if (userData.IndexOf(propertyName + " = ") == 0)
				propertyNameStartIndex = userData.IndexOf(propertyName + " = ");
			else
				propertyNameStartIndex = userData.IndexOf(" " + propertyName + " = ");

			// If the property doesn't exist, add it now
			if (propertyNameStartIndex == -1 & createLine)
			{
				if (userData != "")
					userData = userData.Insert(userData.Length, " ");
				return string.Format("{0}{1} = \"{2}\"\r\n", userData, propertyName, newData);
			}
			else if (propertyNameStartIndex == -1)
			{
				if (userData != "")
					userData = userData.Insert(userData.Length, " ");
				return string.Format("{0}{1} = \"{2}\"", userData, propertyName, newData);
			}
			propertyNameEndIndex = propertyNameStartIndex + propertyName.Length + 1;
			propertyTextStartIndex = userData.IndexOf("\"", propertyNameEndIndex) + 1;
			propertyTextCurrentPosition = propertyTextStartIndex;

			// If no value is found, add it now
			if (propertyTextStartIndex == userData.Length)
			{
				string newInfo = string.Format("{0}\"", newData);
				result = userData.Insert(propertyTextStartIndex, newInfo);
				return result;
			}
			// Enumerate until a valid quotation mark is found
			while (!isEndFound)
			{
				propertyTextEndIndex = userData.IndexOf("\"", propertyTextCurrentPosition);

				// If the string does not stop, act as if it stops at the end
				if (propertyTextEndIndex == -1)
					propertyTextEndIndex = userData.Length - 1;
				length = propertyTextEndIndex - propertyTextStartIndex;

				// If certain conditions are met, reject the quotation mark
				if (userData.ElementAt(propertyTextEndIndex - 1) == '\\')
				{
					int slashCount = 1;
					int slashIndex = propertyTextEndIndex - 1;
					char currentChar = '\\';
					while (currentChar == '\\')
					{
						// If we're at the end of the string, stop.
						if (slashIndex == 0)
							break;

						// Enumerate until there is a character that is not a slash
						currentChar = userData.ElementAt(slashIndex - 1);

						if (currentChar == '\\')
						{
							slashCount++;
							slashIndex--;
						}
					}

					// If the slash is an escape character, reject it
					if (slashCount % 2 != 0)
					{
						propertyTextCurrentPosition = propertyTextEndIndex + 1;
						continue;
					}
				}

				// The loop should now end
				isEndFound = true;
			}

			// Change slashes and quotation marks to match what the user entered
			result = userData.Remove(propertyTextStartIndex, length).Insert(propertyTextStartIndex, newData);
			return result;
		}

		/// <summary>
		/// Converts the specified timespan to a readable string
		/// </summary>
		/// <param name="seconds">The total seconds which will be converted</param>
		/// <returns>The new string returned</returns>
		public static string GetTimeLabel(this double seconds)
		{
			long minutes = (long)seconds / 60;
			long hours = minutes / 60;
			minutes = minutes % 60;
			string minuteText = "minute";
			string hourText = "hour";
			if (hours != 1)
				hourText = hourText + "s";
			if (minutes != 1)
				minuteText = minuteText + "s";

			if (hours > 0 & minutes > 0)
				return string.Join(" ", hours, hourText, minutes, minuteText);
			else if (hours > 0)
				return string.Join(" ", hours, hourText);
			else
				return string.Join(" ", minutes, minuteText);
		}


		/// <summary>
		/// Converts the specified timespan to a readable string
		/// </summary>
		/// <param name="time">The timespan to convert</param>
		/// <returns>The new string returned</returns>
		public static string GetTimeLabel(this TimeSpan time)
		{
			int hours = time.Days * 24 + time.Hours;
			int minutes = time.Minutes;
			string minuteText = "minute";
			string hourText = "hour";
			if (time.Seconds >= 30)
				minutes++;
			if (minutes == 60)
			{
				minutes = 0;
				hours++;
			}
			if (hours != 1)
				hourText = hourText + "s";
			if (minutes != 1)
				minuteText = minuteText + "s";

			if (hours > 0 & minutes > 0)
				return string.Join(" ", hours, hourText, minutes, minuteText);
			else if (hours > 0)
				return string.Join(" ", hours, hourText);
			else
				return string.Join(" ", minutes, minuteText);
		}

		/// <summary>
		/// Converts a list of strings to friendly text
		/// </summary>
		/// <param name="array">The list of strings</param>
		/// <returns>The list of elements converted to a friendly string</returns>
		public static string GetFriendlyText(this string[] array)
		{
			int totalLength = array.Length;
			if (totalLength == 1)
			{
				return array[0];
			}
			else if (totalLength == 2)
			{
				return string.Format("{0} and {1}", array[0], array[1]);
			}
			else
			{
				var newBuilder = new StringBuilder();
				for (int i =0; i< totalLength; i++)
				{
					string item = array[i];
					if (i < totalLength - 2)
					{
						newBuilder.AppendFormat("{0}, ", item);
					}
					else if (i < totalLength - 1)
					{
						newBuilder.AppendFormat("{0}, and ", item);
					}
					else
					{
						newBuilder.Append(item);
					}
				}
				return newBuilder.ToString();
			}
		}
	}
}
