// new 

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
    }
}