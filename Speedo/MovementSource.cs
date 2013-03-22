// new

using System;
using System.ComponentModel;
using System.Device.Location;
using Windows.Devices.Sensors;

// TODO: Maybe three separate events are needed?

namespace Speedo
{
    public sealed class MovementSource
    {
        private Compass compass;
        private GeoCoordinateWatcher watcher;
        private bool forceStop;

        public double Speed { get; private set; }
        public GeoCoordinate Position { get; private set; }
        public double Course { get; private set; }

        public MovementSource()
        {
            Position = GeoCoordinate.Unknown;

            compass = Compass.GetDefault();
            if ( compass != null )
            {
                compass.ReadingChanged += Compass_ReadingChanged;
            }

            if ( !DesignerProperties.IsInDesignTool ) // Cider hates GeoCoordinateWatcher
            {
                watcher = new GeoCoordinateWatcher( GeoPositionAccuracy.High );
                watcher.PositionChanged += Watcher_PositionChanged;
                watcher.StatusChanged += Watcher_StatusChanged;
                watcher.Start();
            }
        }

        public event EventHandler<GeoPositionStatusChangedEventArgs> GeoStatusChanged;
        private void OnGeoStatusChanged( GeoPositionStatusChangedEventArgs e )
        {
            var evt = GeoStatusChanged;
            if ( evt != null )
            {
                evt( this, e );
            }
        }

        public event EventHandler ReadingChanged;
        private void OnReadingChanged()
        {
            var evt = ReadingChanged;
            if ( evt != null )
            {
                evt( this, EventArgs.Empty );
            }
        }

        public event EventHandler Ready;
        private void OnReady()
        {
            var evt = Ready;
            if ( evt != null )
            {
                evt( this, EventArgs.Empty );
            }
        }

        public event EventHandler Stopped;
        private void OnStopped()
        {
            var evt = Stopped;
            if ( evt != null )
            {
                evt( this, EventArgs.Empty );
            }
        }

        public void Start()
        {
            forceStop = false;
            watcher.Start();
        }

        public void Stop()
        {
            forceStop = true;
            watcher.Stop();
            OnStopped();
        }

        private void Compass_ReadingChanged( Compass sender, CompassReadingChangedEventArgs e )
        {
            Course = e.Reading.HeadingTrueNorth ?? e.Reading.HeadingMagneticNorth;
            OnReadingChanged();
        }

        private void Watcher_PositionChanged( object sender, GeoPositionChangedEventArgs<GeoCoordinate> e )
        {
            Speed = e.Position.Location.Speed * 3.6; // m/s -> km/h
            if ( double.IsNaN( Speed ) )
            {
                Speed = 0;
            }
            Position = e.Position.Location;
            if ( compass == null )
            {
                Course = e.Position.Location.Course;
            }
            OnReadingChanged();
        }

        private void Watcher_StatusChanged( object sender, GeoPositionStatusChangedEventArgs e )
        {
            OnGeoStatusChanged( e );
            if ( !forceStop )
            {
                if ( e.Status == GeoPositionStatus.Ready )
                {
                    OnReady();
                }
                else
                {
                    OnStopped();
                }
            }
        }
    }
}