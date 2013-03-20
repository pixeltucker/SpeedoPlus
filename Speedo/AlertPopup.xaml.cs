using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls.Primitives;

namespace Speedo
{
    public partial class AlertPopup : UserControl
    {
        public int AlertSpeed { get; private set; }

        private ILoopingSelectorDataSource speeds, speeds5;

        public AlertPopup( int prevAlertSpeed, string SpeedUnit )
        {
            InitializeComponent();

            AlertSpeed = prevAlertSpeed;
            TextSpeedUnit.Text = SpeedUnit;

            CreateDataSources();

            VisualStateManager.GoToState( this, "Open", true );
        }

        private async void CreateDataSources()
        {
            // TODO: check if these are slow enough to deserve this...
            await Task.Run( () =>
            {
                speeds = new Speeds();
                speeds5 = new Speeds5();
            } );
            speedAlertLoop1.DataSource = speeds;
            speedAlertLoop2.DataSource = speeds5;

            int speed1 = AlertSpeed / 10;
            speedAlertLoop1.DataSource.SelectedItem = speed1;

            string AlertSpeedString = AlertSpeed.ToString();
            int speed2 = int.Parse( AlertSpeedString.Substring( AlertSpeedString.Length - 1, 1 ) );
            speedAlertLoop2.DataSource.SelectedItem = speed2;
        }

        private void Button_Click( object sender, RoutedEventArgs e )
        {
            int speed1 = (int) speedAlertLoop1.DataSource.SelectedItem;
            int speed2 = (int) speedAlertLoop2.DataSource.SelectedItem;
            AlertSpeed = speed1 * 10 + speed2;

            if ( AlertSpeed == 0 )
            {
                MessageBox.Show( "Sorry, the speed alert cannot be set for 0" );
            }
            else
            {
                VisualStateManager.GoToState( this, "Close", true );
            }
        }

        public void Hide()
        {
            AlertSpeed = 0;
            VisualStateManager.GoToState( this, "Close", true );
        }

        public event EventHandler CloseCompleted;
        private void FireClosedCompleted( object sender, EventArgs e )
        {
            var evt = CloseCompleted;
            if ( evt != null )
            {
                evt( this, EventArgs.Empty );
            }
        }
    }
}