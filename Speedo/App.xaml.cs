// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Long Zheng, Solal Pirelli

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Speedo.Controls;
using Speedo.Languages;

namespace Speedo
{
    public partial class App : Application
    {
        private bool lightThemeEnabled;

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        new public static App Current
        {
            get { return (App) Application.Current; }
        }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            lightThemeEnabled = (Visibility) Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

            // Show graphics profiling information while debugging.
            if ( Debugger.IsAttached )
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                // Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }
        }

        public void ShowPrivacyPolicy()
        {
            MessageBox.Show( AppResources.PrivacyPolicyMessage, AppResources.PrivacyPolicyCaption, MessageBoxButton.OK );
        }

        public void ForceDarkTheme()
        {
            if ( lightThemeEnabled )
            {
                ( (SolidColorBrush) Resources["PhoneForegroundBrush"] ).Color = Colors.White;
                ( (SolidColorBrush) Resources["PhoneBackgroundBrush"] ).Color = Colors.Black;
                ( (SolidColorBrush) Resources["PhoneSubtleBrush"] ).Color = Colors.LightGray;
                ( (SolidColorBrush) Resources["PhoneDisabledBrush"] ).Color = Colors.Gray;
            }
        }

        public void AllowLightTheme()
        {
            if ( lightThemeEnabled )
            {
                ( (SolidColorBrush) Resources["PhoneForegroundBrush"] ).Color = (Color) Resources["PhoneForegroundColor"];
                ( (SolidColorBrush) Resources["PhoneBackgroundBrush"] ).Color = (Color) Resources["PhoneBackgroundColor"];
                ( (SolidColorBrush) Resources["PhoneSubtleBrush"] ).Color = (Color) Resources["PhoneSubtleColor"];
                ( (SolidColorBrush) Resources["PhoneDisabledBrush"] ).Color = (Color) Resources["PhoneDisabledColor"];
            }
        }

        public void EnableWindscreenColors()
        {
            var color = ( (SolidColorBrush) Resources["WindscreenBrush"] ).Color;
            ( (SolidColorBrush) Resources["ImportantForegroundBrush"] ).Color = color;
            ( (SolidColorBrush) Resources["ImportantAccentBrush"] ).Color = color;
        }

        public void DisableWindscreenColors()
        {
            ( (SolidColorBrush) Resources["ImportantForegroundBrush"] ).Color = (Color) Resources["PhoneForegroundColor"];
            ( (SolidColorBrush) Resources["ImportantAccentBrush"] ).Color = (Color) Resources["PhoneAccentColor"];
        }

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if ( phoneApplicationInitialized )
            {
                return;
            }

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new OrientationChangingFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;

            InitializeLanguage();
        }

        private void InitializeLanguage()
        {
            try
            {
                RootFrame.Language = XmlLanguage.GetLanguage( AppResources.ResourceLanguage );
                RootFrame.FlowDirection = (FlowDirection) Enum.Parse( typeof( FlowDirection ), AppResources.ResourceFlowDirection );
            }
            catch
            {
                if ( Debugger.IsAttached )
                {
                    Debugger.Break();
                }

                throw;
            }
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication( object sender, NavigationEventArgs e )
        {
            // Set the root visual to allow the application to render
            if ( RootVisual != RootFrame )
            {
                RootVisual = RootFrame;
            }

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed( object sender, NavigationFailedEventArgs e )
        {
            if ( Debugger.IsAttached )
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException( object sender, ApplicationUnhandledExceptionEventArgs e )
        {
            if ( Debugger.IsAttached )
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }
    }
}