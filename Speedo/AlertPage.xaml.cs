using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

// TODO: Convert the speed limit when switching units

namespace Speedo
{
    public partial class AlertPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        private bool useSound;
        public bool UseSound
        {
            get { return useSound; }
            set { SetProperty( ref useSound, value ); }
        }

        public SpeedAlert Alert { get; private set; }

        public IntLoopingDataSource UnitsSource { get; private set; }
        public IntLoopingDataSource TensSource { get; private set; }
        public RelayCommand CloseCommand { get; private set; }

        public AlertPage()
        {
            TensSource = new IntLoopingDataSource( 0, 24, 1 );
            UnitsSource = new IntLoopingDataSource( 0, 5, 5 ) { Loop = false };
            CloseCommand = new RelayCommand( ExecuteCloseCommand, CanExecuteCloseCommand );

            // HACK: simplest way to pass parameters
            Alert = (SpeedAlert) PhoneApplicationService.Current.State["SpeedAlert"];
            UseSound = Alert.NotificationProvider == SpeedAlert.SoundProvider;

            DataContext = this;
            InitializeComponent();
        }

        private void ExecuteCloseCommand( object parameter )
        {
            Alert.NotificationProvider = UseSound ? SpeedAlert.SoundProvider : SpeedAlert.SpeechProvider;
            NavigationService.GoBack();
        }

        private bool CanExecuteCloseCommand( object parameter )
        {
            return Alert != null && Alert.Limit != 0;
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