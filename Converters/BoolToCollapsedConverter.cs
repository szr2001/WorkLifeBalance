using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WorkLifeBalance.Converters
{
    public class BoolToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool enabled)
            {
                return enabled == true ? Visibility.Collapsed : Visibility.Visible;
            }

            throw new Exception("Value is not of type bool");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visible)
            {
                return visible == Visibility.Visible ? false : true;
            }

            throw new Exception("Value is not of type Visibility");
        }
    }
}
