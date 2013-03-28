using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace Speedo
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public ICommand WebsiteCommand { get; private set; }
        public ICommand AwesomeCommand { get; private set; }
        public ICommand ReviewAppCommand { get; private set; }
        public ICommand ShowPrivacyPolicyCommand { get; private set; }

        public AboutPage()
        {
            DataContext = this;
            WebsiteCommand = new RelayCommand( ExecuteWebsiteCommand );
            ReviewAppCommand = new RelayCommand( ExecuteReviewAppCommand );
            AwesomeCommand = new RelayCommand( ExecuteAwesomeCommand );
            ShowPrivacyPolicyCommand = new RelayCommand( ExecuteShowPrivacyPolicyCommand );
            InitializeComponent();
        }

        private void ExecuteWebsiteCommand( object parameter )
        {
            new WebBrowserTask { Uri = new Uri( (string) parameter ) }.Show();
        }

        private void ExecuteAwesomeCommand( object parameter )
        {
            MessageBox.Show( "Very awesome.", "How awesome?", MessageBoxButton.OK );
        }

        private void ExecuteReviewAppCommand( object parameter )
        {
            new MarketplaceReviewTask().Show();
        }

        private void ExecuteShowPrivacyPolicyCommand( object parameter )
        {
            App.Current.ShowPrivacyPolicy();
        }
    }
}