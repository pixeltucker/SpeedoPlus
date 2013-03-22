// new

using System;
using System.Device.Location;

namespace Speedo
{
    public sealed class SpeedSource
    {
        public event EventHandler<SpeedEventArgs> SpeedChanged;
        public void ChangeSpeed( double currentSpeed, GeoCoordinate currentPosition )
        {
            var evt = SpeedChanged;
            if ( evt != null )
            {
                evt( this, new SpeedEventArgs( currentSpeed, currentPosition ) );
            }
        }

        public event EventHandler Cleared;
        public void Clear()
        {
            var evt = Cleared;
            if ( evt != null )
            {
                evt( this, EventArgs.Empty );
            }
        }
    }
}