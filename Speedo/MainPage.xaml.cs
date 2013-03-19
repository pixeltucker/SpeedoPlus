using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Device.Location;
using Microsoft.Phone.Shell;
using System.Windows.Threading;
using System.IO.IsolatedStorage;
using System.Globalization;
using Microsoft.Phone.Controls.Maps;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using System.IO;
using System.ComponentModel;


namespace Speedo
{

    public partial class MainPage : PhoneApplicationPage
    {
        GeoCoordinateWatcher watcher;
        Double CurrentSpeed = 0;
        Double CurrentCourse = 0;
        Double MaxSpeed = 0;
        List<double> prevSpeeds = new List<double>();
        Double AvgSpeed = 0;
        Double accuracy = 0;
        Double distance = 0;
        Double mapScale = 17;
        GeoCoordinate lastKnownPos;
        Boolean windscreenMode = false;
        public string unitConfig;
        string mapConfig;
        string warningConfig;
        DispatcherTimer settingsTimer = new DispatcherTimer();
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        bool darkTheme = ((Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible);
        int SpeedGraphMaxCount = 360;
        Random _rand = new Random();
        bool errorMap = false;
        string LocationAccess;
        string SpeedAlertConfig;
        string SpeedAlertSpeedConfig;
        Double SpeedAlertSpeed;
        SoundEffect SpeedAlertEffect;
        DispatcherTimer SpeedAlertTimer = new DispatcherTimer();

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // show GPS warning
            settings.TryGetValue<string>("warning", out warningConfig);
            if (warningConfig != "seen")
            {
                var warningMsg = MessageBox.Show("This software uses GPS signals to calculate your speed and direction which is subject to interference and results may be skewed.\n\nThe information provided can only be used as a guide.", "Accuracy warning", MessageBoxButton.OK);
                if (warningMsg == MessageBoxResult.OK)
                {
                    var privacyMsg = MessageBox.Show("This software temporarily stores and uses your location data for the purpose of calculating speed and direction.\n\nYour location may be sent to Bing over the internet to position the map.", "Location privacy statement", MessageBoxButton.OK);
                    if (privacyMsg == MessageBoxResult.OK)
                    {
                        settings["warning"] = "seen";
                    }
                }
            }

            // no idling here
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

            // get our unit setting
            settings.TryGetValue<string>("unit", out unitConfig);
            if (unitConfig != "km" && unitConfig != "mi")
            {
                // get our region
                if (CultureInfo.CurrentCulture.Name == "en-US")
                {
                    // this is English US, default to miles
                    unitConfig = "mi";
                }
                else
                {
                    // elsewhere, default to KM
                    unitConfig = "km";
                }
            }
            UpdateUnit();

            if (SpeedGraph.Points.Count == 0)
            {
                ResetSpeedGraph();
            }

            // update all the speed text
            UpdateSpeed();

            // hide settings button
            VisualStateManager.GoToState(this, "HideControls", false);

            // initialize Bing map with our API
            BackMap.CredentialsProvider = new ApplicationIdCredentialsProvider("AtV3X75PD_JTG4pJKbQtd3cT8YRD2b8Fdow7mVKr2wdx63VB4jDqxlU1WELTVFDv");
            BackMap.ZoomLevel = mapScale;

            // get our map setting
            settings.TryGetValue<string>("map", out mapConfig);
            if (mapConfig != "on" && mapConfig != "off")
            {
                mapConfig = "on";
                settings["map"] = "on";
            }
            UpdateMap();

            // get our speed alert speed setting
            settings.TryGetValue<string>("SpeedAlertConfig", out SpeedAlertConfig);
            if (SpeedAlertConfig != "on" && SpeedAlertConfig != "off")
            {
                SpeedAlertConfig = "off";
                settings["SpeedAlertConfig"] = "off";
            }
            UpdateSpeedAlert();

            settings.TryGetValue<string>("SpeedAlertSpeedConfig", out SpeedAlertSpeedConfig);
            if (SpeedAlertSpeedConfig == null)
            {
                // get our region
                if (CultureInfo.CurrentCulture.Name == "en-US")
                {
                    // this is English US, default to 65
                    SpeedAlertSpeed = 65;
                }
                else
                {
                    // elsewhere, default to 80
                    SpeedAlertSpeed = 100;
                }
            }
            else
            {
                SpeedAlertSpeed = Convert.ToDouble(SpeedAlertSpeedConfig);
            }

            Stream AlertStream = TitleContainer.OpenStream("Resources/alert.wav");
            SpeedAlertEffect = SoundEffect.FromStream(AlertStream);
            FrameworkDispatcher.Update();
            SpeedAlertTimer.Tick +=
                        delegate(object s, EventArgs args)
                        {
                            SpeedAlertEffect.Play();
                        };
            SpeedAlertTimer.Interval = new TimeSpan(0, 0, 5);

            // initialize our GPS watcher
            if (watcher == null)
            {
                watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High); // using high accuracy
                watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
                watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            }
            watcher.Start();

