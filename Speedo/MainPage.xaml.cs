﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Device.Location;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Speedo.Controls;

namespace Speedo
{
    public partial class MainPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        public SpeedSource SpeedSource { get; private set; }
        public bool ShowSpeedGraph { get; private set; }

        private bool allowLocationAccess;
        public bool AllowLocationAccess
        {
            get { return allowLocationAccess; }
            set { SetProperty( ref allowLocationAccess, value ); }
        }

        private bool isLocating;
        public bool IsLocating
        {
            get { return isLocating; }
            set { SetProperty( ref isLocating, value ); }
        }

        private string switchUnitsText;
        public string SwitchUnitsText
        {
            get { return switchUnitsText; }
            set { SetProperty( ref switchUnitsText, value ); }
        }

        private string switchLocationAccessText;
        public string SwitchLocationAccessText
        {
            get { return switchLocationAccessText; }
            set { SetProperty( ref switchLocationAccessText, value ); }
        }

        public ICommand SwitchUnitsCommand { get; private set; }
        public ICommand SwitchLocationAccessCommand { get; private set; }
        public ICommand AboutCommand { get; private set; }

        public LengthUnit lengthUnit;
        private double currentSpeed = 0;
        private double currentCourse = 0;
        private double maxSpeed = 0;
        private List<double> prevSpeeds = new List<double>();
        private double avgSpeed = 0;
        private double accuracy = 0;
        private double distance = 0;
        private double mapScale = 17;
        private GeoCoordinate lastKnownPos;
        private bool windscreenMode = false;
        private MapStatus mapStatus;
        private IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        private bool darkTheme = (Visibility) Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible;
        private int speedGraphMaxCount = 360;
        private bool errorMap = false;
        private bool isSpeedAlertEnabled;
        private string speedAlertSpeedConfig;
        private double speedAlertSpeed;
        private SoundEffect speedAlertEffect;
        private DispatcherTimer speedAlertTimer = new DispatcherTimer();
        private int settingsDisplayCount;
        private GeoCoordinateWatcher watcher;

        // Constructor
        public MainPage()
        {
            SpeedSource = new SpeedSource();
            ShowSpeedGraph = true;
            DataContext = this;

            IsLocating = true;

            SwitchUnitsCommand = new RelayCommand( ExecuteSwitchUnitsCommand );
            SwitchLocationAccessCommand = new RelayCommand( ExecuteSwitchLocationAccessCommand );
            AboutCommand = new RelayCommand( ExecuteAboutCommand );

            settings.Clear();

            InitializeComponent();
            ShowWarnings();

            // no idling here
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

            // get our unit setting        
            if ( !settings.TryGetValue<LengthUnit>( "unit", out lengthUnit ) )
            {
                // get our region
                if ( CultureInfo.CurrentCulture.Name == "en-US" )
                {
                    // this is English US, default to miles
                    lengthUnit = LengthUnit.Miles;
                }
                else
                {
                    // elsewhere, default to KM
                    lengthUnit = LengthUnit.Kilometers;
                }
            }
            UpdateUnit();

            SpeedSource.Clear();

            // update all the speed text
            UpdateSpeed();

            // hide settings button
            VisualStateManager.GoToState( this, "HideControls", false );

            // initialize Bing map with our API
            BackMap.ZoomLevel = mapScale;

            // get our map setting
            if ( !settings.TryGetValue<MapStatus>( "map", out mapStatus ) )
            {
                settings["map"] = mapStatus = MapStatus.On;
            }
            UpdateMap();

            // get our speed alert speed setting
            if ( !settings.TryGetValue<bool>( "SpeedAlertConfig", out isSpeedAlertEnabled ) )
            {
                settings["SpeedAlertConfig"] = isSpeedAlertEnabled = false;
            }
            UpdateSpeedAlert();

            settings.TryGetValue<string>( "SpeedAlertSpeedConfig", out speedAlertSpeedConfig );
            if ( speedAlertSpeedConfig == null )
            {
                // get our region
                if ( CultureInfo.CurrentCulture.Name == "en-US" )
                {
                    // this is English US, default to 65
                    speedAlertSpeed = 65;
                }
                else
                {
                    // elsewhere, default to 80
                    speedAlertSpeed = 100;
                }
            }
            else
            {
                speedAlertSpeed = Convert.ToDouble( speedAlertSpeedConfig );
            }

            var alertStream = TitleContainer.OpenStream( "Resources/alert.wav" );
            speedAlertEffect = SoundEffect.FromStream( alertStream );
            FrameworkDispatcher.Update();
            speedAlertTimer.Tick += ( s, e ) =>
            {
                speedAlertEffect.Play();
            };
            speedAlertTimer.Interval = TimeSpan.FromSeconds( 5 );

            // get our location setting
            if ( !settings.TryGetValue<bool>( "LocationAccess", out allowLocationAccess ) )
            {
                settings["LocationAccess"] = AllowLocationAccess = true;

            }
            if ( AllowLocationAccess )
            {
                watcher = new GeoCoordinateWatcher( GeoPositionAccuracy.High );
                watcher.StatusChanged += Watcher_StatusChanged;
                watcher.PositionChanged += Watcher_PositionChanged;
            }
            UpdateLocationAccess();
        }

