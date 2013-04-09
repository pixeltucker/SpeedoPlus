// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Solal Pirelli

using System;
using System.ComponentModel;
using System.Device.Location;
using System.Runtime.CompilerServices;
using Windows.Devices.Sensors;

namespace Speedo
{
    public sealed class MovementSource : INotifyPropertyChanged
    {
        private readonly Compass compass;
        private readonly IGeoPositionWatcher<GeoCoordinate> watcher;
        private bool forceStop;

        private double speed;
        public double Speed
        {
            get { return speed; }
            set { SetProperty( ref speed, value ); }
        }

        private GeoCoordinate position;
        public GeoCoordinate Position
        {
            get { return position; }
            set { SetProperty( ref position, value ); }
        }

        private double course;
        public double Course
        {
            get { return course; }
            set { SetProperty( ref course, value ); }
        }

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
#if DEBUG
                watcher = new FakeGeoPositionWatcher( 0.0, 0.0 );
#else
                watcher = new GeoCoordinateWatcher( GeoPositionAccuracy.High );
#endif
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

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void SetProperty<T>( ref T field, T value, [CallerMemberName] string propertyName = "" )
        {
            NotifyHelper.SetProperty( ref field, value, propertyName, this, PropertyChanged );
        }
        #endregion
    }
}