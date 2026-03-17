using System;
using System.Globalization;
using System.Windows.Data;

namespace YouTubeApiLib.GuiTestWPF
{
	public class BoolInverseConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return targetType == typeof(bool) && value != null ? !(bool)value : false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return targetType == typeof(bool) && value != null ? !(bool)value : false;
		}
	}
}
