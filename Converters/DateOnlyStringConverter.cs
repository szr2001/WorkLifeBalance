using System;
using System.Globalization;
using System.Windows.Data;

namespace WorkLifeBalance.Converters
{
    public class DateOnlyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateOnly date)
            {
                return date.ToString("MM/dd/yyyy");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is string date)
            {
                DateOnly newDate = DateOnly.MinValue;
                string[] dateData = date.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                newDate = new
                    (
                        int.Parse(dateData[2]),
                        int.Parse(dateData[0]),
                        int.Parse(dateData[1])
                    );
                return newDate;
            }
            else
            {
                return DateOnly.MinValue;
            }
        }
    }
}
