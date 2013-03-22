using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Windows;
using Windows.Devices.Sensors;

// TODO: Test the geocoordinate code well

namespace Speedo
{
    public partial class DirectionDisplay : ObservableControl
    {
        private static readonly Dictionary<double, string> Directions = new Dictionary<double, string>
        {
            { 0, "N" },
            { 22.5, "NE" },
            { 67.5, "E" },
            { 112.5, "SE" },
            { 157.5, "S" },
            { 202.5, "SW" },
            { 247.5, "W" },
            { 292.5, "NW" },
            { 337.5, "N" }
        };

        private Compass compass;
        private GeoCoordinateWatcher watcher;

        #region AllowLocationAccess DependencyProperty
        public bool AllowLocationAccess
        {
            get { return (bool) GetValue( AllowLocationAccessProperty ); }
            set { SetValue( AllowLocationAccessProperty, value ); }
        }

        public static readonly DependencyProperty AllowLocationAccessProperty =
            DependencyProperty.Register( "AllowLocationAccess", typeof( bool ), typeof( DirectionDisplay ), new PropertyMetadata( OnAllowLocationAccessPropertyChanged ) );

        private static void OnAllowLocationAccessPropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var display = (DirectionDisplay) obj;

            if ( display.watcher != null )
            {
                if ( (bool) args.NewValue )
                {
                    display.IsEnabled = true;
                    display.watcher.Start();
                }
                else
                {
                    display.IsEnabled = false;
                    display.watcher.Stop();
                }
            }
        }
        #endregion

        private double directionAngle;
        public double DirectionAngle
        {
            get { return directionAngle; }
            set { SetProperty( ref directionAngle, value ); }
        }

        private string directionInitials;
        public string DirectionInitials
        {
            get { return directionInitials; }
            set { SetProperty( ref directionInitials, value ); }
        }

        public DirectionDisplay()
        {
            compass = Compass.GetDefault();
            if ( compass == null )
            {
                watcher = new GeoCoordinateWatcher();
                watcher.PositionChanged += Watcher_PositionChanged;
                watcher.StatusChanged += Watcher_StatusChanged;
                watcher.MovementThreshold = 1;
                watcher.Start();
            }
            else
            {
                IsEnabled = true;
                compass.ReadingChanged += Compass_ReadingChanged;
            }

            InitializeComponent();
            LayoutRoot.DataContext = this;

            Update( 0 );
        }

        private void Update( double direction )
        {
            Dispatcher.BeginInvoke( () =>
            {
                DirectionAngle = direction;
                DirectionInitials = Directions.Last( p => direction >= p.Key ).Value;
            } );
        }

        private void Compass_ReadingChanged( object sender, CompassReadingChangedEventArgs e )
        {
            double reading = e.Reading.HeadingTrueNorth ?? e.Reading.HeadingMagneticNorth;
            Update( reading );
        }

        private void Watcher_PositionChanged( object sender, GeoPositionChangedEventArgs<GeoCoordinate> e )
        {
            Update( e.Position.Location.Course );
        }

        private void Watcher_StatusChanged( object sender, GeoPositionStatusChangedEventArgs e )
        {
            IsEnabled = e.Status == GeoPositionStatus.Ready && AllowLocationAccess;
        }
    }
}