        private void ShowWarnings()
        {
            bool warning;
            settings.TryGetValue<bool>( "warning", out warning );
            if ( !warning )
            {
                MessageBox.Show( "This software uses GPS signals to calculate your speed and direction which is subject to interference and results may be skewed.\n\nThe information provided can only be used as a guide.", "Accuracy warning", MessageBoxButton.OK );
                MessageBox.Show( "This software temporarily stores and uses your location data for the purpose of calculating speed and direction.\n\nYour location may be sent to Bing over the internet to position the map.", "Location privacy statement", MessageBoxButton.OK );
                settings["warning"] = true;
            }
        }

        private void PositionChanged( GeoPositionChangedEventArgs<GeoCoordinate> e )
        {
            // store horizontal GPS accuracy
            accuracy = e.Position.Location.HorizontalAccuracy;

            // update only if accuracy <= 70m
            if ( accuracy <= 70 )
            {
                StatusTextBlock.Text = "";
                if ( Double.IsNaN( e.Position.Location.Speed ) )
                {
                    currentSpeed = 0;
                }
                else
                {
                    currentSpeed = e.Position.Location.Speed * 3.6; // convert M/s into KM/h
                    currentCourse = e.Position.Location.Course;
                    UpdateAverage();
                }

                if ( Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Emulator )
                {
                    StatusTextBlock.Text = "Emulator mode";
                    if ( lastKnownPos != null )
                    {
                        currentSpeed = ( e.Position.Location.GetDistanceTo( lastKnownPos ) );
                        if ( currentSpeed > 0 )
                        {
                            var lat1 = Math.PI * lastKnownPos.Latitude / 180.0;
                            var lat2 = Math.PI * e.Position.Location.Latitude / 180.0;
                            var dLon = Math.PI * e.Position.Location.Longitude / 180.0 - Math.PI * lastKnownPos.Longitude / 180.0;
                            var y = Math.Sin( dLon ) * Math.Cos( lat2 );
                            var x = Math.Cos( lat1 ) * Math.Sin( lat2 ) - Math.Sin( lat1 ) * Math.Cos( lat2 ) * Math.Cos( dLon );
                            var brng = Math.Atan2( y, x );
                            currentCourse = ( 180.0 * brng / Math.PI + 360 ) % 360;
                        }
                    }
                    UpdateAverage();
                }

                if ( lastKnownPos != null )
                {
                    double travelled = ( e.Position.Location.GetDistanceTo( lastKnownPos ) ) / 1000;
                    distance = distance + travelled;
                }
                lastKnownPos = e.Position.Location;

            }
            else
            {
                StatusTextBlock.Text = "weak GPS signal";
                SpeedTextBlock.Text = "-";
                currentSpeed = 0;
                currentCourse = 0;
            }

            // if current speed is faster than maxspeed, override it
            if ( currentSpeed > maxSpeed )
            {
                maxSpeed = currentSpeed;
            }

            SpeedSource.ChangeSpeed( currentSpeed );
            UpdateSpeed();
            UpdateMapRender();
        }

