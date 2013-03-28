using System;
using System.ComponentModel;
using System.Device.Location;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Speedo
{
    public partial class MainPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        // HACK: For some reason, a Binding subclass for settings makes the app crash on startup or not show some UI parts
        public AppSettings Settings { get; private set; }
        public MovementSource MovementSource { get; private set; }
        public SpeedAlert SpeedAlert { get; private set; }

        private bool showSpeedGraph;
        public bool ShowSpeedGraph
        {
            get { return showSpeedGraph; }
            set { SetProperty( ref showSpeedGraph, value ); }
        }

        private bool isLocating;
        public bool IsLocating
        {
            get { return isLocating; }
            set { SetProperty( ref isLocating, value ); }
        }

        private MapStatus mapStatus;
        public MapStatus MapStatus
        {
            get { return mapStatus; }
            set { SetProperty( ref mapStatus, value ); }
        }

        private GpsStatus gpsStatus;
        public GpsStatus GpsStatus
        {
            get { return gpsStatus; }
            set { SetProperty( ref gpsStatus, value ); }
        }

        public ICommand SwitchMapStatusCommand { get; private set; }
        public ICommand SwitchUnitsCommand { get; private set; }
        public ICommand SwitchLocationAccessCommand { get; private set; }
        public ICommand AboutCommand { get; private set; }

        private MapStatus previousStatus;
        private bool windscreenMode = false;

        public MainPage()
        {
            Settings = AppSettings.Current;
            MovementSource = new MovementSource();
            MovementSource.GeoStatusChanged += MovementSource_GeoStatusChanged;
            MovementSource.ReadingChanged += MovementSource_ReadingChanged;
            ShowSpeedGraph = true;
            DataContext = this;

            IsLocating = true;

            SwitchMapStatusCommand = new RelayCommand( ExecuteSwitchMapStatusCommand, CanExecuteSwitchMapStatusCommand );
            SwitchUnitsCommand = new RelayCommand( ExecuteSwitchUnitsCommand );
            SwitchLocationAccessCommand = new RelayCommand( ExecuteSwitchLocationAccessCommand );
            AboutCommand = new RelayCommand( ExecuteAboutCommand );

            SpeedAlert = new SpeedAlert( MovementSource, SpeedAlert.SoundProvider );

            InitializeComponent();
            ShowWarnings();

            // no idling here
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

            // hide settings button
            VisualStateManager.GoToState( this, "HideControls", false );

            SpeedAlert.Limit = AppSettings.Current.SpeedLimit;

            UpdateLocationAccess();
        }

        private void ShowWarnings()
        {
            if ( AppSettings.Current.IsFirstRun )
            {
                AppSettings.Current.IsFirstRun = false;
                MessageBox.Show( "This software uses GPS signals to calculate your speed and direction which is subject to interference and results may be skewed.\n\nThe information provided can only be used as a guide.", "Accuracy warning", MessageBoxButton.OK );
                MessageBox.Show( "This software temporarily stores and uses your location data for the purpose of calculating speed and direction.\n\nYour location may be sent to Bing over the internet to position the map.", "Location privacy statement", MessageBoxButton.OK );
            }
        }

        private void UpdateWindscreen()
        {
            if ( windscreenMode )
            {
                ContentScaleTransform.ScaleX = -1;

                App.Current.ForceDarkTheme();
                App.Current.EnableWindscreenColors();

                ShowSpeedGraph = false;
                windsreenIndicator.Visibility = Visibility.Visible;

                previousStatus = MapStatus;
                MapStatus = MapStatus.Disabled;
            }
            else
            {
                ContentScaleTransform.ScaleX = 1;

                App.Current.AllowLightTheme();
                App.Current.DisableWindscreenColors();

                ShowSpeedGraph = true;
                windsreenIndicator.Visibility = Visibility.Collapsed;
                MapStatus = previousStatus;
            }
        }

        private void UpdateLocationAccess()
        {
            if ( AppSettings.Current.AllowLocationAccess )
            {
                MovementSource.Start();
            }
            else
            {
                MovementSource.Stop();
                GpsStatus = GpsStatus.Inaccessible;
                MapStatus = MapStatus.Disabled;
            }
        }

        protected override void OnNavigatedTo( NavigationEventArgs e )
        {
            if ( PhoneApplicationService.Current.StartupMode == StartupMode.Activate )
            {
                var stateSettings = PhoneApplicationService.Current.State;
                windscreenMode = (bool) stateSettings["windscreenMode"];
                SpeedAlert.IsEnabled = (bool) stateSettings["SpeedAlertConfig"];
                UpdateWindscreen();
            }
        }

        protected override void OnNavigatedFrom( NavigationEventArgs e )
        {
            var stateSettings = PhoneApplicationService.Current.State;
            stateSettings["windscreenMode"] = windscreenMode;
            stateSettings["SpeedAlertConfig"] = SpeedAlert.IsEnabled;
        }

        protected override void OnOrientationChanged( OrientationChangedEventArgs e )
        {
            base.OnOrientationChanged( e );
            VisualStateManager.GoToState( this, e.Orientation.ToString(), true );
        }

        private void ExecuteSwitchMapStatusCommand( object parameter )
        {
            MapStatus = MapStatus == MapStatus.On ? MapStatus.Off : MapStatus.On;
            AppSettings.Current.MapStatus = MapStatus;
        }

        private bool CanExecuteSwitchMapStatusCommand( object parameter )
        {
            return MapStatus != MapStatus.Disabled;
        }

        private void ExecuteSwitchUnitsCommand( object parameter )
        {
            AppSettings.Current.SpeedUnit = SpeedUtils.Switch( AppSettings.Current.SpeedUnit );
        }

        private void ExecuteSwitchLocationAccessCommand( object parameter )
        {
            if ( AppSettings.Current.AllowLocationAccess )
            {
                var warningMsg = MessageBox.Show( "This application will not work without location access. Are you sure you still want to disable it?", "Disable location", MessageBoxButton.OKCancel );
                if ( warningMsg == MessageBoxResult.OK )
                {
                    AppSettings.Current.AllowLocationAccess = false;
                    UpdateLocationAccess();
                }
            }
            else
            {
                AppSettings.Current.AllowLocationAccess = true;
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
                    GpsStatus = GpsStatus.Inaccessible;
                    IsLocating = false;
                    MapStatus = MapStatus.Disabled;
                    break;

                case GeoPositionStatus.Initializing:
                    // The Location Service is initializing.
                    // Disable the Start Location button.
                    GpsStatus = GpsStatus.Initializing;
                    IsLocating = true;
                    MapStatus = MapStatus.Disabled;
                    break;

                case GeoPositionStatus.NoData:
                    // The Location Service is working, but it cannot get location data.
                    GpsStatus = GpsStatus.Unavailable;
                    IsLocating = false;
                    MapStatus = MapStatus.Disabled;
                    break;

                case GeoPositionStatus.Ready:
                    // The Location Service is working and is receiving location data.
                    // Show the current position and enable the Stop Location button.
                    GpsStatus = GpsStatus.Normal;
                    IsLocating = false;
                    MapStatus = AppSettings.Current.MapStatus;
                    break;
            }
        }

        private void MovementSource_ReadingChanged( object sender, EventArgs e )
        {
            GpsStatus = MovementSource.Position.HorizontalAccuracy < 70 ? GpsStatus.Normal : GpsStatus.Weak;
        }

        // TODO: This is not bound to anything
        private void ResetTrip_Click( object sender, EventArgs e )
        {
            MovementSource.Start();
        }

        private void WindscreenButton_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
        {
            windscreenMode = !windscreenMode;
            UpdateWindscreen();
        }

        private void AlertButton_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
        {
            if ( SpeedAlert.IsEnabled )
            {
                AppSettings.Current.IsSpeedAlertEnabled = SpeedAlert.IsEnabled = false;
            }
            else
            {
                // HACK: simplest way to pass parameters...
                PhoneApplicationService.Current.State["SpeedAlert"] = SpeedAlert;
                NavigationService.Navigate( new Uri( "/AlertPage.xaml", UriKind.Relative ) );
                AppSettings.Current.IsSpeedAlertEnabled = SpeedAlert.IsEnabled = true;
            }
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void SetProperty<T>( ref T field, T value, [CallerMemberName] string propertyName = "" )
        {
            NotifyHelper.SetProperty( ref field, value, propertyName, this, PropertyChanged );
        }
        #endregion
    }
}