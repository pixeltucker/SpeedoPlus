// new

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Speedo
{
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public bool IsReversed { get; set; }

        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( IsReversed )
            {
                return (bool) value ? Visibility.Collapsed : Visibility.Visible;
            }
            return (bool) value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
}