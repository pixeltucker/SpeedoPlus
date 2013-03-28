// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Solal Pirelli

using System;
using System.Globalization;
using System.Windows.Data;

namespace Speedo
{
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
}
