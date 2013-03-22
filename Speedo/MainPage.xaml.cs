using System;
using System.ComponentModel;
using System.Device.Location;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Speedo.Controls;

namespace Speedo
{
    public partial class MainPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        public MovementSource MovementSource { get; private set; }
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

        private SpeedUnit speedUnit;
        public SpeedUnit SpeedUnit
        {
            get { return speedUnit; }
            set { SetProperty( ref speedUnit, value ); }
        }

        private string switchLocationAccessText;
        public string SwitchLocationAccessText
        {
            get { return switchLocationAccessText; }
            set { SetProperty( ref switchLocationAccessText, value ); }
        }

        private MapStatus mapStatus;
        public MapStatus MapStatus
        {
            get { return mapStatus; }
            set { SetProperty( ref mapStatus, value ); }
        }

        public ICommand SwitchUnitsCommand { get; private set; }
        public ICommand SwitchLocationAccessCommand { get; private set; }
        public ICommand AboutCommand { get; private set; }

        private MapStatus previousStatus;
        private SpeedAlert speedAlert;

        private double accuracy = 0;
        private double mapScale = 17;
        private bool windscreenMode = false;
        private IsolatedStorageSettings settings;
        private bool darkTheme = (Visibility) Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible;
        private int settingsDisplayCount;

