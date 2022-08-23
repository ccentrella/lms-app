using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RecordPro
{
	public class NameValidationRule : ValidationRule
	{
		public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
		{
			// Ensure the name is valid
			if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
				return new ValidationResult(false, "The name is invalid. Please choose a valid name.");
			else
				return new ValidationResult(true, "");
		}
	}
}
