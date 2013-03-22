using System;
using System.Device.Location;
using System.Windows;

namespace Speedo.Controls
{
    public partial class DistanceDisplay : ObservableControl
    {
        #region Source DependencyProperty
        public SpeedSource Source
        {
            get { return (SpeedSource) GetValue( SourceProperty ); }
            set { SetValue( SourceProperty, value ); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register( "Source", typeof( SpeedSource ), typeof( DistanceDisplay ), new PropertyMetadata( OnSourcePropertyChanged ) );

        private static void OnSourcePropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var display = (DistanceDisplay) obj;
            if ( args.OldValue != null )
            {
                var source = (SpeedSource) args.OldValue;
                source.SpeedChanged -= display.Source_SpeedChanged;
                source.Cleared -= display.Source_Cleared;
            }
            if ( args.NewValue != null )
            {
                var source = (SpeedSource) args.NewValue;
                source.SpeedChanged += display.Source_SpeedChanged;
                source.Cleared += display.Source_Cleared;
            }
            display.Clear();
        }
        #endregion

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

        private void Clear()
        {
            Distance = 0;
        }

        private void Source_SpeedChanged( object sender, SpeedEventArgs e )
        {
            if ( lastPosition != null )
            {
                Distance += lastPosition.GetDistanceTo( e.CurrentPosition );
            }
            lastPosition = e.CurrentPosition;
        }

        private void Source_Cleared( object sender, EventArgs e )
        {
            Clear();
        }
    }
}