using System;
using System.Windows.Data;

namespace Speedo
{
    public sealed class BooleanToOpacityConverter : IValueConverter
    {
        public double Opacity { get; set; }

        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return (bool) value ? Opacity : 0;
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return (double) value == Opacity;
        }
    }
}