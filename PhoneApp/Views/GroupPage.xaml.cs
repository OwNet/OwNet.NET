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
    public partial class GroupPage : PhoneApplicationPage, Helpers.INotifiableObject
    {
        private int _groupId = 0;
        private Controllers.GroupController _groupController = new Controllers.GroupController();

        public GroupPage()
        {
            InitializeComponent();

            _groupController.NotifiableObject = this;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string groupIdStr = "";

            if (NavigationContext.QueryString.TryGetValue("groupId", out groupIdStr))
            {
                _groupId = Convert.ToInt32(groupIdStr);

                UpdateListSource();
                _groupController.GetRecommendations(_groupId);
            }
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
                    var groups = context.Groups.Where(g => g.Id == _groupId);
                    if (groups.Any())
                    {
                        var group = groups.First();
                        GroupName.Text = group.Name;
                        listRecommendations.ItemsSource = group.Recommendations.OrderByDescending(r => r.DateCreated);
                    }
                }
            }
            catch (Exception ex)
            {
                Controllers.LogsController.WriteException("LoadGroups", ex.Message);
            }
        }

        private void listRecommendations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                Database.Recommendation item = (Database.Recommendation)e.AddedItems[0];

                NavigationService.Navigate(new Uri(string.Format("/Views/RecommendationPage.xaml?recommendationId={0}", item.Id), UriKind.Relative));
            }
        }
    }
}