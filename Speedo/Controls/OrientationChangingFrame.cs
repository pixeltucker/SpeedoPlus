// Copyright (C) Microsoft Corporation. All Rights Reserved.
// This code released under the terms of the Microsoft Public License
// (Ms-PL, http://opensource.org/licenses/ms-pl.html).
// Original: http://blogs.msdn.com/b/delay/archive/2010/09/28/this-one-s-for-you-gregor-mendel-code-to-animate-and-fade-windows-phone-orientation-changes-now-supports-a-new-mode-hybrid.aspx

using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;

namespace Speedo.Controls
{
    /// <summary>
    /// PhoneApplicationFrame subclass that animates device orientation changes.
    /// </summary>
    public class OrientationChangingFrame : PhoneApplicationFrame
    {
        /// <summary>
        /// Stores the Transform used to do the rotation.
        /// </summary>
        private readonly RotateTransform _rotateTransform = new RotateTransform();

        /// <summary>
        /// Stores the Transform used for centering the visuals.
        /// </summary>
        private readonly TranslateTransform _translateTransform = new TranslateTransform();

        /// <summary>
        /// Stores the Storyboard used to change the Progress value.
        /// </summary>
        private readonly Storyboard _progressStoryboard = new Storyboard();

        /// <summary>
        /// Stores the Animation used to change the Progress value.
        /// </summary>
        private readonly DoubleAnimation _progressAnimation = new DoubleAnimation();

        /// <summary>
        /// Stores the "from" state.
        /// </summary>
        private readonly OrientationState _from = new OrientationState();

        /// <summary>
        /// Stores the "to" state.
        /// </summary>
        private readonly OrientationState _to = new OrientationState();

        /// <summary>
        /// Stores a reference to the "ClientArea" template part.
        /// </summary>
        private UIElement _clientArea;

        /// <summary>
        /// Stores the last computed size from OnProgressChanged.
        /// </summary>
        private Size _lastSize;

        /// <summary>
        /// Stores a flag indicating whether the SizeChanged event has been handled yet.
        /// </summary>
        private bool _handledSizeChanged;

        /// <summary>
        /// Initializes a new instance of the AnimateOrientationChangesFrame class.
        /// </summary>
        public OrientationChangingFrame()
        {
            // Find existing "offset transform" and take it over (if possible) to support SIP raise/lower
            var transformGroup = new TransformGroup();
            var oldTransformGroup = RenderTransform as TransformGroup;
            if ( ( null != oldTransformGroup ) && ( 3 <= oldTransformGroup.Children.Count ) )
            {
                var offsetTransform = oldTransformGroup.Children[0] as TranslateTransform;
                if ( null != offsetTransform )
                {
                    transformGroup.Children.Add( offsetTransform );
                }
            }
            // Add custom transforms
            transformGroup.Children.Add( _rotateTransform );
            transformGroup.Children.Add( _translateTransform );
            // Replace existing transform(s)
            RenderTransform = transformGroup;

            // Set up animation
            _progressAnimation.From = 0;
            _progressAnimation.To = 1;
            Storyboard.SetTarget( _progressAnimation, this );
            Storyboard.SetTargetProperty( _progressAnimation, new PropertyPath( "Progress" ) );
            _progressStoryboard.Children.Add( _progressAnimation );

            // Initialize variables
            EasingFunction = new QuarticEase(); // Initialized here to avoid a single shared instance

            // Hook events
            SizeChanged += new SizeChangedEventHandler( HandleSizeChanged );
            OrientationChanged += new EventHandler<OrientationChangedEventArgs>( HandleOrientationChanged );
        }

        /// <summary>
        /// Gets or sets a value indicating whether animation is enabled.
        /// </summary>
        public bool IsAnimationEnabled
        {
            get { return (bool) GetValue( IsAnimationEnabledProperty ); }
            set { SetValue( IsAnimationEnabledProperty, value ); }
        }
        /// <summary>
        /// Identifies the IsAnimationEnabled DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty IsAnimationEnabledProperty =
            DependencyProperty.Register( "IsAnimationEnabled", typeof( bool ), typeof( OrientationChangingFrame ), new PropertyMetadata( true ) );

        /// <summary>
        /// Gets or sets a value indicating the duration of the orientation change animation.
        /// </summary>
        public TimeSpan Duration
        {
            get { return (TimeSpan) GetValue( DurationProperty ); }
            set { SetValue( DurationProperty, value ); }
        }
        /// <summary>
        /// Identifies the Duration DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register( "Duration", typeof( TimeSpan ), typeof( OrientationChangingFrame ), new PropertyMetadata( TimeSpan.FromSeconds( 0.5 ) ) );

        /// <summary>
        /// Gets or sets a value indicating the IEasingFunction to use for the orientation change animation.
        /// </summary>
        public IEasingFunction EasingFunction
        {
            get { return (IEasingFunction) GetValue( EasingFunctionProperty ); }
            set { SetValue( EasingFunctionProperty, value ); }
        }
        /// <summary>
        /// Identifies the EasingFunction DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty EasingFunctionProperty =
            DependencyProperty.Register( "EasingFunction", typeof( IEasingFunction ), typeof( OrientationChangingFrame ), new PropertyMetadata( null ) );

