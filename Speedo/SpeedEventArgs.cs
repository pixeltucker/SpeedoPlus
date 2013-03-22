using System;

namespace Speedo
{
    public sealed class SpeedEventArgs : EventArgs
    {
        public double CurrentSpeed { get; private set; }

        public SpeedEventArgs( double currentSpeed )
        {
            CurrentSpeed = currentSpeed;
        }
    }
}