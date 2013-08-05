// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Long Zheng, Solal Pirelli

using System;
using System.ComponentModel;
using System.Device.Location;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Speedo.Languages;

namespace Speedo
{
    public partial class MainPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        // HACK: Can't bind to static properties (subclassing Binding results in weird stuff)
        public AppSettings Settings { get; private set; }
        public MovementSource MovementSource { get; private set; }
        public SpeedAlert SpeedAlert { get; private set; }

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

        private bool isWindscreenModeEnabled;
        public bool IsWindscreenModeEnabled
        {
            get { return isWindscreenModeEnabled; }
            set { SetProperty( ref isWindscreenModeEnabled, value ); }
        }

        private bool isLocating;
        public bool IsLocating
        {
            get { return isLocating; }
            set { SetProperty( ref isLocating, value ); }
        }

        public RelayCommand SwitchMapStatusCommand { get; private set; }
        public RelayCommand SwitchWindscreenModeCommand { get; private set; }
        public RelayCommand SwitchSpeedAlertCommand { get; private set; }
        public RelayCommand SwitchUnitsCommand { get; private set; }
        public RelayCommand SwitchLocationAccessCommand { get; private set; }
        public RelayCommand AboutCommand { get; private set; }

        private MapStatus previousStatus;

        public MainPage()
        {
            Settings = AppSettings.Current;

            MovementSource = new MovementSource();
            MovementSource.GeoStatusChanged += MovementSource_GeoStatusChanged;
            MovementSource.PropertyChanged += MovementSource_PropertyChanged;

            DataContext = this;

            SwitchMapStatusCommand = new RelayCommand( ExecuteSwitchMapStatusCommand, CanExecuteSwitchMapStatusCommand );
            SwitchMapStatusCommand.BindToPropertyChange( this, "MapStatus" );
            SwitchWindscreenModeCommand = new RelayCommand( ExecuteSwitchWindscreenModeCommand );
            SwitchSpeedAlertCommand = new RelayCommand( ExecuteSwitchSpeedAlertCommand );
            SwitchUnitsCommand = new RelayCommand( ExecuteSwitchUnitsCommand );
            SwitchLocationAccessCommand = new RelayCommand( ExecuteSwitchLocationAccessCommand );
            AboutCommand = new RelayCommand( ExecuteAboutCommand );

            SpeedAlert = new SpeedAlert( MovementSource, SpeedAlert.SoundProvider );

            InitializeComponent();
            ShowWarnings();

            // no idling here
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

            UpdateLocationAccess();
        }

        private void ShowWarnings()
        {
            if ( Settings.IsFirstRun )
            {
                Settings.IsFirstRun = false;
                MessageBox.Show( AppResources.AccuracyWarningMessage, AppResources.AccuracyWarningCaption, MessageBoxButton.OK );
                App.Current.ShowPrivacyPolicy();
            }
        }

        private void UpdateWindscreen()
        {
            if ( IsWindscreenModeEnabled )
            {
                ContentScaleTransform.ScaleX = -1;

                App.Current.ForceDarkTheme();
                App.Current.EnableWindscreenColors();

                previousStatus = Settings.MapStatus;
                MapStatus = MapStatus.Disabled;
            }
            else
            {
                ContentScaleTransform.ScaleX = 1;

                App.Current.AllowLightTheme();
                App.Current.DisableWindscreenColors();

                MapStatus = previousStatus;
            }
        }

        private void UpdateLocationAccess()
        {
            if ( Settings.AllowLocationAccess )
            {
                IsLocating = true;
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
                IsWindscreenModeEnabled = (bool) stateSettings["windscreenMode"];
                UpdateWindscreen();
            }
        }

        protected override void OnNavigatedFrom( NavigationEventArgs e )
        {
            var stateSettings = PhoneApplicationService.Current.State;
            stateSettings["windscreenMode"] = IsWindscreenModeEnabled;
        }

        protected override void OnOrientationChanged( OrientationChangedEventArgs e )
        {
            base.OnOrientationChanged( e );
            VisualStateManager.GoToState( this, e.Orientation.ToString(), true );
        }

        private void ExecuteSwitchMapStatusCommand( object parameter )
        {
            MapStatus = MapStatus == MapStatus.On ? MapStatus.Off : MapStatus.On;
            Settings.MapStatus = MapStatus;
        }

        private bool CanExecuteSwitchMapStatusCommand( object parameter )
        {
            return MapStatus != MapStatus.Disabled;
        }

        private void ExecuteSwitchWindscreenModeCommand( object parameter )
        {
            IsWindscreenModeEnabled = !IsWindscreenModeEnabled;
            UpdateWindscreen();
        }

        private void ExecuteSwitchSpeedAlertCommand( object parameter )
        {
            if ( Settings.IsSpeedAlertEnabled )
            {
                Settings.IsSpeedAlertEnabled = false;
            }
            else
            {
                // HACK: simplest way to pass parameters...
                PhoneApplicationService.Current.State["SpeedAlert"] = SpeedAlert;
                NavigationService.Navigate( new Uri( "/AlertPage.xaml", UriKind.Relative ) );
            }
        }

        private void ExecuteSwitchUnitsCommand( object parameter )
        {
            var newUnit = SpeedUtils.Switch( Settings.SpeedUnit );
            Settings.SpeedLimit = SpeedUtils.ConvertSpeedLimit( Settings.SpeedUnit, newUnit, Settings.SpeedLimit );
            Settings.SpeedUnit = newUnit;
        }

        private void ExecuteSwitchLocationAccessCommand( object parameter )
        {
            if ( Settings.AllowLocationAccess )
            {
                var warningMsg = MessageBox.Show( AppResources.LocationDisableWarningMessage, AppResources.LocationDisableWarningCaption, MessageBoxButton.OKCancel );
                if ( warningMsg == MessageBoxResult.OK )
                {
                    Settings.AllowLocationAccess = false;
                    UpdateLocationAccess();
                }
            }
            else
            {
                Settings.AllowLocationAccess = true;
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
                    GpsStatus = GpsStatus.Inaccessible;
                    IsLocating = false;
                    MapStatus = MapStatus.Disabled;
                    break;

                case GeoPositionStatus.Initializing:
                    GpsStatus = GpsStatus.Initializing;
                    IsLocating = true;
                    MapStatus = MapStatus.Disabled;
                    break;

                case GeoPositionStatus.NoData:
                    GpsStatus = GpsStatus.Unavailable;
                    IsLocating = false;
                    MapStatus = MapStatus.Disabled;
                    break;

                case GeoPositionStatus.Ready:
                    GpsStatus = GpsStatus.Normal;
                    IsLocating = false;
                    if ( !IsWindscreenModeEnabled )
                    {
                        // Don't enable the map if the user enabled windscreen mode during loading
                        MapStatus = Settings.MapStatus;
                    }
                    break;
            }
        }

        private void MovementSource_PropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            Dispatcher.BeginInvoke( () =>
            {
                GpsStatus = MovementSource.Position.HorizontalAccuracy < 70 ? GpsStatus.Normal : GpsStatus.Weak;
            } );
        }

        // TODO: This is not bound to anything
        //private void ResetTrip_Click( object sender, EventArgs e )
        //{
        //    MovementSource.Start();
        //}

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void SetProperty<T>( ref T field, T value, [CallerMemberName] string propertyName = "" )
        {
            NotifyHelper.SetProperty( ref field, value, propertyName, this, PropertyChanged );
        }
        #endregion
    }
}