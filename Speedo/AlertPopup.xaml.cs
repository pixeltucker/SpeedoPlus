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
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using Microsoft.Phone.Controls.Primitives;

namespace Speedo
{
    public partial class AlertPopup : UserControl
    {
        int AlertSpeed;
        ILoopingSelectorDataSource s1;
        ILoopingSelectorDataSource s2;

        public AlertPopup(int prevAlertSpeed, string SpeedUnit)
        {
            InitializeComponent();

            AlertSpeed = prevAlertSpeed;
            TextSpeedUnit.Text = SpeedUnit;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync();

            VisualStateManager.GoToState(this, "Open", true);
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            s1 = new Speedo.Speeds();
            s2 = new Speedo.Speeds5();
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            speedAlertLoop1.DataSource = s1;
            speedAlertLoop2.DataSource = s2;

            int speed1 = AlertSpeed / 10;
            speedAlertLoop1.DataSource.SelectedItem = speed1;

            string AlertSpeedString = AlertSpeed.ToString();
            int speed2 = Convert.ToInt32(AlertSpeedString.Substring(AlertSpeedString.Length - 1, 1));
            speedAlertLoop2.DataSource.SelectedItem = speed2;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int speed1 = (int)speedAlertLoop1.DataSource.SelectedItem;
            int speed2 = (int)speedAlertLoop2.DataSource.SelectedItem;
            AlertSpeed = speed1 * 10 + speed2;

            if (AlertSpeed == 0)
            {
                MessageBox.Show("Sorry, the speed alert cannot be set for 0");
            }
            else
            {
                VisualStateManager.GoToState(this, "Close", true);
            }
        }

        public void Hide()
        {
            AlertSpeed = 0;
            VisualStateManager.GoToState(this, "Close", true);
        }

        public delegate void MyAwesomeEventHandler(int alertspeed);
        public event MyAwesomeEventHandler closeCompleted;

        private void fireClosedCompleted(object sender, EventArgs e)
        {
            closeCompleted(AlertSpeed);
        }

    }
}
