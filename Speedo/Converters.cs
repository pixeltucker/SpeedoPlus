// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Solal Pirelli

using System;
using System.Globalization;
using System.Windows;
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
            return (double) value - Opacity <= double.Epsilon;
        }
    }

    public sealed class BooleanToStringConverter : IValueConverter
    {
        public string True { get; set; }
        public string False { get; set; }

        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return (bool) value ? True : False;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }

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

    public sealed class SpeedUnitToStringConverter : IValueConverter
    {
        public string Kilometers { get; set; }
        public string Miles { get; set; }

        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return (SpeedUnit) value == SpeedUnit.Kilometers ? Kilometers : Miles;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }

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