using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace Speedo
{
    public partial class About : PhoneApplicationPage
    {
        public RelayCommand WebsiteCommand { get; private set; }
        public RelayCommand ReviewAppCommand { get; private set; }
        public RelayCommand AwesomeCommand { get; private set; }

        public About()
        {
            DataContext = this;
            WebsiteCommand = new RelayCommand( ExecuteWebsiteCommand );
            ReviewAppCommand = new RelayCommand( ExecuteReviewAppCommand );
            AwesomeCommand = new RelayCommand( ExecuteAwesomeCommand );
            InitializeComponent();
        }

        private void ExecuteWebsiteCommand( object parameter )
        {
            new WebBrowserTask { Uri = new Uri( (string) parameter ) }.Show();
        }

        private void ExecuteReviewAppCommand( object parameter )
        {
            new MarketplaceReviewTask().Show();
        }

        private void ExecuteAwesomeCommand( object parameter )
        {
            MessageBox.Show( "Very awesome.", "How awesome?", MessageBoxButton.OK );
        }
    }
}