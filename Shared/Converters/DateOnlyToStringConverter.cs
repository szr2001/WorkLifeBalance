using System.Globalization;
using System.Windows.Data;

namespace WorkLifeBalance.Shared.Converters
{
    public class DateOnlyToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is DateOnly date ? date.ToString("MM/dd/yyyy") : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
