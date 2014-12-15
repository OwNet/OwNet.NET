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

namespace PhoneApp.Views
{
    public partial class MainPanoramaPage : PhoneApplicationPage, Helpers.INotifiableObject
    {
        private Controllers.GroupsController _groupsController = new Controllers.GroupsController();
        private Controllers.SharedFilesController _sharedFileController = new Controllers.SharedFilesController();
        private Controllers.LocalGroupsController _localGroupsController = new Controllers.LocalGroupsController();

        public MainPanoramaPage()
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateListSource();
            _groupsController.NotifiableObject = this;
            _groupsController.GetGroups();

            listSharedFiles.ItemsSource = new List<ServiceEntities.SharedFolder>()
            {
                new ServiceEntities.SharedFolder()
                {
                    Name = "Server not found",
                    Description = "Local server was not found, tap to refresh",
                    IsRealItem = false
                }
            };

            _sharedFileController.NotifiableObject = new SharedFilesNotifier(this);
            _sharedFileController.GetSharedFiles();

            _localGroupsController.NotifiableObject = this;
            _localGroupsController.GetLocalGroups();
        }

        public void NotifyFinished()
        {
            UpdateListSource();
        }

        public void NotifyFailed()
        { }

        private void UpdateListSource()
        {
            try
            {
                using (var context = new Database.DatabaseContext(Database.DatabaseContext.DBConnectionString))
                {
                    this.listGroups.ItemsSource = from u in context.Users.Where(u => u.LoggedIn)
                                                  from ug in u.UserGroups
                                                  orderby ug.Group.Name
                                                  select ug.Group;
                }
            }
            catch (Exception ex)
            {
                Controllers.LogsController.WriteException("LoadGroups", ex.Message);
            }
        }

        private void listGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                Database.Group item = (Database.Group)e.AddedItems[0];

                NavigationService.Navigate(new Uri(string.Format("/Views/GroupPage.xaml?groupId={0}", item.Id), UriKind.Relative));
            }
        }

        private void listSharedFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ServiceEntities.SharedItem item = (ServiceEntities.SharedItem)e.AddedItems[0];

                if (!item.IsRealItem)
                {
                    _sharedFileController.GetSharedFiles();
                    _localGroupsController.GetLocalGroups();
                }
                else if (item.IsFolder)
                {
                    Controllers.SharedFilesController.CurrentFolder = (ServiceEntities.SharedFolder)item;
                    NavigationService.Navigate(new Uri("/Views/SharedFolderPage.xaml", UriKind.Relative));
                }
                else
                {
                    Controllers.SharedFilesController.OpenFile((ServiceEntities.SharedFile)item);
                }
            }
        }

        internal void NotifySharedFilesFinished()
        {
            listSharedFiles.ItemsSource = _sharedFileController.RootFolder.AllItems;
        }

        private void AccountsBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/AccountsPage.xaml", UriKind.Relative));
        }

        private void LocalServerBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/LocalServerConfigPage.xaml", UriKind.Relative));
        }

        class SharedFilesNotifier : Helpers.INotifiableObject
        {
            MainPanoramaPage _parent = null;

            public SharedFilesNotifier(MainPanoramaPage parent)
            {
                _parent = parent;
            }

            public void NotifyFinished()
            {
                _parent.NotifySharedFilesFinished();
            }

            public void NotifyFailed()
            { }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            listSharedFiles.ItemsSource = new List<ServiceEntities.SharedFolder>();
            _sharedFileController.Refresh();
            _groupsController.GetGroups();
            _localGroupsController.GetLocalGroups();
        }
    }
}