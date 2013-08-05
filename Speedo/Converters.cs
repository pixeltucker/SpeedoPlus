// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Solal Pirelli

using System;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Data;
using Speedo.Languages;

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

    public sealed class LocationAccessToMenuStringConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return (bool) value ? AppResources.SwitchLocationMenu_Enabled : AppResources.SwitchLocationMenu_Disabled;
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

    public sealed class SpeedUnitToMenuStringConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return (SpeedUnit) value == SpeedUnit.Kilometers ? AppResources.SwitchUnitMenu_Kilometers : AppResources.SwitchUnitMenu_Miles;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Converts enums to localized strings from AppResources.
    /// </summary>
    public sealed class EnumToLocalizedStringConverter : IValueConverter
    {
        private ResourceManager _manager;

        public EnumToLocalizedStringConverter()
        {
            var assembly = typeof( AppResources ).Assembly;
            _manager = new ResourceManager( assembly.GetName().Name + ".Languages.AppResources", assembly );
        }

        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            string enumName = value.GetType().Name;
            return _manager.GetString( enumName + "_" + value );
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
}