        private void UpdateMapRender()
        {
            BackMap.Center = lastKnownPos;
            if ( !Double.IsNaN( currentCourse ) )
            {
                Storyboard BackMapSB = new Storyboard();
                DoubleAnimation BackMapAngleAnim = new DoubleAnimation();
                BackMapAngleAnim.EasingFunction = new ExponentialEase { Exponent = 6, EasingMode = EasingMode.EaseOut };
                BackMapAngleAnim.Duration = new Duration( TimeSpan.FromSeconds( 0.6 ) );
                BackMapAngleAnim.To = 360 - currentCourse;
                Storyboard.SetTarget( BackMapAngleAnim, BackMapRotateTransform );
                Storyboard.SetTargetProperty( BackMapAngleAnim, new PropertyPath( RotateTransform.AngleProperty ) );
                BackMapSB.Children.Add( BackMapAngleAnim );
                BackMapSB.Begin();
            }
        }

        private void UpdateAverage()
        {
            if ( currentSpeed > 0 )
            {
                if ( prevSpeeds.Count > 0 )
                {
                    double exponent = ( 2 / ( prevSpeeds.Count + 1 ) );
                    avgSpeed = ( currentSpeed * exponent ) + ( prevSpeeds.Average() * ( 1 - exponent ) );
                }
                else
                {
                    avgSpeed = 0;
                }

                if ( prevSpeeds.Count >= speedGraphMaxCount )
                {
                    prevSpeeds[1] = ( prevSpeeds[0] + prevSpeeds[1] ) / 2;
                    prevSpeeds.RemoveAt( 0 );
                }
                prevSpeeds.Add( currentSpeed );
            }

        }

        private void UpdateSpeed()
        {
            if ( !Double.IsNaN( currentSpeed ) )
            {
                double unitFactor = lengthUnit == LengthUnit.Miles ? 0.621371192 : 1;

                double CurrentSpeedFactored = Math.Ceiling( currentSpeed * unitFactor );
                double AvgSpeedFactored = Math.Floor( avgSpeed * unitFactor );
                double MaxSpeedFactored = Math.Ceiling( maxSpeed * unitFactor );
                double DistanceFactored = Math.Round( distance * unitFactor, 1 );

                SpeedTextBlock.Text = CurrentSpeedFactored.ToString();

                if ( this.Orientation == PageOrientation.PortraitUp || this.Orientation == PageOrientation.PortraitDown )
                {
                    if ( CurrentSpeedFactored >= 1000 )
                    {
                        SpeedTextBlock.FontSize = 180;
                    }
                    else if ( CurrentSpeedFactored >= 200 )
                    {
                        SpeedTextBlock.FontSize = 230;
                    }
                    else
                    {
                        SpeedTextBlock.FontSize = 260;
                    }
                }

                MaxSpeedTextBlock.Text = MaxSpeedFactored.ToString();
                DistanceTextBlock.Text = DistanceFactored.ToString();
                AvgSpeedTextBlock.Text = AvgSpeedFactored.ToString();

                //// only show compass if we're actually moving
                //if ( currentSpeed == 0 )
                //{
                //    //Direction.Opacity = 0;
                //}
                //else
                //{
                //    Direction.Opacity = 1;
                //    DirectionTextBlock.Text = CourseDirection( currentCourse );
                //    Storyboard DirectionSB = new Storyboard();
                //    DoubleAnimation DirectionAnim = new DoubleAnimation();
                //    DirectionAnim.EasingFunction = new ExponentialEase { Exponent = 6, EasingMode = EasingMode.EaseOut };
                //    DirectionAnim.Duration = new Duration( TimeSpan.FromSeconds( 0.6 ) );
                //    DirectionAnim.To = 360 - currentCourse;
                //    Storyboard.SetTarget( DirectionAnim, DirectionRotateTransform );
                //    Storyboard.SetTargetProperty( DirectionAnim, new PropertyPath( RotateTransform.AngleProperty ) );
                //    DirectionSB.Children.Add( DirectionAnim );
                //    DirectionSB.Begin();
                //}

                // speed alert
                if ( isSpeedAlertEnabled && CurrentSpeedFactored > speedAlertSpeed )
                {
                    if ( !speedAlertTimer.IsEnabled )
                    {
                        speedAlertEffect.Play();

                        speedAlertTimer.Start();
                    }
                }
                else
                {
                    speedAlertTimer.Stop();
                }

            }
            else
            {
                SpeedTextBlock.Text = "-";
                //Direction.Opacity = 0;
            }
        }

