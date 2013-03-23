using System;
using System.Threading.Tasks;
using System.Windows;

// TODO: Convert the speed limit when switching units

namespace Speedo.Controls
{
    public partial class AlertPopup : ObservableControl
    {
        private int alertSpeed;
        public int AlertSpeed
        {
            get { return alertSpeed; }
            set
            {
                SetProperty( ref alertSpeed, value );
                CloseCommand.OnCanExecuteChanged();
            }
        }

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

        public AlertPopup( SpeedAlert alert )
        {
            TensSource = new IntLoopingDataSource( 0, 24, 1 );
            UnitsSource = new IntLoopingDataSource( 0, 5, 5 ) { Loop = false };
            CloseCommand = new RelayCommand( _ => ClosePopup(), _ => AlertSpeed != 0 );

            Alert = alert;
            AlertSpeed = alert.Limit;
            UseSound = Alert.NotificationProvider == SpeedAlert.SoundProvider;

            InitializeComponent();
            LayoutRoot.DataContext = this;
            VisualStateManager.GoToState( this, "Open", true );
        }

        // can't be called Close() because of a collision with the "Close" visual state
        private async void ClosePopup()
        {
            VisualStateManager.GoToState( this, "Close", true );
            // TODO: Find a better way (without events)
            await Task.Delay( 350 );

            Alert.Limit = AlertSpeed;
            Alert.NotificationProvider = UseSound ? SpeedAlert.SoundProvider : SpeedAlert.SpeechProvider;

            FireCloseCompleted();
        }

        public event EventHandler CloseCompleted;
        private void FireCloseCompleted()
        {
            var evt = CloseCompleted;
            if ( evt != null )
            {
                evt( this, EventArgs.Empty );
            }
        }
    }
}