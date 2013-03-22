// new

using System;
using System.Windows;

namespace Speedo.Controls
{
    public partial class SpeedDisplay : ObservableControl
    {
        #region Source DependencyProperty
        public SpeedSource Source
        {
            get { return (SpeedSource) GetValue( SourceProperty ); }
            set { SetValue( SourceProperty, value ); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register( "Source", typeof( SpeedSource ), typeof( SpeedDisplay ), new PropertyMetadata( OnSourcePropertyChanged ) );

        private static void OnSourcePropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var display = (SpeedDisplay) obj;
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

        #region Unit DependencyProperty
        public SpeedUnit Unit
        {
            get { return (SpeedUnit) GetValue( UnitProperty ); }
            set { SetValue( UnitProperty, value ); }
        }

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register( "Unit", typeof( SpeedUnit ), typeof( SpeedDisplay ), new PropertyMetadata( OnUnitPropertyChanged ) );

        private static void OnUnitPropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            ( (SpeedDisplay) obj ).ChangeUnits( (SpeedUnit) args.OldValue, (SpeedUnit) args.NewValue );
        }
        #endregion

        private int dataCount;

        private double average;
        public double Average
        {
            get { return average; }
            set { SetProperty( ref average, value ); }
        }

        private double max;
        public double Max
        {
            get { return max; }
            set { SetProperty( ref max, value ); }
        }

        private double current;
        public double Current
        {
            get { return current; }
            set { SetProperty( ref current, value ); }
        }

        public SpeedDisplay()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }

        private void Clear()
        {
            dataCount = 0;
            Average = Max = Current = 0;
        }

        private void ChangeUnits( SpeedUnit oldUnit, SpeedUnit newUnit )
        {
            double factor = SpeedUtils.GetFactor( newUnit ) / SpeedUtils.GetFactor( oldUnit );
            Average *= factor;
            Max *= factor;
            Current *= factor;
        }

        private void Source_Cleared( object sender, EventArgs e )
        {
            Clear();
        }

        private void Source_SpeedChanged( object sender, SpeedEventArgs e )
        {
            int speed = (int) Math.Round( e.CurrentSpeed * SpeedUtils.GetFactor( Unit ) );
            Average = ( Average * dataCount + speed ) / ( dataCount + 1 );
            Max = Math.Max( Max, speed );
            Current = speed;

            dataCount++;
        }
    }
}