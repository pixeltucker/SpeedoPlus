// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Long Zheng, Solal Pirelli

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Speedo
{
    public partial class AlertPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        private SpeedAlert alert;

        private bool useSound;
        public bool UseSound
        {
            get { return useSound; }
            set { SetProperty( ref useSound, value ); }
        }

        private int limit;
        public int Limit
        {
            get { return limit; }
            set { SetProperty( ref limit, value ); }
        }

        public SpeedUnit Unit
        {
            get { return AppSettings.Current.SpeedUnit; }
        }

        public IntLoopingDataSource UnitsSource { get; private set; }
        public IntLoopingDataSource TensSource { get; private set; }
        public RelayCommand CloseCommand { get; private set; }

        public AlertPage()
        {
            // HACK: simplest way to pass parameters
            alert = (SpeedAlert) PhoneApplicationService.Current.State["SpeedAlert"];
            UseSound = alert.NotificationProvider == SpeedAlert.SoundProvider;
            Limit = AppSettings.Current.SpeedLimit;

            TensSource = new IntLoopingDataSource( 0, 24, 1 );
            UnitsSource = new IntLoopingDataSource( 0, 5, 5 ) { Loop = false };
            CloseCommand = new RelayCommand( ExecuteCloseCommand, CanExecuteCloseCommand );
            CloseCommand.BindToPropertyChange( this, "Limit" );

            DataContext = this;
            InitializeComponent();
        }

        private void ExecuteCloseCommand( object parameter )
        {
            alert.NotificationProvider = UseSound ? SpeedAlert.SoundProvider : SpeedAlert.SpeechProvider;
            AppSettings.Current.IsSpeedAlertEnabled = true;
            AppSettings.Current.SpeedLimit = limit;
            NavigationService.GoBack();
        }

        private bool CanExecuteCloseCommand( object parameter )
        {
            return Limit != 0;
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