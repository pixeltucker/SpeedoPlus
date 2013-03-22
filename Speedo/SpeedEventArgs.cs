// new

using System;
using System.Device.Location;

namespace Speedo
{
    public sealed class SpeedEventArgs : EventArgs
    {
        public double CurrentSpeed { get; private set; }
        public GeoCoordinate CurrentPosition { get; private set; }

        public SpeedEventArgs( double currentSpeed, GeoCoordinate currentPosition )
        {
            CurrentSpeed = currentSpeed;
            CurrentPosition = currentPosition;
        }
    }
}