// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Solal Pirelli

using System;
using System.Device.Location;
using System.Windows;
using Microsoft.Phone.Controls;

namespace Speedo.Controls
{
    public partial class BackgroundMap : MovementControl
    {
        #region Status DependencyProperty
        public MapStatus Status
        {
            get { return (MapStatus) GetValue( StatusProperty ); }
            set { SetValue( StatusProperty, value ); }
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register( "Status", typeof( MapStatus ), typeof( BackgroundMap ), new PropertyMetadata( OnStatusPropertyChanged ) );

        private static void OnStatusPropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            ( (BackgroundMap) obj ).SetIsEnabled();
        }
        #endregion

        private GeoCoordinate position;
        public GeoCoordinate Position
        {
            get { return position; }
            set { SetProperty( ref position, value ); }
        }

        private double zoom;
        public double Zoom
        {
            get { return zoom; }
            set { SetProperty( ref zoom, value ); }
        }

        private double mapRotation;
        public double MapRotation
        {
            get { return mapRotation; }
            set { SetProperty( ref mapRotation, value ); }
        }

        private double oldZoom;

        public BackgroundMap()
        {
            Zoom = 16;
            Position = GeoCoordinate.Unknown;
            InitializeComponent();
            LayoutRoot.DataContext = this;
            SetIsEnabled();
        }

        private void SetIsEnabled()
        {
            IsEnabled = IsReady && Status == MapStatus.On;
        }

        protected override void ChangePosition( GeoCoordinate position )
        {
            Position = position;
        }

        protected override void IsReadyChanged()
        {
            SetIsEnabled();
        }

        // I don't think there's a simpler way to disable panning on a map
        // rolling out an EventToCommand behavior seems a bit much

        private void GestureListener_PinchStarted( object sender, PinchStartedGestureEventArgs e )
        {
            oldZoom = Zoom;
        }

        private void GestureListener_PinchDelta( object sender, PinchGestureEventArgs e )
        {
            double desiredZoom = oldZoom * Math.Pow( e.DistanceRatio, 0.5 );
            Zoom = Math.Max( 1, Math.Min( desiredZoom, 19 ) ); // clip between 1 and 19
        }

		private void Map_Loaded(object sender, RoutedEventArgs e)
		{
			Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = "d2075a58-c9a4-e011-986b-78e7d1fa76f8";
			Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = "st0QyPY2wXv8S8323SVY4Q";
		}
    }
}