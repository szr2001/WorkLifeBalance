using System;
using System.Globalization;
using System.Windows.Data;

namespace WorkLifeBalance.Converters
{
    public class TimeOnlyToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeOnly time)
            {
                return time.ToString("HH:mm:ss");
            }
            throw new Exception("Value is not of type TimeOnly");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string time)
            {
                TimeOnly newTime = TimeOnly.MinValue;
                string[] timeData = time.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                Console.WriteLine(timeData.Length);
                foreach(string timer in timeData)
                {
                    Console.WriteLine(timer);
                }
                newTime = new
                    (
                        int.Parse(timeData[0]),
                        int.Parse(timeData[1]),
                        int.Parse(timeData[2])
                    );
                return newTime;
            }
            throw new Exception("Value is not of type string");
        }
    }
}