        /// <summary>
        /// Identifies the Progress DependencyProperty.
        /// </summary>
        private static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register( "Progress", typeof( double ), typeof( OrientationChangingFrame ), new PropertyMetadata( 0.0, OnProgressChanged ) );
        /// <summary>
        /// Handles changes to the Progress property.
        /// </summary>
        /// <param name="o">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnProgressChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
        {
            ( (OrientationChangingFrame) o ).OnProgressChanged(/*(double)e.OldValue,*/ (double) e.NewValue );
        }
        /// <summary>
        /// Handles changes to the Progress property.
        /// </summary>
        /// <param name="newValue">New value.</param>
        private void OnProgressChanged(/*double oldValue,*/ double newValue )
        {
            // Update rotation
            _rotateTransform.Angle = _from.Angle + ( newValue * ( _to.Angle - _from.Angle ) );
            // Update translation (to center things)
            var width = _from.Width + ( newValue * ( _to.Width - _from.Width ) );
            var height = _from.Height + ( newValue * ( _to.Height - _from.Height ) );
            var transformBounds = _rotateTransform.TransformBounds( new Rect( 0, 0, width, height ) );
            _translateTransform.X = ( ( ActualWidth - transformBounds.Width ) / 2 ) - transformBounds.Left;
            _translateTransform.Y = ( ( ActualHeight - transformBounds.Height ) / 2 ) - transformBounds.Top;
            // Invalidate only if size has changed
            var size = new Size( Math.Round( width ), Math.Round( height ) );
            if ( size != _lastSize )
            {
                _lastSize = size;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Called when the element's Template changes.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            // Get the template part
            _clientArea = base.GetTemplateChild( "ClientArea" ) as UIElement;
        }

        /// <summary>
        /// Handles the SizeChanged event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void HandleSizeChanged( object sender, SizeChangedEventArgs e )
        {
            // Capture new size
            _from.Width = e.NewSize.Width;
            _from.Height = e.NewSize.Height;
            _lastSize = e.NewSize;

            // Measure/Arrange can be called before the SizeChanged event fires
            InvalidateMeasure();

            // Record the method call and re-run any "early" orientation changes
            _handledSizeChanged = true;
            HandleOrientationChanged( null, new OrientationChangedEventArgs( Orientation ) );
        }

        /// <summary>
        /// Handles the OrientationChanged event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        [SuppressMessage( "Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "PageOrientation", Justification = "Spelled correctly." )]
        private void HandleOrientationChanged( object sender, OrientationChangedEventArgs e )
        {
            if ( !_handledSizeChanged )
            {
                // ActualWidth, ActualHeight, and _lastSize aren't valid yet
                return;
            }

            // Capture current/before angle and size
            _from.Angle = _rotateTransform.Angle;
            _from.Width = _lastSize.Width;
            _from.Height = _lastSize.Height;
            _progressStoryboard.Stop();

            // Determine new angle
            switch ( e.Orientation )
            {
                case PageOrientation.PortraitUp:
                    _to.Angle = 0;
                    break;
                case PageOrientation.LandscapeLeft:
                    _to.Angle = 90;
                    break;
                case PageOrientation.LandscapeRight:
                    _to.Angle = -90;
                    break;
                case PageOrientation.PortraitDown:
                    _to.Angle = 180;
                    break;
                default:
                    throw new NotSupportedException( "Unknown PageOrientation value." );
            }

            // Determine new size
            var actualWidth = ActualWidth;
            var actualHeight = ActualHeight;
            var toPortrait = ( 0 == ( _to.Angle % 180 ) );
            _to.Width = toPortrait ? actualWidth : actualHeight;
            _to.Height = toPortrait ? actualHeight : actualWidth;

            if ( IsAnimationEnabled && ( null != sender ) )
            {
                // Animate the rotation
                _progressAnimation.Duration = Duration;
                _progressAnimation.EasingFunction = EasingFunction;
                _progressStoryboard.Begin();
            }
            else
            {
                // Snap to the rotation (with guaranteed property change)
                SetValue( ProgressProperty, 0.0 );
                SetValue( ProgressProperty, 1.0 );
            }
        }

        /// <summary>
        /// Handles measuring the children of this element.
        /// </summary>
        /// <param name="availableSize">Available size.</param>
        /// <returns>Desired size.</returns>
        protected override Size MeasureOverride( Size availableSize )
        {
            if ( null != _clientArea )
            {
                // Adjust measure size to transition size
                var newValue = (double) GetValue( ProgressProperty );
                var width = _from.Width + ( newValue * ( _to.Width - _from.Width ) );
                var height = _from.Height + ( newValue * ( _to.Height - _from.Height ) );
                _clientArea.Measure( new Size( width, height ) );
            }
            // Return default size
            return availableSize;
        }

        /// <summary>
        /// Handles arranging the children of this element.
        /// </summary>
        /// <param name="finalSize">Size to arrange to.</param>
        /// <returns>Used size.</returns>
        protected override Size ArrangeOverride( Size finalSize )
        {
            if ( null != _clientArea )
            {
                // Adjust arrange size to transition size
                var newValue = (double) GetValue( ProgressProperty );
                var width = _from.Width + ( newValue * ( _to.Width - _from.Width ) );
                var height = _from.Height + ( newValue * ( _to.Height - _from.Height ) );
                _clientArea.Arrange( new Rect( 0, 0, width, height ) );
            }
            // Return default size
            return finalSize;
        }

        /// <summary>
        /// Stores state variables for orientation changes.
        /// </summary>
        private class OrientationState
        {
            /// <summary>
            /// Gets or sets the Width.
            /// </summary>
            public double Width { get; set; }

            /// <summary>
            /// Gets or sets the Height.
            /// </summary>
            public double Height { get; set; }

            /// <summary>
            /// Gets or sets the Angle.
            /// </summary>
            public double Angle { get; set; }
        }
    }
}
