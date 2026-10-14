using System.Globalization;
using System.Windows.Data;

namespace WorkLifeBalance.Shared.Converters
{
    public class TimeOnlyToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is TimeOnly time ? time.ToString("HH:mm:ss") : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
