using System;

namespace Speedo
{
    public sealed class SpeedSource
    {
        public event EventHandler<SpeedEventArgs> SpeedChanged;
        public void ChangeSpeed( double currentSpeed )
        {
            var evt = SpeedChanged;
            if ( evt != null )
            {
                evt( this, new SpeedEventArgs( currentSpeed ) );
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