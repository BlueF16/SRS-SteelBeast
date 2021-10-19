﻿using System;
using System.Windows.Data;

namespace Ciribob.FS3D.SimpleRadio.Standalone.Client.Utils.ValueConverters
{
	class BooleanInverterConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return !(bool)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
