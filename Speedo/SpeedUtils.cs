// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Solal Pirelli

using System;

namespace Speedo
{
    public static class SpeedUtils
    {
        public static double GetFactor( SpeedUnit unit )
        {
            return unit == SpeedUnit.Kilometers ? 1 : 0.621371192;
        }

        public static SpeedUnit Switch( SpeedUnit unit )
        {
            return unit == SpeedUnit.Kilometers ? SpeedUnit.Miles : SpeedUnit.Kilometers;
        }

        public static int ConvertSpeedLimit( SpeedUnit oldUnit, SpeedUnit newUnit, int limit )
        {
            double factor = GetFactor( newUnit ) / GetFactor( oldUnit );
            int newLimit = (int) Math.Round( limit * factor );
            return newLimit - ( newLimit % 5 );
        }
    }
}