using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Panoramio.Converters
{
    public class CountToVisibilityConverter : IValueConverter
    {
        public bool InvertValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var count = value as int?;

            var hasValue = !(count == null || count == 0);

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
