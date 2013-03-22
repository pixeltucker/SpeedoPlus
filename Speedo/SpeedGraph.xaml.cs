using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Speedo
{
    public partial class SpeedGraph : UserControl
    {
        #region Source DependencyProperty
        public SpeedSource Source
        {
            get { return (SpeedSource) GetValue( SourceProperty ); }
            set { SetValue( SourceProperty, value ); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register( "Source", typeof( SpeedSource ), typeof( SpeedGraph ), new PropertyMetadata( OnSourcePropertyChanged ) );

        private static void OnSourcePropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var graph = (SpeedGraph) obj;
            if ( args.OldValue != null )
            {
                var source = (SpeedSource) args.OldValue;
                source.SpeedChanged -= graph.Source_SpeedChanged;
                source.Cleared -= graph.Source_Cleared;
            }
            if ( args.NewValue != null )
            {
                var source = (SpeedSource) args.NewValue;
                source.SpeedChanged += graph.Source_SpeedChanged;
                source.Cleared += graph.Source_Cleared;
            }
        }
        #endregion

        #region PointsCount DependencyProperty
        public int PointsCount
        {
            get { return (int) GetValue( PointsCountProperty ); }
            set { SetValue( PointsCountProperty, value ); }
        }

        public static readonly DependencyProperty PointsCountProperty =
            DependencyProperty.Register( "PointsCount", typeof( int ), typeof( SpeedGraph ), new PropertyMetadata( OnPointsCountPropertyChanged ) );

        private static void OnPointsCountPropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            ( (SpeedGraph) obj ).Clear();
        }
        #endregion

        public PointCollection Points { get; private set; }

        public SpeedGraph()
        {
            Points = new PointCollection();
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }

        private void AddSpeed( double speed )
        {
            Points.RemoveAt( 0 );
            for ( int n = 0; n < Points.Count; n++ )
            {
                Points[n] = new Point( n, Points[n].Y );
            }
            Points.Add( new Point( PointsCount, -Math.Round( speed ) ) );
        }

        private void Clear()
        {
            Points.Clear();
            for ( int n = 0; n < PointsCount; n++ )
            {
                Points.Add( new Point( n, 0 ) );
            }
        }

        private void Source_SpeedChanged( object sender, SpeedEventArgs e )
        {
            AddSpeed( e.CurrentSpeed );
        }

        private void Source_Cleared( object sender, EventArgs e )
        {
            Clear();
        }
    }
}