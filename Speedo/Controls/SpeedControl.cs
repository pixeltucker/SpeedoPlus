// new

using System;
using System.Device.Location;
using System.Windows;

namespace Speedo.Controls
{
    public abstract class SpeedControl : ObservableControl
    {
        #region Source DependencyProperty
        public SpeedSource Source
        {
            get { return (SpeedSource) GetValue( SourceProperty ); }
            set { SetValue( SourceProperty, value ); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register( "Source", typeof( SpeedSource ), typeof( SpeedControl ), new PropertyMetadata( OnSourcePropertyChanged ) );

        private static void OnSourcePropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var control = (SpeedControl) obj;
            if ( args.OldValue != null )
            {
                var source = (SpeedSource) args.OldValue;
                source.SpeedChanged -= control.Source_SpeedChanged;
                source.Cleared -= control.Source_Cleared;
            }
            if ( args.NewValue != null )
            {
                var source = (SpeedSource) args.NewValue;
                source.SpeedChanged += control.Source_SpeedChanged;
                source.Cleared += control.Source_Cleared;
            }
            control.Clear();
        }
        #endregion

        #region Unit DependencyProperty
        public SpeedUnit Unit
        {
            get { return (SpeedUnit) GetValue( UnitProperty ); }
            set { SetValue( UnitProperty, value ); }
        }

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register( "Unit", typeof( SpeedUnit ), typeof( SpeedControl ), new PropertyMetadata( OnUnitPropertyChanged ) );

        private static void OnUnitPropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var control = (SpeedControl) obj;
            double factor = SpeedUtils.GetFactor( (SpeedUnit) args.NewValue ) / SpeedUtils.GetFactor( (SpeedUnit) args.OldValue );
            control.ChangeUnits( factor );
        }
        #endregion

        protected abstract void ChangeUnits( double factor );
        protected abstract void ChangeSpeed( double speed, GeoCoordinate position );
        protected abstract void Clear();

        private void Source_SpeedChanged( object sender, SpeedEventArgs e )
        {
            ChangeSpeed( e.CurrentSpeed, e.CurrentPosition );
        }

        private void Source_Cleared( object sender, EventArgs e )
        {
            Clear();
        }
    }
}