// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Solal Pirelli

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Speedo.Controls
{
    // A grid that only displays its content for a short period of time when clicked.
    public sealed class PhantomGrid : Grid
    {
        private const int Timeout = 2000; // milliseconds
        private const int AnimationDuration = 200; // milliseconds

        private int hitCount;
        private Storyboard hideStoryboard;
        private Storyboard showStoryboard;

        public PhantomGrid()
        {
            hitCount = 0;
            CreateStoryboards();
            MouseLeftButtonDown += This_MouseLeftButtonDown;

            IsHitTestVisible = true;
            Background = new SolidColorBrush( Colors.Transparent );

            Opacity = 0;
        }

        private void CreateStoryboards()
        {
            var showAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds( AnimationDuration ),
            };
            Storyboard.SetTarget( showAnimation, this );
            Storyboard.SetTargetProperty( showAnimation, new PropertyPath( "Opacity" ) );
            showStoryboard = new Storyboard();
            showStoryboard.Children.Add( showAnimation );

            var hideAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds( AnimationDuration ),
            };
            Storyboard.SetTarget( hideAnimation, this );
            Storyboard.SetTargetProperty( hideAnimation, new PropertyPath( "Opacity" ) );
            hideStoryboard = new Storyboard();
            hideStoryboard.Children.Add( hideAnimation );
        }

        private async void This_MouseLeftButtonDown( object sender, MouseEventArgs e )
        {
            hitCount++;
            int oldCount = hitCount;

            showStoryboard.Begin();

            await Task.Delay( Timeout );

            // make sure the user hasn't clicked again
            if ( oldCount == hitCount )
            {
                hideStoryboard.Begin();
            }
        }
    }
}