            // get our location setting
            settings.TryGetValue<string>("LocationAccess", out LocationAccess);
            if (LocationAccess != "on" && LocationAccess != "off")
            {
                LocationAccess = "on";
            }
            UpdateLocationAccess();

        }

        // Event handler for the GeoCoordinateWatcher.StatusChanged event.
        void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case GeoPositionStatus.Disabled:
                    // The Location Service is disabled or unsupported.
                    // Check to see whether the user has disabled the Location Service.
                    StatusTextBlock.Text = "location inaccessible";
                    LocatingIndicator.IsVisible = false;
                    CurrentSpeed = Double.NaN;
                    UpdateSpeed();
                    mapConfig = "disabled";
                    UpdateMap();
                    break;

                case GeoPositionStatus.Initializing:
                    // The Location Service is initializing.
                    // Disable the Start Location button.
                    StatusTextBlock.Text = "GPS initializating";
                    LocatingIndicator.IsVisible = true;
                    CurrentSpeed = Double.NaN;
                    UpdateSpeed();
                    mapConfig = "disabled";
                    UpdateMap();
                    break;

                case GeoPositionStatus.NoData:
                    // The Location Service is working, but it cannot get location data.
                    StatusTextBlock.Text = "GPS not available";
                    LocatingIndicator.IsVisible = false;
                    mapConfig = "disabled";
                    UpdateMap();
                    break;

