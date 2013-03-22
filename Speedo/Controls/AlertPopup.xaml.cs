using System;
using System.Threading.Tasks;
using System.Windows;

// TODO: there's probably a better way to show units than by converting them to string here...
// but there are no custom markupextensions :(

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

        public string Unit { get; private set; }

        public IntLoopingDataSource UnitsSource { get; private set; }
        public IntLoopingDataSource TensSource { get; private set; }
        public RelayCommand CloseCommand { get; private set; }

        public AlertPopup( SpeedUnit unit )
        {
            Unit = SpeedUtils.GetString( unit );

            TensSource = new IntLoopingDataSource( 0, 24, 1 );
            UnitsSource = new IntLoopingDataSource( 0, 5, 5 ) { Loop = false };
            CloseCommand = new RelayCommand( _ => ClosePopup(), _ => AlertSpeed != 0 );

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