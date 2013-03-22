using System;
using System.Windows.Data;

namespace Speedo
{
    public sealed class SpeedUnitToStringConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return SpeedUtils.GetString( (SpeedUnit) value );
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
}