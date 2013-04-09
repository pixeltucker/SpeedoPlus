// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Solal Pirelli

using System.Collections.Generic;
using System.Linq;

namespace Speedo.Controls
{
    public partial class DirectionDisplay : MovementControl
    {
        private static readonly Dictionary<double, string> Directions = new Dictionary<double, string>
        {
            { 0, "N" },
            { 22.5, "NE" },
            { 67.5, "E" },
            { 112.5, "SE" },
            { 157.5, "S" },
            { 202.5, "SW" },
            { 247.5, "W" },
            { 292.5, "NW" },
            { 337.5, "N" }
        };

        private double directionAngle;
        public double DirectionAngle
        {
            get { return directionAngle; }
            set { SetProperty( ref directionAngle, value ); }
        }

        private string directionInitials;
        public string DirectionInitials
        {
            get { return directionInitials; }
            set { SetProperty( ref directionInitials, value ); }
        }

        public DirectionDisplay()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;

            DirectionAngle = 0;
            DirectionInitials = Directions.First().Value;
        }

        protected override void ChangeCourse( double course )
        {
            DirectionAngle = -course;
            DirectionInitials = Directions.Last( p => course >= p.Key ).Value;
        }
    }
}
