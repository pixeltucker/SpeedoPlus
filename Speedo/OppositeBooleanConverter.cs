// new

using System;
using System.Windows.Data;

namespace Speedo
{
    public sealed class OppositeBooleanConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return !( (bool) value );
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return !( (bool) value );
        }
    }
}