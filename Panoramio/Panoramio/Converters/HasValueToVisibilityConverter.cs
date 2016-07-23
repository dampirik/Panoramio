using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Panoramio.Converters
{
    public class HasValueToVisibilityConverter : IValueConverter
    {
        public bool InvertValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var hasValue = !(value == null || (value is string && string.IsNullOrWhiteSpace((string)value)));

            if (InvertValue)
            {
                hasValue = !hasValue;
            }

            return hasValue ? Visibility.Visible : Visibility.Collapsed;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}