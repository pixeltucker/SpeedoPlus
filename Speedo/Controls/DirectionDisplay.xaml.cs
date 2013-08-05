// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Solal Pirelli

using System.Collections.Generic;
using System.Linq;
using Microsoft.Phone.Controls;

namespace Speedo.Controls
{
    public partial class DirectionDisplay : MovementControl
    {
        private static readonly Dictionary<double, Direction> Directions = new Dictionary<double, Direction>
        {
            { 0, Direction.North },
            { 22.5, Direction.NorthEast },
            { 67.5, Direction.East },
            { 112.5, Direction.SouthEast },
            { 157.5, Direction.South },
            { 202.5, Direction.SouthWest },
            { 247.5, Direction.West },
            { 292.5, Direction.NorthWest },
            { 337.5, Direction.North }
        };

        private static readonly Dictionary<PageOrientation, double> OrientationAngles = new Dictionary<PageOrientation, double>
        {
            { PageOrientation.PortraitUp, 0 },
            { PageOrientation.LandscapeRight, 90 },
            { PageOrientation.PortraitDown, 180 }, // not currently used
            { PageOrientation.LandscapeLeft, 270 }
        };

        private double directionAngle;
        public double DirectionAngle
        {
            get { return directionAngle; }
            set { SetProperty( ref directionAngle, value ); }
        }

        private double orientationFixAngle;
        public double OrientationFixAngle
        {
            get { return orientationFixAngle; }
            set { SetProperty( ref orientationFixAngle, value ); }
        }

        private Direction direction;
        public Direction Direction
        {
            get { return direction; }
            set { SetProperty( ref direction, value ); }
        }

        public DirectionDisplay()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;

            DirectionAngle = 0;
            Direction = Directions.First().Value;

            App.Current.RootFrame.OrientationChanged += RootFrame_OrientationChanged;
        }

        private void RootFrame_OrientationChanged( object sender, OrientationChangedEventArgs e )
        {
            OrientationFixAngle = OrientationAngles.First( p => e.Orientation.HasFlag( p.Key ) ).Value;
        }

        protected override void ChangeCourse( double course )
        {
            DirectionAngle = -course;
            Direction = Directions.Last( p => course >= p.Key ).Value;
        }
    }
}
