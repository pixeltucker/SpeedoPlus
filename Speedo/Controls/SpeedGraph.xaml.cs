// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Long Zheng, Solal Pirelli

using System;
using System.Windows;
using System.Windows.Media;

namespace Speedo.Controls
{
    public partial class SpeedGraph : MovementControl
    {
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

        protected override void ChangeSpeed( double speed )
        {
            Points.RemoveAt( 0 );
            for ( int n = 0; n < Points.Count; n++ )
            {
                Points[n] = new Point( n, Points[n].Y );
            }
            Points.Add( new Point( PointsCount, -Math.Round( speed ) ) );
        }

        protected override void Clear()
        {
            Points.Clear();
            for ( int n = 0; n < PointsCount; n++ )
            {
                Points.Add( new Point( n, 0 ) );
            }
        }
    }
}