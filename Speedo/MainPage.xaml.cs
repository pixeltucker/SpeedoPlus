using System;
using System.ComponentModel;
using System.Device.Location;
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
        private bool windscreenMode = false;
        private bool darkTheme = (Visibility) Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible;
        private int settingsDisplayCount;

        public MainPage()
        {
            Settings = AppSettings.Current;
            MovementSource = new MovementSource();
            MovementSource.GeoStatusChanged += MovementSource_GeoStatusChanged;
            MovementSource.ReadingChanged += MovementSource_ReadingChanged;
            ShowSpeedGraph = true;
            DataContext = this;

            IsLocating = true;

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

        private void UpdateLocationAccess()
        {
            if ( AppSettings.Current.AllowLocationAccess )
            {
                SwitchLocationAccessText = "disable location access";
                MovementSource.Start();
            }
            else
            {
                SwitchLocationAccessText = "enable location access";
                MovementSource.Stop();
                StatusTextBlock.Text = "location inaccessible";
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
                    StatusTextBlock.Text = "location inaccessible";
                    IsLocating = false;
                    MapStatus = MapStatus.Disabled;
                    break;

                case GeoPositionStatus.Initializing:
                    // The Location Service is initializing.
                    // Disable the Start Location button.
                    StatusTextBlock.Text = "GPS initializating";
                    IsLocating = true;
                    MapStatus = MapStatus.Disabled;
                    break;

                case GeoPositionStatus.NoData:
                    // The Location Service is working, but it cannot get location data.
                    StatusTextBlock.Text = "GPS not available";
                    IsLocating = false;
                    MapStatus = MapStatus.Disabled;
                    break;

                case GeoPositionStatus.Ready:
                    // The Location Service is working and is receiving location data.
                    // Show the current position and enable the Stop Location button.
                    StatusTextBlock.Text = "";
                    IsLocating = false;
                    break;
            }
        }

        private void MovementSource_ReadingChanged( object sender, EventArgs e )
        {
            Dispatcher.BeginInvoke( () =>
            {
                if ( MovementSource.Position.HorizontalAccuracy > 70 )
                {
                    StatusTextBlock.Text = "weak GPS signal";
                }
            } );
        }

        // TODO: This is not bound to anything
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
            if ( SpeedAlert.IsEnabled )
            {
                AppSettings.Current.IsSpeedAlertEnabled = SpeedAlert.IsEnabled = false;
            }
            else
            {
                var alertPopup = new AlertPopup( SpeedAlert );

                PopupContent.Children.Add( alertPopup );
                LayoutRoot.IsHitTestVisible = false;
                ApplicationBar.IsVisible = false;

                alertPopup.CloseCompleted += ( s, _ ) =>
                {
                    PopupContent.Children.Clear();
                    LayoutRoot.IsHitTestVisible = true;
                    ApplicationBar.IsVisible = true;
                    AppSettings.Current.SpeedLimit = SpeedAlert.Limit;
                    AppSettings.Current.IsSpeedAlertEnabled = SpeedAlert.IsEnabled = true;
                };
            }
        }

        private void MapButton_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
        {
            MapStatus = MapStatus == MapStatus.On ? MapStatus.Off
                      : MapStatus == MapStatus.Off ? MapStatus.On
                                                   : MapStatus.Disabled;

            AppSettings.Current.MapStatus = MapStatus;
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