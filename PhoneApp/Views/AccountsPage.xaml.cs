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
    public partial class AccountsPage : PhoneApplicationPage
    {
        public AccountsPage()
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (Controllers.UsersController.AuthenticatedUser != null)
            {
                CurrentUserBox.Text = Controllers.UsersController.AuthenticatedUser.Username;
            }
            UpdateItemsSource();
        }

        private void UpdateItemsSource()
        {
            try
            {
                using (var context = new Database.DatabaseContext(Database.DatabaseContext.DBConnectionString))
                {
                    listUsers.ItemsSource = context.Users.Where(u => !u.LoggedIn).OrderBy(u => u.Username);
                }
            }
            catch (Exception ex)
            {
                Controllers.LogsController.WriteException("ShowUsers", ex.Message);
            }
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/ChooseServerPage.xaml", UriKind.Relative));
        }

        private void listUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                Database.User item = (Database.User)e.AddedItems[0];

                Controllers.UsersController.SwitchUser(item.Username);
                NavigationService.Navigate(new Uri("/Views/MainPanoramaPage.xaml", UriKind.Relative));
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var user = ((sender as MenuItem).DataContext) as Database.User;

            if (user != null)
            {
                Controllers.UsersController.RemoveUser(user.Username);
                UpdateItemsSource();
            }
        }
    }
}