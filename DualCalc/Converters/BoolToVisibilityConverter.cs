using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace DualCalc.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool bVal = value is bool b && b;
            bool invert = parameter is string s && s == "Invert";
            return (bVal ^ invert) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            bool invert = parameter is string s && s == "Invert";
            bool visible = value is Visibility v && v == Visibility.Visible;
            return visible ^ invert;
        }
    }
}
