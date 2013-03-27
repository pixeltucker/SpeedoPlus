// new

using System.ComponentModel;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;

namespace Speedo
{
    public class AppSettings : INotifyPropertyChanged
    {
        private const string IsFirstRunKey = "IsFirstRun";
        private const string SpeedUnitKey = "SpeedUnit";
        private const string MapStatusKey = "MapStatus";
        private const string IsSpeedAlertEnabledKey = "IsSpeedAlertEnabled";
        private const string SpeedLimitKey = "SpeedLimit";
        private const string AllowLocationAccessKey = "AllowLocationAccess";

        private IsolatedStorageSettings settings;

        public bool IsFirstRun
        {
            get { return (bool) settings[IsFirstRunKey]; }
            set { SetSetting( IsFirstRunKey, value ); }
        }

        public SpeedUnit SpeedUnit
        {
            get { return (SpeedUnit) settings[SpeedUnitKey]; }
            set { SetSetting( SpeedUnitKey, value ); }
        }

        public MapStatus MapStatus
        {
            get { return (MapStatus) settings[MapStatusKey]; }
            set { SetSetting( MapStatusKey, value ); }
        }

        public bool IsSpeedAlertEnabled
        {
            get { return (bool) settings[IsSpeedAlertEnabledKey]; }
            set { SetSetting( IsSpeedAlertEnabledKey, value ); }
        }

        public int SpeedLimit
        {
            get { return (int) settings[SpeedLimitKey]; }
            set { SetSetting( SpeedLimitKey, value ); }
        }

        public bool AllowLocationAccess
        {
            get { return (bool) settings[AllowLocationAccessKey]; }
            set { SetSetting( AllowLocationAccessKey, value ); }
        }

        public static AppSettings Current { get; private set; }

        static AppSettings()
        {
            Current = new AppSettings();
        }

        private AppSettings()
        {
            if ( DesignerProperties.IsInDesignTool )
            {
                return; // doesn't work in Cider :(
            }

            settings = IsolatedStorageSettings.ApplicationSettings;
#if DEBUG
            settings.Clear();
#endif

            if ( !settings.Contains( IsFirstRunKey ) )
            {
                IsFirstRun = true;
            }

            if ( !settings.Contains( SpeedUnitKey ) )
            {
                SpeedUnit = CultureInfo.CurrentCulture.Name == "en-US" ? SpeedUnit.Miles : SpeedUnit.Kilometers;
            }

            if ( !settings.Contains( MapStatusKey ) )
            {
                MapStatus = MapStatus.On;
            }

            if ( !settings.Contains( IsSpeedAlertEnabledKey ) )
            {
                IsSpeedAlertEnabled = false;
            }

            if ( !settings.Contains( SpeedLimitKey ) )
            {
                SpeedLimit = SpeedUnit == SpeedUnit.Miles ? 65 : 100;
            }

            if ( !settings.Contains( AllowLocationAccessKey ) )
            {
                AllowLocationAccess = true;
            }
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void SetSetting( string key, object value, [CallerMemberName] string propertyName = "" )
        {
            if ( !settings.Contains( key ) )
            {
                settings.Add( key, value );
            }
            else
            {
                settings[key] = value;
            }

            settings.Save();
            NotifyHelper.OnPropertyChanged( propertyName, this, PropertyChanged );
        }
        #endregion
    }
}