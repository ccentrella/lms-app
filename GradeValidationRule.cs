using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace RecordPro
{
	class GradeValidationRule : ValidationRule
	{
		public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
		{
			if (value.GetHashCode() == 1)
			{
				byte convertedByte;
				if (value == null || (byte.TryParse(value.ToString(), out convertedByte) && convertedByte <= 100))
					return new ValidationResult(true,null);
				else
					return new ValidationResult(false, "An invalid grade has been entered.");
			}
			else
				return null;
		}
		
	}
}
