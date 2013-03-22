// new

using System.Device.Location;

namespace Speedo.Controls
{
    public partial class DistanceDisplay : SpeedControl
    {
        private GeoCoordinate lastPosition;

        private double distance;
        public double Distance
        {
            get { return distance; }
            set { SetProperty( ref distance, value ); }
        }

        public DistanceDisplay()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }

        protected override void ChangeUnits( double factor )
        {
            Distance *= factor;
        }

        protected override void ChangeSpeed( double speed, GeoCoordinate position )
        {
            if ( lastPosition != null )
            {
                Distance += lastPosition.GetDistanceTo( position );
            }
            lastPosition = position;
        }

        protected override void Clear()
        {
            Distance = 0;
        }
    }
}