        private void UpdateUnit()
        {
            switch ( lengthUnit )
            {
                case LengthUnit.Kilometers:
                    SwitchUnitsText = "switch to imperial (mph)";
                    UnitTextBlock.Text = "km/h";
                    break;
                case LengthUnit.Miles:
                    SwitchUnitsText = "switch to metric (km/h)";
                    UnitTextBlock.Text = "mph";
                    break;
            }
        }

        private void UpdateWindscreen()
        {
            if ( windscreenMode )
            {
                ContentScaleTransform.ScaleX = -1;
                if ( !darkTheme )
                {
                    SolidColorBrush foregroundbrush = (SolidColorBrush) App.Current.Resources["PhoneForegroundBrush"];
                    SolidColorBrush backgroundbrush = (SolidColorBrush) App.Current.Resources["PhoneBackgroundBrush"];
                    SolidColorBrush subtlebrush = (SolidColorBrush) App.Current.Resources["PhoneSubtleBrush"];
                    SolidColorBrush disabledbrush = (SolidColorBrush) App.Current.Resources["PhoneDisabledBrush"];
                    foregroundbrush.Color = Colors.White;
                    backgroundbrush.Color = Colors.Black;
                    subtlebrush.Color = Colors.LightGray;
                    disabledbrush.Color = Colors.Gray;
                }
                SpeedTextBlock.Foreground = (SolidColorBrush) App.Current.Resources["WindscreenColor"];
                // TODO fix that
                //DirectionTextBlock.Foreground = (SolidColorBrush) App.Current.Resources["WindscreenColor"];
                //DirectionIcon.Fill = (SolidColorBrush) App.Current.Resources["WindscreenColor"];
                //SpeedChart.Visibility = Visibility.Collapsed;
                ShowSpeedGraph = false;
                UnitTextBlock.Opacity = 0;
                windsreenIndicator.Visibility = Visibility.Visible;

                MapLayer.Visibility = Visibility.Collapsed;
                BackMap.Visibility = Visibility.Collapsed;
                BackMap.IsEnabled = false;
                mapButton.IsEnabled = false;
            }
            else
            {
                ContentScaleTransform.ScaleX = 1;
                if ( !darkTheme )
                {
                    SolidColorBrush foregroundbrush = (SolidColorBrush) App.Current.Resources["PhoneForegroundBrush"];
                    SolidColorBrush backgroundbrush = (SolidColorBrush) App.Current.Resources["PhoneBackgroundBrush"];
                    SolidColorBrush subtlebrush = (SolidColorBrush) App.Current.Resources["PhoneSubtleBrush"];
                    SolidColorBrush disabledbrush = (SolidColorBrush) App.Current.Resources["PhoneDisabledBrush"];
                    foregroundbrush.Color = (System.Windows.Media.Color) App.Current.Resources["PhoneForegroundColor"];
                    backgroundbrush.Color = (System.Windows.Media.Color) App.Current.Resources["PhoneBackgroundColor"];
                    subtlebrush.Color = (System.Windows.Media.Color) App.Current.Resources["PhoneSubtleColor"];
                    disabledbrush.Color = (System.Windows.Media.Color) App.Current.Resources["PhoneDisabledColor"];
                }
                SpeedTextBlock.Foreground = (SolidColorBrush) App.Current.Resources["PhoneForegroundBrush"];
                //DirectionTextBlock.Foreground = (SolidColorBrush) App.Current.Resources["PhoneAccentBrush"];
                //DirectionIcon.Fill = (SolidColorBrush) App.Current.Resources["PhoneAccentBrush"];
                //SpeedChart.Visibility = Visibility.Visible;
                ShowSpeedGraph = true;
                UnitTextBlock.Opacity = 1;
                windsreenIndicator.Visibility = Visibility.Collapsed;
                UpdateMap();
            }
        }

