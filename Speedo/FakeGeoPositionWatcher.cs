// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Solal Pirelli

#if DEBUG
using System;
using System.Device.Location;
using System.Windows.Threading;

namespace Speedo
{
    // This class is horribly wrong from a geographical point of view
    // TODO: maybe change it...
    public sealed class FakeGeoPositionWatcher : IGeoPositionWatcher<GeoCoordinate>
    {
        private const double MinimumChange = 0.00005; // lon/lat
        private const double MaximumChange = 0.0003; // lon/lat
        private const int EarthRadius = 6371009; // meters
        private const int TimerTick = 1; // seconds

        private DispatcherTimer timer;
        private Random random;

        private GeoPosition<GeoCoordinate> position;
        public GeoPosition<GeoCoordinate> Position
        {
            get { return position; }
            set { position = value; OnPositionChanged(); }
        }

        private GeoPositionStatus status;
        public GeoPositionStatus Status
        {
            get { return status; }
            set { status = value; OnStatusChanged(); }
        }

        public FakeGeoPositionWatcher( double latitude, double longitude )
        {
            var coordinate = new GeoCoordinate( latitude, longitude );
            Position = new GeoPosition<GeoCoordinate>( DateTimeOffset.Now, coordinate );

            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds( TimerTick );

            random = new Random();
        }

        public event EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>> PositionChanged;
        private void OnPositionChanged()
        {
            var evt = PositionChanged;
            if ( evt != null )
            {
                evt( this, new GeoPositionChangedEventArgs<GeoCoordinate>( Position ) );
            }
        }

        public event EventHandler<GeoPositionStatusChangedEventArgs> StatusChanged;
        private void OnStatusChanged()
        {
            var evt = StatusChanged;
            if ( evt != null )
            {
                evt( this, new GeoPositionStatusChangedEventArgs( Status ) );
            }
        }

        public void Start( bool suppressPermissionPrompt )
        {
            Start();
        }

        public void Start()
        {
            Status = GeoPositionStatus.Ready;
            timer.Start();
        }

        public bool TryStart( bool suppressPermissionPrompt, TimeSpan timeout )
        {
            Start();
            return true;
        }

        public void Stop()
        {
            Status = GeoPositionStatus.Disabled;
            timer.Stop();
        }

        private double GetRandomChange()
        {
            double sign = Math.Sign( random.NextDouble() - 0.5 );
            return sign * Math.Max( MinimumChange, random.NextDouble() % 2 * MaximumChange );
        }

        private void Timer_Tick( object sender, EventArgs e )
        {
            double latChange = GetRandomChange(), longChange = GetRandomChange();
            double newLat = Position.Location.Latitude + latChange;
            double newLong = Position.Location.Longitude + longChange;
            if ( newLat > 90 || newLat < -90 )
            {
                newLat = 0;
            }
            if ( newLong > 180 || newLong < -180 )
            {
                newLong = 0;
            }

            var newCoordinate = new GeoCoordinate( newLat, newLong )
            {
                HorizontalAccuracy = 10,
                VerticalAccuracy = 10,
                Course = random.Next( 360 ) + random.NextDouble(),
                Altitude = 0
            };
            newCoordinate.Speed = newCoordinate.GetDistanceTo( Position.Location ) / TimerTick;

            Position = new GeoPosition<GeoCoordinate>( DateTimeOffset.Now, newCoordinate );
        }
    }
}
#endif