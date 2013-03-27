// new

using System;
using System.Globalization;
using System.Windows.Data;

namespace Speedo
{
    public sealed class GpsStatusToStringConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            switch ( (GpsStatus) value )
            {
                case GpsStatus.Unavailable:
                    return "GPS not available";
                case GpsStatus.Inaccessible:
                    return "location inaccessible";
                case GpsStatus.Initializing:
                    return "GPS initializing";
                case GpsStatus.Weak:
                    return "weak GPS signal";
                case GpsStatus.Normal:
                default:
                    return "";
            }
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
}