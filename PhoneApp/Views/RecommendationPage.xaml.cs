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
    public partial class RecommendationPage : PhoneApplicationPage, Helpers.INotifiableObject
    {
        int _recommendationId = 0;
        string _absoluteUri = "";
        Controllers.RecommendationController _recommendationController = null;

        public RecommendationPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string recommedationIdStr = "";

            if (NavigationContext.QueryString.TryGetValue("recommendationId", out recommedationIdStr))
            {
                _recommendationId = Convert.ToInt32(recommedationIdStr);

                try
                {
                    using (var context = new Database.DatabaseContext(Database.DatabaseContext.DBConnectionString))
                    {
                        var recommendations = context.Recommendations.Where(r => r.Id == _recommendationId);
                        if (recommendations.Any())
                        {
                            var recommendation = recommendations.First();
                            _absoluteUri = recommendation.AbsoluteUri;
                            RecommendationTitle.Text = recommendation.Title;
                            _recommendationController = new Controllers.RecommendationController(recommendation);
                            _recommendationController.NotifiableObject = this;
                        }
                        _recommendationController.GetWebContent();
                    }
                }
                catch (Exception ex)
                {
                    Controllers.LogsController.WriteException("GetGroup", ex.Message);
                }
            }
        }

        public void NotifyFinished()
        {
            RecommendationWebBrowser.Navigated += new EventHandler<System.Windows.Navigation.NavigationEventArgs>(RecommendationWebBrowser_Navigated);
            RecommendationWebBrowser.Navigate(new Uri(Controllers.RecommendationController.GetRecommendationFilePath(_absoluteUri), UriKind.Relative));
        }

        void RecommendationWebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            RecommendationWebBrowser.Navigating += new EventHandler<NavigatingEventArgs>(RecommendationWebBrowser_Navigating);
            RecommendationWebBrowser.Base = _absoluteUri;
        }

        void RecommendationWebBrowser_Navigating(object sender, NavigatingEventArgs e)
        {
            if (e.Uri.OriginalString.StartsWith("Recommendations") && e.Uri.OriginalString.EndsWith(".recommendation.html"))
            {
                e.Cancel = true;
            }
            else
            {
                var task = new WebBrowserTask();
                string uri = e.Uri.OriginalString;
                if (uri.StartsWith("Recommendations"))
                {
                    uri = uri.Remove(0, "Recommendations\\".Length);
                    string[] splitUri = _absoluteUri.Split('/');
                    string lastPart = splitUri.Last();
                    string baseUri = _absoluteUri;

                    if (lastPart.Contains('.'))
                        baseUri = string.Join("/", splitUri.Take(splitUri.Length - 1));
                    if (!baseUri.EndsWith("/"))
                        baseUri += "/";

                    uri = baseUri + uri;
                }
                task.Uri = new Uri(uri);
                task.Show();
                e.Cancel = true;
            }
        }

        public void NotifyFailed()
        { }

        private void RecommendationTitle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.Uri = new Uri(_absoluteUri);
            task.Show();
        }
    }
}