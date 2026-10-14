using System.Globalization;
using System.Windows.Data;

namespace WorkLifeBalance.Shared.Converters
{
    // True when the enum value has the given name: the ConverterParameter, or the second value of a MultiBinding
    public class EnumToBooleanConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value?.ToString() == parameter?.ToString();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
            values.Length == 2 && values[0]?.ToString() == values[1]?.ToString();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
