using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace RecordPro
{
	class GradeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			byte convertedValue;
			if (value == null || !byte.TryParse(value.ToString(), out convertedValue))
				return null;
			else if (convertedValue >= 100)
				return "GradeStyle1";
			else if (convertedValue >= 95)
				return "GradeStyle2";
			else if (convertedValue >= 85)
				return "GradeStyle3";
			else if (convertedValue >= 75)
				return "GradeStyle4";
			else if (convertedValue >= 65)
				return "GradeStyle5";
			else
				return "GradeStyle6";
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}

	class GradeVisibilityConverter:IValueConverter
	{

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
				return "Collapsed";
			else
				return "Visible";
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return DependencyProperty.UnsetValue;
		}
	}
}