        private void UpdateMap()
        {
            if ( errorMap )
            {
                mapStatus = MapStatus.Disabled;
            }

            switch ( mapStatus )
            {
                case MapStatus.On:
                    if ( !windscreenMode )
                    {
                        MapLayer.Visibility = System.Windows.Visibility.Visible;
                        BackMap.Visibility = System.Windows.Visibility.Visible;
                        BackMap.IsEnabled = true;
                        mapButton.IsEnabled = true;
                    }
                    break;
                case MapStatus.Off:
                    if ( !windscreenMode )
                    {
                        MapLayer.Visibility = System.Windows.Visibility.Collapsed;
                        BackMap.Visibility = System.Windows.Visibility.Collapsed;
                        BackMap.IsEnabled = false;
                        mapButton.IsEnabled = true;
                    }
                    break;
                case MapStatus.Disabled:
                    MapLayer.Visibility = System.Windows.Visibility.Collapsed;
                    BackMap.Visibility = System.Windows.Visibility.Collapsed;
                    BackMap.IsEnabled = false;
                    mapButton.IsEnabled = false;
                    break;
            }
        }

        private void UpdateSpeedAlert()
        {
            alertIndicator.Visibility = isSpeedAlertEnabled ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateLocationAccess()
        {
            settings["LocationAccess"] = AllowLocationAccess;
            if ( AllowLocationAccess )
            {
                SwitchLocationAccessText = "disable location access";
                watcher.Start();
            }
            else
            {
                SwitchLocationAccessText = "enable location access";
                watcher.Stop();
                StatusTextBlock.Text = "location inaccessible";
                currentSpeed = Double.NaN;
                UpdateSpeed();
                mapStatus = MapStatus.Disabled;
                UpdateMap();
            }
            settings.Save();
        }

        protected override void OnNavigatedTo( NavigationEventArgs e )
        {
            if ( PhoneApplicationService.Current.StartupMode == StartupMode.Activate )
            {
                var stateSettings = PhoneApplicationService.Current.State;
                distance = (double) stateSettings["distance"];
                maxSpeed = (double) stateSettings["MaxSpeed"];
                avgSpeed = (double) stateSettings["AvgSpeed"];
                windscreenMode = (bool) stateSettings["windscreenMode"];
                mapScale = (double) stateSettings["mapScale"];
                prevSpeeds = (List<Double>) stateSettings["prevSpeeds"];
                SpeedSource.Clear();
                foreach ( var speed in prevSpeeds )
                {
                    SpeedSource.ChangeSpeed( speed );
                }
                isSpeedAlertEnabled = (bool) stateSettings["SpeedAlertConfig"];
                UpdateWindscreen();
                UpdateSpeedAlert();
            }
        }

        protected override void OnNavigatedFrom( NavigationEventArgs e )
        {
            var stateSettings = PhoneApplicationService.Current.State;
            stateSettings["distance"] = distance;
            stateSettings["MaxSpeed"] = maxSpeed;
            stateSettings["AvgSpeed"] = avgSpeed;
            stateSettings["windscreenMode"] = windscreenMode;
            stateSettings["mapScale"] = mapScale;
            stateSettings["prevSpeeds"] = prevSpeeds;
            stateSettings["SpeedAlertConfig"] = isSpeedAlertEnabled;
        }

        protected override void OnBackKeyPress( CancelEventArgs e )
        {
            if ( PopupContent.Children.Count > 0 )
            {
                Storyboard hidePopup = (Storyboard) App.Current.Resources["SwivelOut"];
                Storyboard.SetTarget( hidePopup, PopupHost );
                hidePopup.Begin();
                hidePopup.Completed += ( s, _ ) =>
                {
                    PopupContent.Children.Clear();
                    LayoutRoot.IsHitTestVisible = true;
                    ApplicationBar.IsVisible = true;
                    hidePopup.Stop();
                };
                e.Cancel = true;
            }
        }

        protected override void OnOrientationChanged( OrientationChangedEventArgs e )
        {
            base.OnOrientationChanged( e );
            VisualStateManager.GoToState( this, e.Orientation.ToString(), true );
        }

        private void ExecuteSwitchUnitsCommand( object parameter )
        {
            var newUnit = lengthUnit == LengthUnit.Kilometers ? LengthUnit.Miles : LengthUnit.Kilometers;
            settings["unit"] = lengthUnit = newUnit;
            settings.Save();
            UpdateUnit();
            UpdateSpeed();
        }

        private void ExecuteSwitchLocationAccessCommand( object parameter )
        {
            if ( AllowLocationAccess )
            {
                var warningMsg = MessageBox.Show( "This application will not work without location access. Are you sure you still want to disable it?", "Disable location", MessageBoxButton.OKCancel );
                if ( warningMsg == MessageBoxResult.OK )
                {
                    AllowLocationAccess = false;
                    UpdateLocationAccess();
                }
            }
            else
            {
                AllowLocationAccess = true;
                UpdateLocationAccess();
            }
        }

        private void ExecuteAboutCommand( object parameter )
        {
            NavigationService.Navigate( new Uri( "/AboutPage.xaml", UriKind.Relative ) );
        }

        //private void BackMap_LoadingError( object sender, LoadingErrorEventArgs e )
        //{
        //    errorMap = true;
        //    UpdateMap();
        //}

        // Event handler for the GeoCoordinateWatcher.StatusChanged event.
        private void Watcher_StatusChanged( object sender, GeoPositionStatusChangedEventArgs e )
        {
            switch ( e.Status )
            {
                case GeoPositionStatus.Disabled:
                    // The Location Service is disabled or unsupported.
                    // Check to see whether the user has disabled the Location Service.
                    StatusTextBlock.Text = "location inaccessible";
                    IsLocating = false;
                    currentSpeed = Double.NaN;
                    UpdateSpeed();
                    mapStatus = MapStatus.Disabled;
                    UpdateMap();
                    break;

                case GeoPositionStatus.Initializing:
                    // The Location Service is initializing.
                    // Disable the Start Location button.
                    StatusTextBlock.Text = "GPS initializating";
                    IsLocating = true;
                    currentSpeed = Double.NaN;
                    UpdateSpeed();
                    mapStatus = MapStatus.Disabled;
                    UpdateMap();
                    break;

                case GeoPositionStatus.NoData:
                    // The Location Service is working, but it cannot get location data.
                    StatusTextBlock.Text = "GPS not available";
                    IsLocating = false;
                    mapStatus = MapStatus.Disabled;
                    UpdateMap();
                    break;

                case GeoPositionStatus.Ready:
                    // The Location Service is working and is receiving location data.
                    // Show the current position and enable the Stop Location button.
                    StatusTextBlock.Text = "";
                    IsLocating = false;
                    settings.TryGetValue<MapStatus>( "map", out mapStatus );
                    UpdateMap();
                    break;
            }
        }

        private void Watcher_PositionChanged( object sender, GeoPositionChangedEventArgs<GeoCoordinate> e )
        {
            BackMap.Center = e.Position.Location;
        }

        private void ResetTrip_Click( object sender, EventArgs e )
        {
            distance = 0;
            maxSpeed = 0;
            avgSpeed = 0;
            SpeedSource.Clear();
            prevSpeeds.Clear();
            UpdateAverage();
            SpeedSource.ChangeSpeed( currentSpeed );
            UpdateSpeed();
        }

        private async void LayoutRoot_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
        {
            settingsDisplayCount++;
            int oldCount = settingsDisplayCount;
            VisualStateManager.GoToState( this, "ShowControls", true );

            await Task.Delay( 2000 );

            if ( settingsDisplayCount == oldCount ) // the user didn't touch again
            {
                VisualStateManager.GoToState( this, "HideControls", true );
            }
        }

        private void WindscreenButton_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
        {
            windscreenMode = !windscreenMode;
            UpdateWindscreen();
        }

        private void AlertButton_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
        {
            if ( !isSpeedAlertEnabled )
            {
                var alertPopup = new AlertPopup( UnitTextBlock.Text ) { AlertSpeed = (int) speedAlertSpeed };
                PopupContent.Children.Add( alertPopup );
                LayoutRoot.IsHitTestVisible = false;
                ApplicationBar.IsVisible = false;

                alertPopup.CloseCompleted += ( s, _ ) =>
                {
                    PopupContent.Children.Clear();
                    LayoutRoot.IsHitTestVisible = true;
                    ApplicationBar.IsVisible = true;
                    speedAlertSpeed = alertPopup.AlertSpeed;
                    settings["SpeedAlertSpeedConfig"] = speedAlertSpeed.ToString();
                    settings["SpeedAlertConfig"] = isSpeedAlertEnabled = true;
                    settings.Save();
                    UpdateSpeedAlert();
                };
            }
            else
            {
                settings["SpeedAlertConfig"] = isSpeedAlertEnabled = false;
                settings.Save();
                UpdateSpeedAlert();
            }

        }

        private void MapButton_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
        {
            mapStatus = mapStatus == MapStatus.On ? MapStatus.Off
                      : mapStatus == MapStatus.Off ? MapStatus.On
                                                   : MapStatus.Disabled;

            settings["map"] = mapStatus;
            settings.Save();
            UpdateMap();
        }

        private void GestureListener_PinchStarted( object sender, PinchStartedGestureEventArgs e )
        {
            mapScale = BackMap.ZoomLevel;
        }

        private void GestureListener_PinchDelta( object sender, PinchGestureEventArgs e )
        {
            double desiredZoom = mapScale * Math.Pow( e.DistanceRatio, 0.5 );
            if ( desiredZoom < 1 )
            {
                BackMap.ZoomLevel = 1;
            }
            else if ( desiredZoom > 19 )
            {
                BackMap.ZoomLevel = 19;
            }
            else
            {
                BackMap.ZoomLevel = desiredZoom;
            }
        }

        private void GestureListener_PinchCompleted( object sender, PinchGestureEventArgs e )
        {
            mapScale = BackMap.ZoomLevel;
        }

        #region INotifyPropertyChanged implementation
        // TODO: find a way to reuse the code...

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Fires the <see cref="PropertyChanged"/> event.
        /// </summary>
        private void FirePropertyChanged( [CallerMemberName] string propertyName = "" )
        {
            var evt = this.PropertyChanged;
            if ( evt != null )
            {
                evt( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        /// <summary>
        /// Sets the specified field to the specified value and raises <see cref="PropertyChanged"/> if needed.
        /// </summary>
        private void SetProperty<T>( ref T field, T value, [CallerMemberName] string propertyName = "" )
        {
            if ( !object.Equals( field, value ) )
            {
                field = value;
                this.FirePropertyChanged( propertyName );
            }
        }
        #endregion
    }
}