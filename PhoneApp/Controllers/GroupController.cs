using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PhoneApp.Controllers
{
    public class GroupController : Helpers.NotifierObject
    {
        DateTime _recommendationsUpdateTime = DateTime.Now;
        int _groupId = 0;

        public void GetRecommendations(int groupId)
        {
            _groupId = groupId;
            try
            {
                using (var context = new Database.DatabaseContext(Database.DatabaseContext.DBConnectionString))
                {
                    var group = context.Groups.FirstOrDefault(g => g.Id == groupId);
                    if (group == null)
                        return;

                    _recommendationsUpdateTime = DateTime.Now;

                    if (group.IsGlobal)
                    {
                        var client = new PhoneAppCentralService.PhoneAppServiceClient();
                        client.GetGroupRecommendationsAsync(groupId, group.LastRecommendationsUpdate, UsersController.AuthenticatedUser);
                        client.GetGroupRecommendationsCompleted += new EventHandler<PhoneAppCentralService.GetGroupRecommendationsCompletedEventArgs>(GetGroupRecommendationsCompleted);
                    }
                    else
                    {
                        WebClient serviceRequest = new WebClient();
                        serviceRequest.DownloadStringAsync(new Uri(ServersController.GetServiceAddress(
                            string.Format("groups/recommendations?group={0}&since={1}", group.LocalServerId,
                            Helpers.Common.GetFullString(group.LastRecommendationsUpdate)))));
                        serviceRequest.DownloadStringCompleted += new DownloadStringCompletedEventHandler(serviceRequest_DownloadStringCompleted);
                    }
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException("LoadGroups", ex.Message);
            }
        }

        void GetGroupRecommendationsCompleted(object sender, PhoneAppCentralService.GetGroupRecommendationsCompletedEventArgs e)
        {
            try
            {
                var recommendations = e.Result;

                if (recommendations.Count > 0)
                {
                    List<Database.Recommendation> recommendationsItems = new List<Database.Recommendation>();

                    foreach (var recommendation in recommendations)
                    {
                        recommendationsItems.Add(new Database.Recommendation()
                        {
                            AbsoluteUri = recommendation.AbsoluteUri,
                            Description = recommendation.Description,
                            Title = recommendation.Title,
                            GroupId = recommendation.GroupId
                        });
                    }

                    using (var context = new Database.DatabaseContext(Database.DatabaseContext.DBConnectionString))
                    {
                        var groups = context.Groups.Where(g => g.Id == _groupId);
                        if (groups.Any())
                            groups.First().LastRecommendationsUpdate = _recommendationsUpdateTime;

                        context.Recommendations.InsertAllOnSubmit(recommendationsItems);
                        context.SubmitChanges();
                    }

                    NotifySuccess();
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException("GetRecommendations", ex.Message);
            }
        }

        void serviceRequest_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                string res = e.Result;

                using (var context = new Database.DatabaseContext(Database.DatabaseContext.DBConnectionString))
                {
                    var group = context.Groups.FirstOrDefault(g => g.Id == _groupId);
                    if (group != null)
                    {
                        XDocument document = XDocument.Parse(res);

                        if (document.Root != null)
                        {
                            var childNode = document.Root.FirstNode;
                            while (childNode != null)
                            {
                                var childElement = childNode as XElement;
                                ReadRecommendation(childElement, group);
                                childNode = childNode.NextNode;
                            }
                        }

                        group.LastRecommendationsUpdate = _recommendationsUpdateTime;

                        context.SubmitChanges();
                    }
                }

                NotifySuccess();
            }
            catch (Exception ex)
            {
                Controllers.LogsController.WriteException("GetSharedFiles", ex.Message);
                ServersController.ServerAddressInvalid();
            }
        }

        void ReadRecommendation(XElement recommendationElement, Database.Group group)
        {
            Database.Recommendation recommendation = new Database.Recommendation();
            XNode node = recommendationElement.FirstNode;

            while (node != null)
            {
                var element = node as XElement;
                switch (element.Name.LocalName)
                {
                    case "DateCreated":
                        recommendation.DateCreated = DateTime.Parse(element.Value);
                        break;

                    case "AbsoluteUri":
                        recommendation.AbsoluteUri = element.Value;
                        break;

                    case "Description":
                        recommendation.Description = element.Value;
                        break;

                    case "Title":
                        recommendation.Title = element.Value;
                        break;
                }
                node = node.NextNode;
            }

            group.Recommendations.Add(recommendation);
        }
    }
}