                case GeoPositionStatus.Ready:
                    // The Location Service is working and is receiving location data.
                    // Show the current position and enable the Stop Location button.
                    StatusTextBlock.Text = "";
                    LocatingIndicator.IsVisible = false;
                    settings.TryGetValue<string>("map", out mapConfig);
                    UpdateMap();
                    break;
            }
        }

        void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            Dispatcher.BeginInvoke(() => PositionChanged(e));
        }

        private string CourseDirection(Double course)
        {
            if (Double.IsNaN(course))
            {
                return "";
            }
            else if (0 <= course && course <= 22.50)
            {
                return "N";
            }
            else if (22.50 < course && course <= 67.50)
            {
                return "NE";
            }
            else if (67.50 < course && course <= 112.50)
            {
                return "E";
            }
            else if (112.50 < course && course <= 157.50)
            {
                return "SE";
            }
            else if (157.50 < course && course <= 202.50)
            {
                return "S";
            }
            else if (202.50 < course && course <= 247.50)
            {
                return "SW";
            }
            else if (247.50 < course && course <= 292.50)
            {
                return "W";
            }
            else if (292.50 < course && course <= 337.50)
            {
                return "NW";
            }
            else if (337.50 < course && course <= 360)
            {
                return "N";
            }
            else
            {
                return null;
            }
        }

        void PositionChanged(GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            // store horizontal GPS accuracy
            accuracy = e.Position.Location.HorizontalAccuracy;

            // update only if accuracy <= 70m
            if (accuracy <= 70)
            {
                StatusTextBlock.Text = "";
                if (Double.IsNaN(e.Position.Location.Speed))
                {
                    CurrentSpeed = 0;
                }
                else
                {
                    CurrentSpeed = e.Position.Location.Speed * 3.6; // convert M/s into KM/h
                    CurrentCourse = e.Position.Location.Course;
                    UpdateAverage();
                }

                if (Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Emulator)
                {
                    StatusTextBlock.Text = "Emulator mode";
                    if (lastKnownPos != null)
                    {
                        CurrentSpeed = (e.Position.Location.GetDistanceTo(lastKnownPos));
                        if (CurrentSpeed > 0)
                        {
                            var lat1 = Math.PI * lastKnownPos.Latitude / 180.0;
                            var lat2 = Math.PI * e.Position.Location.Latitude / 180.0;
                            var dLon = Math.PI * e.Position.Location.Longitude / 180.0 - Math.PI * lastKnownPos.Longitude / 180.0;
                            var y = Math.Sin(dLon) * Math.Cos(lat2);
                            var x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);
                            var brng = Math.Atan2(y, x);
                            CurrentCourse = (180.0 * brng / Math.PI + 360) % 360;
                        }
                    }
                    UpdateAverage();
                }

                if (lastKnownPos != null)
                {
                    double travelled = (e.Position.Location.GetDistanceTo(lastKnownPos)) / 1000;
                    distance = distance + travelled;
                }
                lastKnownPos = e.Position.Location;

            }
            else
            {
                StatusTextBlock.Text = "weak GPS signal";
                SpeedTextBlock.Text = "-";
                CurrentSpeed = 0;
                CurrentCourse = 0;
            }

            // if current speed is faster than maxspeed, override it
            if (CurrentSpeed > MaxSpeed)
            {
                MaxSpeed = CurrentSpeed;
            }

            UpdateSpeedChart();
            UpdateSpeed();
            UpdateMapRender();

        }

        void UpdateMapRender()
        {
            BackMap.Center = lastKnownPos;
            if (!Double.IsNaN(CurrentCourse))
            {
                Storyboard BackMapSB = new Storyboard();
                DoubleAnimation BackMapAngleAnim = new DoubleAnimation();
                BackMapAngleAnim.EasingFunction = new ExponentialEase { Exponent = 6, EasingMode = EasingMode.EaseOut };
                BackMapAngleAnim.Duration = new Duration(TimeSpan.FromSeconds(0.6));
                BackMapAngleAnim.To = 360 - CurrentCourse;
                Storyboard.SetTarget(BackMapAngleAnim, BackMapRotateTransform);
                Storyboard.SetTargetProperty(BackMapAngleAnim, new PropertyPath(RotateTransform.AngleProperty));
                BackMapSB.Children.Add(BackMapAngleAnim);
                BackMapSB.Begin();
            }
        }

        void UpdateSpeedChart()
        {
            SpeedGraph.Points.RemoveAt(0);

            for (int i = 0; i < SpeedGraph.Points.Count; i++)
            {
                SpeedGraph.Points[i] = new System.Windows.Point(SpeedGraph.Points[i].X - 1, SpeedGraph.Points[i].Y);
            }

            SpeedGraph.Points.Add(new System.Windows.Point(SpeedGraphMaxCount, -Math.Ceiling(CurrentSpeed)));

        }

        void UpdateAverage()
        {
            if (CurrentSpeed > 0)
            {
                if (prevSpeeds.Count > 0)
                {
                    double exponent = (2 / (prevSpeeds.Count + 1));
                    AvgSpeed = (CurrentSpeed * exponent) + (prevSpeeds.Average() * (1 - exponent));
                }
                else
                {
                    AvgSpeed = 0;
                }

                if (prevSpeeds.Count >= SpeedGraphMaxCount)
                {
                    prevSpeeds[1] = (prevSpeeds[0] + prevSpeeds[1]) / 2;
                    prevSpeeds.RemoveAt(0);
                }
                prevSpeeds.Add(CurrentSpeed);
            }

        }

        void UpdateSpeed()
        {
            if (!Double.IsNaN(CurrentSpeed))
            {
                double unitFactor = 1;

                if (unitConfig == "km")
                {
                    unitFactor = 1;
                }
                else if (unitConfig == "mi")
                {
                    unitFactor = 0.621371192;
                }

                double CurrentSpeedFactored = Math.Ceiling(CurrentSpeed * unitFactor);
                double AvgSpeedFactored = Math.Floor(AvgSpeed * unitFactor);
                double MaxSpeedFactored = Math.Ceiling(MaxSpeed * unitFactor);
                double DistanceFactored = Math.Round(distance * unitFactor, 1);

                SpeedTextBlock.Text = CurrentSpeedFactored.ToString();

                if (this.Orientation == PageOrientation.PortraitUp || this.Orientation == PageOrientation.PortraitDown)
                {
                    if (CurrentSpeedFactored >= 1000)
                    {
                        SpeedTextBlock.FontSize = 180;
                    }
                    else if (CurrentSpeedFactored >= 200)
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

                // only show compass if we're actually moving
                if (CurrentSpeed == 0)
                {
                    Direction.Opacity = 0;
                }
                else
                {
                    Direction.Opacity = 1;
                    DirectionTextBlock.Text = CourseDirection(CurrentCourse);
                    Storyboard DirectionSB = new Storyboard();
                    DoubleAnimation DirectionAnim = new DoubleAnimation();
                    DirectionAnim.EasingFunction = new ExponentialEase { Exponent = 6, EasingMode = EasingMode.EaseOut };
                    DirectionAnim.Duration = new Duration(TimeSpan.FromSeconds(0.6));
                    DirectionAnim.To = 360 - CurrentCourse;
                    Storyboard.SetTarget(DirectionAnim, DirectionRotateTransform);
                    Storyboard.SetTargetProperty(DirectionAnim, new PropertyPath(RotateTransform.AngleProperty));
                    DirectionSB.Children.Add(DirectionAnim);
                    DirectionSB.Begin();
                }

                // speed alert
                if (SpeedAlertConfig == "on" && CurrentSpeedFactored > SpeedAlertSpeed)
                {
                    if (!SpeedAlertTimer.IsEnabled)
                    {
                        SpeedAlertEffect.Play();

                        SpeedAlertTimer.Start();
                    }
                }
                else
                {
                    SpeedAlertTimer.Stop();
                }

            }
            else
            {
                SpeedTextBlock.Text = "-";
                Direction.Opacity = 0;
            }
        }

        private void LayoutRoot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            settingsTimer.Stop();
            VisualStateManager.GoToState(this, "ShowControls", true);
            settingsTimer.Tick +=
                delegate(object s, EventArgs args)
                {
                    VisualStateManager.GoToState(this, "HideControls", true);
                    settingsTimer.Stop();
                };

            settingsTimer.Interval = new TimeSpan(0, 0, 2);
            settingsTimer.Start();
        }

        void UpdateUnit()
        {
            switch (unitConfig)
            {
                case "km":
                    ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = "switch to imperial (mph)";
                    UnitTextBlock.Text = "km/h";
                    break;
                case "mi":
                    ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = "switch to metric (km/h)";
                    UnitTextBlock.Text = "mph";
                    break;
            }
        }

        private void windscreenButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            windscreenMode = !windscreenMode;
            UpdateWindscreen();
        }

        void UpdateWindscreen()
        {
            if (windscreenMode)
            {
                ContentScaleTransform.ScaleX = -1;
                if (!darkTheme)
                {
                    SolidColorBrush foregroundbrush = (SolidColorBrush)App.Current.Resources["PhoneForegroundBrush"];
                    SolidColorBrush backgroundbrush = (SolidColorBrush)App.Current.Resources["PhoneBackgroundBrush"];
                    SolidColorBrush subtlebrush = (SolidColorBrush)App.Current.Resources["PhoneSubtleBrush"];
                    SolidColorBrush disabledbrush = (SolidColorBrush)App.Current.Resources["PhoneDisabledBrush"];
                    foregroundbrush.Color = Colors.White;
                    backgroundbrush.Color = Colors.Black;
                    subtlebrush.Color = Colors.LightGray;
                    disabledbrush.Color = Colors.Gray;
                }
                SpeedTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["WindscreenColor"];
                DirectionTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["WindscreenColor"];
                DirectionIcon.Fill = (SolidColorBrush)App.Current.Resources["WindscreenColor"];
                SpeedChart.Visibility = System.Windows.Visibility.Collapsed;
                UnitTextBlock.Opacity = 0;
                windsreenIndicator.Visibility = System.Windows.Visibility.Visible;

                MapLayer.Visibility = System.Windows.Visibility.Collapsed;
                BackMap.Visibility = System.Windows.Visibility.Collapsed;
                BackMap.IsEnabled = false;
                mapButton.IsEnabled = false;
            }
            else
            {
                ContentScaleTransform.ScaleX = 1;
                if (!darkTheme)
                {
                    SolidColorBrush foregroundbrush = (SolidColorBrush)App.Current.Resources["PhoneForegroundBrush"];
                    SolidColorBrush backgroundbrush = (SolidColorBrush)App.Current.Resources["PhoneBackgroundBrush"];
                    SolidColorBrush subtlebrush = (SolidColorBrush)App.Current.Resources["PhoneSubtleBrush"];
                    SolidColorBrush disabledbrush = (SolidColorBrush)App.Current.Resources["PhoneDisabledBrush"];
                    foregroundbrush.Color = (System.Windows.Media.Color)App.Current.Resources["PhoneForegroundColor"];
                    backgroundbrush.Color = (System.Windows.Media.Color)App.Current.Resources["PhoneBackgroundColor"];
                    subtlebrush.Color = (System.Windows.Media.Color)App.Current.Resources["PhoneSubtleColor"];
                    disabledbrush.Color = (System.Windows.Media.Color)App.Current.Resources["PhoneDisabledColor"];
                }
                SpeedTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["PhoneForegroundBrush"];
                DirectionTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["PhoneAccentBrush"];
                DirectionIcon.Fill = (SolidColorBrush)App.Current.Resources["PhoneAccentBrush"];
                SpeedChart.Visibility = System.Windows.Visibility.Visible;
                UnitTextBlock.Opacity = 1;
                windsreenIndicator.Visibility = System.Windows.Visibility.Collapsed;
                UpdateMap();
            }
        }

        void UpdateMap()
        {
            if (errorMap)
            {
                mapConfig = "disabled";
            }

            switch (mapConfig)
            {
                case "on":
                    if (!windscreenMode)
                    {
                        MapLayer.Visibility = System.Windows.Visibility.Visible;
                        BackMap.Visibility = System.Windows.Visibility.Visible;
                        BackMap.IsEnabled = true;
                        mapButton.IsEnabled = true;
                    }
                    break;
                case "off":
                    if (!windscreenMode)
                    {
                        MapLayer.Visibility = System.Windows.Visibility.Collapsed;
                        BackMap.Visibility = System.Windows.Visibility.Collapsed;
                        BackMap.IsEnabled = false;
                        mapButton.IsEnabled = true;
                    }
                    break;
                case "disabled":
                    MapLayer.Visibility = System.Windows.Visibility.Collapsed;
                    BackMap.Visibility = System.Windows.Visibility.Collapsed;
                    BackMap.IsEnabled = false;
                    mapButton.IsEnabled = false;
                    break;
            }
        }

        private void mapButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (mapConfig)
            {
                case "off":
                    mapConfig = "on";
                    break;
                case "on":
                    mapConfig = "off";
                    break;
            }
            settings["map"] = mapConfig;
            settings.Save();
            UpdateMap();
        }

        private void GestureListener_PinchStarted(object sender, PinchStartedGestureEventArgs e)
        {
            mapScale = BackMap.ZoomLevel;
        }

        private void GestureListener_PinchDelta(object sender, PinchGestureEventArgs e)
        {
            double desiredZoom = mapScale * Math.Pow(e.DistanceRatio, 0.5);
            if (desiredZoom < 1)
            {
                BackMap.ZoomLevel = 1;
            }
            else if (desiredZoom > 19)
            {
                BackMap.ZoomLevel = 19;
            }
            else
            {
                BackMap.ZoomLevel = desiredZoom;
            }
        }

        private void GestureListener_PinchCompleted(object sender, PinchGestureEventArgs e)
        {
            mapScale = BackMap.ZoomLevel;
        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (PhoneApplicationService.Current.StartupMode == StartupMode.Activate)
            {
                var stateSettings = PhoneApplicationService.Current.State;
                distance = (double)stateSettings["distance"];
                MaxSpeed = (double)stateSettings["MaxSpeed"];
                AvgSpeed = (double)stateSettings["AvgSpeed"];
                windscreenMode = (bool)stateSettings["windscreenMode"];
                mapScale = (double)stateSettings["mapScale"];
                SpeedGraph.Points = (PointCollection)stateSettings["SpeedGraphPoints"];
                prevSpeeds = (List<Double>)stateSettings["prevSpeeds"];
                SpeedAlertConfig = (string)stateSettings["SpeedAlertConfig"];
                UpdateWindscreen();
                UpdateSpeedAlert();
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            var stateSettings = PhoneApplicationService.Current.State;
            stateSettings["distance"] = distance;
            stateSettings["MaxSpeed"] = MaxSpeed;
            stateSettings["AvgSpeed"] = AvgSpeed;
            stateSettings["windscreenMode"] = windscreenMode;
            stateSettings["mapScale"] = mapScale;
            stateSettings["SpeedGraphPoints"] = SpeedGraph.Points;
            stateSettings["prevSpeeds"] = prevSpeeds;
            stateSettings["SpeedAlertConfig"] = SpeedAlertConfig;
        }

        private void BackMap_LoadingError(object sender, LoadingErrorEventArgs e)
        {
            errorMap = true;
            UpdateMap();
        }

        void ResetSpeedGraph()
        {
            for (int i = 0; i <= SpeedGraphMaxCount; i++)
            {
                SpeedGraph.Points.Add(new System.Windows.Point(i, 0));
            }
        }

        private void ResetTrip_Click(object sender, EventArgs e)
        {
            distance = 0;
            MaxSpeed = 0;
            AvgSpeed = 0;
            SpeedGraph.Points.Clear();
            ResetSpeedGraph();
            prevSpeeds.Clear();
            UpdateAverage();
            UpdateSpeedChart();
            UpdateSpeed();
        }

        void UpdateLocationAccess()
        {
            if (LocationAccess == "off")
            {
                settings["LocationAccess"] = "off";
                watcher.Stop();
                ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = "enable location access";

                StatusTextBlock.Text = "location inaccessible";
                CurrentSpeed = Double.NaN;
                UpdateSpeed();
                mapConfig = "disabled";
                UpdateMap();
            }
            else
            {
                settings["LocationAccess"] = "on";
                watcher.Start();
                ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = "disable location access";
            }
            settings.Save();
        }


        private void DisableLocation_Click(object sender, EventArgs e)
        {
            if (LocationAccess == "on")
            {
                var warningMsg = MessageBox.Show("This application will not work without location access. Are you sure you still want to disable it?", "Disable location", MessageBoxButton.OKCancel);
                if (warningMsg == MessageBoxResult.OK)
                {
                    LocationAccess = "off";
                    UpdateLocationAccess();
                }
            }
            else
            {
                LocationAccess = "on";
                UpdateLocationAccess();
            }
        }

        private void About_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private void alertButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SpeedAlertConfig == "off")
            {
                AlertPopup alertPopup = new AlertPopup(Convert.ToInt32(SpeedAlertSpeed), UnitTextBlock.Text);
                PopupContent.Children.Add(alertPopup);
                LayoutRoot.IsHitTestVisible = false;
                ApplicationBar.IsVisible = false;

                alertPopup.closeCompleted += (int SpeedAlert) =>
                {
                    PopupContent.Children.Clear();
                    LayoutRoot.IsHitTestVisible = true;
                    ApplicationBar.IsVisible = true;
                    SpeedAlertSpeed = SpeedAlert;
                    settings["SpeedAlertSpeedConfig"] = SpeedAlertSpeed.ToString();
                    SpeedAlertConfig = "on";
                    settings["SpeedAlertConfig"] = SpeedAlertConfig;
                    settings.Save();
                    UpdateSpeedAlert();
                };
            }
            else
            {
                SpeedAlertConfig = "off";
                settings["SpeedAlertConfig"] = SpeedAlertConfig;
                settings.Save();
                UpdateSpeedAlert();
            }
            
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (PopupContent.Children.Count > 0)
            {
                Storyboard hidePopup = (Storyboard)App.Current.Resources["SwivelOut"];
                Storyboard.SetTarget(hidePopup, PopupHost);
                hidePopup.Begin();
                hidePopup.Completed += (object sender, EventArgs ea) =>
                    {
                        PopupContent.Children.Clear();
                        LayoutRoot.IsHitTestVisible = true;
                        ApplicationBar.IsVisible = true;
                        hidePopup.Stop();
                    };
                e.Cancel = true;
            }
        }

        void UpdateSpeedAlert()
        {
            if (SpeedAlertConfig == "on")
            {
                alertIndicator.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                alertIndicator.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void SwitchMetric_Click(object sender, EventArgs e)
        {
            switch (unitConfig)
            {
                case "km":
                    unitConfig = "mi";
                    break;
                case "mi":
                    unitConfig = "km";
                    break;
            }
            settings["unit"] = unitConfig;
            settings.Save();
            UpdateUnit();
            UpdateSpeed();
        }

        private void ApplicationBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
            ApplicationBar appbar = sender as ApplicationBar;
            if (e.IsMenuVisible)
            {
                appbar.Opacity = 0.8;
            }
            else {
                appbar.Opacity = 0;
            }

        }

        private void OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            // Switch to the visual state that corresponds to our target orientation
            VisualStateManager.GoToState(this, e.Orientation.ToString(), true);
        }


    }
}