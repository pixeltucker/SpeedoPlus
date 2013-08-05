// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Solal Pirelli

using System.ComponentModel;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;

namespace Speedo
{
    public class AppSettings : INotifyPropertyChanged
    {
        private static readonly bool UseMiles = CultureInfo.CurrentCulture.Name == "en-US";

        private IsolatedStorageSettings settings;

        public bool IsFirstRun
        {
            get { return Get<bool>( defaultValue: true ); }
            set { Set( value ); }
        }

        public SpeedUnit SpeedUnit
        {
            get { return Get<SpeedUnit>( defaultValue: UseMiles ? SpeedUnit.Miles : SpeedUnit.Kilometers ); }
            set { Set( value ); }
        }

        public MapStatus MapStatus
        {
            get { return Get<MapStatus>( defaultValue: MapStatus.On ); }
            set { Set( value ); }
        }

        public bool IsSpeedAlertEnabled
        {
            get { return Get<bool>( defaultValue: false ); }
            set { Set( value ); }
        }

        public int SpeedLimit
        {
            get { return Get<int>( defaultValue: UseMiles ? 65 : 100 ); }
            set { Set( value ); }
        }

        public bool AllowLocationAccess
        {
            get { return Get<bool>( defaultValue: true ); }
            set { Set( value ); }
        }

        public static AppSettings Current { get; private set; }

        static AppSettings()
        {
            Current = new AppSettings();
        }

        private AppSettings()
        {
            if ( !DesignerProperties.IsInDesignTool ) // doesn't work in Cider :(
            {
                settings = IsolatedStorageSettings.ApplicationSettings;
#if DEBUG
                settings.Clear();
#endif
            }
        }

        private T Get<T>( T defaultValue, [CallerMemberName] string key = "" )
        {
            if ( !settings.Contains( key ) )
            {
                Set( defaultValue, key );
            }
            return (T) settings[key];
        }

        private void Set( object value, [CallerMemberName] string key = "" )
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
            NotifyHelper.OnPropertyChanged( key, this, PropertyChanged );
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}