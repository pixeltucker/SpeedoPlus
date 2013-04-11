// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Solal Pirelli

using System;
using System.ComponentModel;
using System.Device.Location;
using System.Windows;

namespace Speedo.Controls
{
    public abstract class MovementControl : ObservableControl
    {
        #region Source DependencyProperty
        public MovementSource Source
        {
            get { return (MovementSource) GetValue( SourceProperty ); }
            set { SetValue( SourceProperty, value ); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register( "Source", typeof( MovementSource ), typeof( MovementControl ), new PropertyMetadata( OnSourcePropertyChanged ) );

        private static void OnSourcePropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var control = (MovementControl) obj;
            if ( args.OldValue != null )
            {
                var source = (MovementSource) args.OldValue;
                source.PropertyChanged -= control.Source_PropertyChanged;
                source.Ready -= control.Source_Ready;
                source.Stopped -= control.Source_Stopped;
            }
            if ( args.NewValue != null )
            {
                var source = (MovementSource) args.NewValue;
                source.PropertyChanged += control.Source_PropertyChanged;
                source.Ready += control.Source_Ready;
                source.Stopped += control.Source_Stopped;
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
            DependencyProperty.Register( "Unit", typeof( SpeedUnit ), typeof( MovementControl ), new PropertyMetadata( OnUnitPropertyChanged ) );

        private static void OnUnitPropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var control = (MovementControl) obj;
            double factor = SpeedUtils.GetFactor( (SpeedUnit) args.NewValue ) / SpeedUtils.GetFactor( (SpeedUnit) args.OldValue );
            control.ChangeUnits( factor );
        }
        #endregion

        public bool IsReady { get; private set; }

        protected virtual void ChangeUnits( double factor ) { }
        protected virtual void ChangeSpeed( double speed ) { }
        protected virtual void ChangePosition( GeoCoordinate position ) { }
        protected virtual void ChangeCourse( double course ) { }
        protected virtual void IsReadyChanged() { }
        protected virtual void Clear() { }

        private void Source_PropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            Dispatcher.BeginInvoke( () =>
            {
                if ( e.PropertyName == "Speed" )
                {
                    ChangeSpeed( Source.Speed );
                }
                else if ( e.PropertyName == "Position" )
                {
                    ChangePosition( Source.Position );
                }
                else if ( e.PropertyName == "Course" )
                {
                    ChangeCourse( Source.Course );
                }
            } );
        }

        private void Source_Ready( object sender, EventArgs e )
        {
            IsReady = true;
            IsReadyChanged();
            Clear();
        }

        private void Source_Stopped( object sender, EventArgs e )
        {
            IsReady = false;
            IsReadyChanged();
            Clear();
        }
    }
}