        public MainPage()
        {
            MovementSource = new MovementSource();
            MovementSource.GeoStatusChanged += MovementSource_GeoStatusChanged;
            ShowSpeedGraph = true;
            DataContext = this;

            IsLocating = true;

            SwitchUnitsCommand = new RelayCommand( ExecuteSwitchUnitsCommand );
            SwitchLocationAccessCommand = new RelayCommand( ExecuteSwitchLocationAccessCommand );
            AboutCommand = new RelayCommand( ExecuteAboutCommand );

            settings = IsolatedStorageSettings.ApplicationSettings;
            settings.Clear();
            speedAlert = new SpeedAlert( MovementSource, SpeedAlert.SoundProvider );

            InitializeComponent();
            ShowWarnings();

            // no idling here
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

            // get our unit setting        
            if ( !settings.TryGetValue<SpeedUnit>( "unit", out speedUnit ) )
            {
                // get our region
                if ( CultureInfo.CurrentCulture.Name == "en-US" )
                {
                    // this is English US, default to miles
                    SpeedUnit = SpeedUnit.Miles;
                }
                else
                {
                    // elsewhere, default to KM
                    SpeedUnit = SpeedUnit.Kilometers;
                }
            }

            // hide settings button
            VisualStateManager.GoToState( this, "HideControls", false );

            // get our map setting
            if ( !settings.TryGetValue<MapStatus>( "map", out mapStatus ) )
            {
                settings["map"] = MapStatus = MapStatus.On;
            }

            // get our speed alert speed setting
            bool isSpeedAlertEnabled = false;
            if ( settings.TryGetValue<bool>( "SpeedAlertConfig", out isSpeedAlertEnabled ) )
            {
                speedAlert.IsEnabled = isSpeedAlertEnabled;
            }
            UpdateSpeedAlert();

            int speedLimit;
            if ( !settings.TryGetValue( "SpeedLimit", out speedLimit ) )
            {
                if ( CultureInfo.CurrentCulture.Name == "en-US" )
                {
                    // this is English US, default to 65
                    speedLimit = 65;
                }
                else
                {
                    // elsewhere, default to 80
                    speedLimit = 100;
                }
            }
            speedAlert.Limit = speedLimit;

            // get our location setting
            if ( !settings.TryGetValue<bool>( "LocationAccess", out allowLocationAccess ) )
            {
                settings["LocationAccess"] = AllowLocationAccess = true;
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
                //if ( Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Emulator )
                //{
                //    StatusTextBlock.Text = "Emulator mode";
                //    if ( lastKnownPos != null )
                //    {
                //        currentSpeed = ( e.Position.Location.GetDistanceTo( lastKnownPos ) );
                //        if ( currentSpeed > 0 )
                //        {
                //            var lat1 = Math.PI * lastKnownPos.Latitude / 180.0;
                //            var lat2 = Math.PI * e.Position.Location.Latitude / 180.0;
                //            var dLon = Math.PI * e.Position.Location.Longitude / 180.0 - Math.PI * lastKnownPos.Longitude / 180.0;
                //            var y = Math.Sin( dLon ) * Math.Cos( lat2 );
                //            var x = Math.Cos( lat1 ) * Math.Sin( lat2 ) - Math.Sin( lat1 ) * Math.Cos( lat2 ) * Math.Cos( dLon );
                //            var brng = Math.Atan2( y, x );
                //            currentCourse = ( 180.0 * brng / Math.PI + 360 ) % 360;
                //        }
                //    }
                //}
            }
            else
            {
                StatusTextBlock.Text = "weak GPS signal";
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
                // TODO fix that

                //SpeedTextBlock.Foreground = (SolidColorBrush) App.Current.Resources["WindscreenColor"];
                //DirectionTextBlock.Foreground = (SolidColorBrush) App.Current.Resources["WindscreenColor"];
                //DirectionIcon.Fill = (SolidColorBrush) App.Current.Resources["WindscreenColor"];
                //SpeedChart.Visibility = Visibility.Collapsed;
                //UnitTextBlock.Opacity = 0;

                ShowSpeedGraph = false;
                windsreenIndicator.Visibility = Visibility.Visible;

                previousStatus = MapStatus;
                MapStatus = MapStatus.Disabled;
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
                    foregroundbrush.Color = (Color) App.Current.Resources["PhoneForegroundColor"];
                    backgroundbrush.Color = (Color) App.Current.Resources["PhoneBackgroundColor"];
                    subtlebrush.Color = (Color) App.Current.Resources["PhoneSubtleColor"];
                    disabledbrush.Color = (Color) App.Current.Resources["PhoneDisabledColor"];
                }
                //SpeedTextBlock.Foreground = (SolidColorBrush) App.Current.Resources["PhoneForegroundBrush"];
                //DirectionTextBlock.Foreground = (SolidColorBrush) App.Current.Resources["PhoneAccentBrush"];
                //DirectionIcon.Fill = (SolidColorBrush) App.Current.Resources["PhoneAccentBrush"];
                ShowSpeedGraph = true;
                windsreenIndicator.Visibility = Visibility.Collapsed;
                MapStatus = previousStatus;
            }
        }

        private void UpdateSpeedAlert()
        {
            alertIndicator.Visibility = speedAlert.IsEnabled ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateLocationAccess()
        {
            settings["LocationAccess"] = AllowLocationAccess;
            if ( AllowLocationAccess )
            {
                SwitchLocationAccessText = "disable location access";
                MovementSource.Start();
            }
            else
            {
                SwitchLocationAccessText = "enable location access";
                MovementSource.Stop();
                StatusTextBlock.Text = "location inaccessible";
                mapStatus = MapStatus.Disabled;
            }
            settings.Save();
        }

        protected override void OnNavigatedTo( NavigationEventArgs e )
        {
            if ( PhoneApplicationService.Current.StartupMode == StartupMode.Activate )
            {
                var stateSettings = PhoneApplicationService.Current.State;
                windscreenMode = (bool) stateSettings["windscreenMode"];
                mapScale = (double) stateSettings["mapScale"];
                speedAlert.IsEnabled = (bool) stateSettings["SpeedAlertConfig"];
                UpdateWindscreen();
                UpdateSpeedAlert();
            }
        }

        protected override void OnNavigatedFrom( NavigationEventArgs e )
        {
            var stateSettings = PhoneApplicationService.Current.State;
            stateSettings["windscreenMode"] = windscreenMode;
            stateSettings["mapScale"] = mapScale;
            stateSettings["SpeedAlertConfig"] = speedAlert.IsEnabled;
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
            settings["unit"] = SpeedUnit = SpeedUtils.Switch( SpeedUnit );
            settings.Save();
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

        private void MovementSource_GeoStatusChanged( object sender, GeoPositionStatusChangedEventArgs e )
        {
            switch ( e.Status )
            {
                case GeoPositionStatus.Disabled:
                    // The Location Service is disabled or unsupported.
                    // Check to see whether the user has disabled the Location Service.
                    StatusTextBlock.Text = "location inaccessible";
                    IsLocating = false;
                    mapStatus = MapStatus.Disabled;
                    break;

                case GeoPositionStatus.Initializing:
                    // The Location Service is initializing.
                    // Disable the Start Location button.
                    StatusTextBlock.Text = "GPS initializating";
                    IsLocating = true;
                    mapStatus = MapStatus.Disabled;
                    break;

                case GeoPositionStatus.NoData:
                    // The Location Service is working, but it cannot get location data.
                    StatusTextBlock.Text = "GPS not available";
                    IsLocating = false;
                    mapStatus = MapStatus.Disabled;
                    break;

                case GeoPositionStatus.Ready:
                    // The Location Service is working and is receiving location data.
                    // Show the current position and enable the Stop Location button.
                    StatusTextBlock.Text = "";
                    IsLocating = false;
                    settings.TryGetValue<MapStatus>( "map", out mapStatus );
                    break;
            }
        }

        private void Watcher_PositionChanged( object sender, GeoPositionChangedEventArgs<GeoCoordinate> e )
        {
            Dispatcher.BeginInvoke( () => PositionChanged( e ) );
        }

        private void ResetTrip_Click( object sender, EventArgs e )
        {
            MovementSource.Start();
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
            if ( speedAlert.IsEnabled )
            {
                settings["SpeedAlertConfig"] = speedAlert.IsEnabled = false;
                settings.Save();
                UpdateSpeedAlert();
            }
            else
            {
                var alertPopup = new AlertPopup( SpeedUnit ) { AlertSpeed = speedAlert.Limit };

                PopupContent.Children.Add( alertPopup );
                LayoutRoot.IsHitTestVisible = false;
                ApplicationBar.IsVisible = false;

                alertPopup.CloseCompleted += ( s, _ ) =>
                {
                    PopupContent.Children.Clear();
                    LayoutRoot.IsHitTestVisible = true;
                    ApplicationBar.IsVisible = true;
                    settings["SpeedLimit"] = speedAlert.Limit = alertPopup.AlertSpeed; ;
                    settings["SpeedAlertConfig"] = speedAlert.IsEnabled = true;
                    settings.Save();
                    UpdateSpeedAlert();
                };
            }
        }

        private void MapButton_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
        {
            MapStatus = MapStatus == MapStatus.On ? MapStatus.Off
                      : MapStatus == MapStatus.Off ? MapStatus.On
                                                   : MapStatus.Disabled;

            settings["map"] = MapStatus;
            settings.Save();
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