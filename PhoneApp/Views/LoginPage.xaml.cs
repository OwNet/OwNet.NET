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
    public partial class LoginPage : PhoneApplicationPage, Helpers.INotifiableObject
    {
        Controllers.UsersController _usersController = new Controllers.UsersController();

        public LoginPage()
        {
            InitializeComponent();

            SchoolNameBlock.Text = Controllers.ServersController.SelectedServer.ServerName;
            _usersController.NotifiableObject = this;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginButton.IsEnabled = false;
            _usersController.Authenticate(UsernameBox.Text, PasswordBox.Password);
        }

        public void NotifyFinished()
        {
            LoginButton.IsEnabled = true;
            if (Controllers.UsersController.AuthenticatedUser == null)
            {
                MessageBox.Show("Failed to authenticate the user using the given credentials.");
            }
            else
            {
                NavigationService.Navigate(new Uri("/Views/MainPanoramaPage.xaml", UriKind.Relative));
            }
        }

        public void NotifyFailed()
        {
            MessageBox.Show("Failed to authenticate the user using the given credentials.");
        }
    }
}