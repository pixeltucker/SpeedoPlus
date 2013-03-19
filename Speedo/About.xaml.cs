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
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Marketplace;

namespace Speedo
{
    public partial class About : PhoneApplicationPage
    {
        public About()
        {
            InitializeComponent();

            bool appTrial = new LicenseInformation().IsTrial();
            if (appTrial)
            {
                freetext.Visibility = System.Windows.Visibility.Visible;
                paidtext.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                freetext.Visibility = System.Windows.Visibility.Collapsed;
                paidtext.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void ReviewApp_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }

        private void BuyApp_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();
            marketplaceDetailTask.ContentType = MarketplaceContentType.Applications;
            marketplaceDetailTask.Show();
        }

        private void Awesome_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Very awesome.","How awesome?",MessageBoxButton.OK);
        }
    }
}