// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Solal Pirelli

using System.Device.Location;

namespace Speedo.Controls
{
    public partial class DistanceDisplay : MovementControl
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

        protected override void ChangePosition( GeoCoordinate position )
        {
            if ( lastPosition != null && !lastPosition.IsUnknown && !position.IsUnknown )
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