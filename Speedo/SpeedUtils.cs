using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speedo
{
    public static class SpeedUtils
    {
        public static string GetString( SpeedUnit unit )
        {
            return unit == SpeedUnit.Kilometers ? "km/h" : "mph";
        }

        public static double GetFactor( SpeedUnit unit )
        {
            return unit == SpeedUnit.Kilometers ? 1 : 0.621371192;
        }
    }
}