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

namespace PhoneApp.Views
{
    public partial class LocalServerConfigPage : PhoneApplicationPage, Helpers.INotifiableObject
    {
        public LocalServerConfigPage()
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(LocalServerConfigPage_Loaded);
        }

        void LocalServerConfigPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (Controllers.UsersController.AuthenticatedUser != null)
            {
                ServerAddressBox.Text = Controllers.ServersController.CurrentServerAddress;
            }
        }

        private void CheckAndSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckAndSaveBtn.IsEnabled = false;
            var ping = new Helpers.LocalServerPing();
            ping.NotifiableObject = this;
            ping.Ping(ServerAddressBox.Text);
        }

        public void NotifyFinished()
        {
            Controllers.ServersController.SetServerAddress(Controllers.UsersController.AuthenticatedUser.ServerUsername,
                ServerAddressBox.Text);
            Controllers.Multicasts.ServerInfoMulticastReceiver.Stop();
            NavigationService.Navigate(new Uri("/Views/MainPanoramaPage.xaml", UriKind.Relative));
        }

        public void NotifyFailed()
        {
            MessageBox.Show("Failed to access the server address.");
            CheckAndSaveBtn.IsEnabled = true;
        }
    }
}