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
    public partial class ChooseServerPage : PhoneApplicationPage, Helpers.INotifiableObject
    {
        private Controllers.ServersController _serversController = new Controllers.ServersController();

        public ChooseServerPage()
        {
            InitializeComponent();

            _serversController.NotifiableObject = this;
            _serversController.GetAllServers();
        }

        void tap_JumpListItem(object sender, System.Windows.Input.GestureEventArgs e)
        {
            TextBlock hb = sender as TextBlock;
            var server = from s in _serversController.ServerInfos
                         where s.ServerName == hb.Text
                         select s;

            if (server.Any())
            {
                Controllers.ServersController.SelectedServer = server.First();
                NavigationService.Navigate(new Uri("/Views/LoginPage.xaml", UriKind.Relative));
            }
        }

        public void NotifyFinished()
        {
            if (_serversController.ServerInfos == null)
                return;

            List<JumpItem> source = new List<JumpItem>();

            foreach (var server in _serversController.ServerInfos)
            {
                if (server.ServerName == "")
                    continue;

                source.Add(new JumpItem()
                {
                    ServerInfo = server,
                    Name = server.ServerName,
                    GroupBy = server.ServerName.Substring(0, 1)
                });
            }

            var groupBy = from jumpdemo in source
                          group jumpdemo by jumpdemo.GroupBy into c
                          orderby c.Key
                          select new JumpGroup<JumpItem>(c.Key, c);

            this.citiesListGropus.ItemsSource = groupBy;
        }

        public void NotifyFailed()
        { }
    }

    public class JumpItem
    {
        public string Name { get; set; }
        public PhoneAppCentralService.ServerInfo ServerInfo { get; set; }
        public string GroupBy { get; set; }
    }

    public class JumpGroup<T> : IEnumerable<T>
    {
        public JumpGroup(string name, IEnumerable<T> items)
        {
            this.Title = name;
            this.Items = new List<T>(items);
        }

        public override bool Equals(object obj)
        {
            JumpGroup<T> that = obj as JumpGroup<T>;
            return (that != null) && (this.Title.Equals(that.Title));
        }

        public string Title { get; set; }

        public IList<T> Items { get; set; }

        #region IEnumerable<T> Members
        public IEnumerator<T> GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }
        #endregion
    }
}