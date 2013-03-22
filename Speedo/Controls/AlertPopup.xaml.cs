using System;
using System.Threading.Tasks;
using System.Windows;

// TODO: Remove SpeedUnit, inject it via app settings

namespace Speedo.Controls
{
    public partial class AlertPopup : ObservableControl
    {
        private int alertspeed;
        public int AlertSpeed
        {
            get { return alertspeed; }
            set
            {
                SetProperty( ref alertspeed, value );
                CloseCommand.OnCanExecuteChanged();
            }
        }

        public IntLoopingDataSource UnitsSource { get; private set; }
        public IntLoopingDataSource TensSource { get; private set; }
        public RelayCommand CloseCommand { get; private set; }

        public AlertPopup( string SpeedUnit )
        {
            TensSource = new IntLoopingDataSource( 0, 24, 1 );
            UnitsSource = new IntLoopingDataSource( 0, 5, 5 ) { Loop = false };
            CloseCommand = new RelayCommand( _ => ClosePopup(), _ => AlertSpeed != 0 );

            InitializeComponent();
            LayoutRoot.DataContext = this;

            TextSpeedUnit.Text = SpeedUnit;
            VisualStateManager.GoToState( this, "Open", true );
        }

        // can't be called Close() because of a collision with the "Close" visual state
        private async void ClosePopup()
        {
            VisualStateManager.GoToState( this, "Close", true );
            // TODO: Find a better way (without events)
            await Task.Delay( 350 );
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