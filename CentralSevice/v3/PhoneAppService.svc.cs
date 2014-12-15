using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Net;
using System.IO;
using ServiceEntities.CentralService.v3;

namespace CentralServerShared.v3
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "PhoneAppService" in code, svc and config file together.
    public class PhoneAppService : IPhoneAppService
    {
        public List<ServiceEntities.CentralService.v2.GroupReport> GetGroups(DateTime since, AuthenticatedUser authenticatedUser)
        {
            var groups = new List<ServiceEntities.CentralService.v2.GroupReport>();
            try
            {
                using (DataModelContainer container = new DataModelContainer())
                {
                    var user = UserAuthentication.GetUserByAuthentication(authenticatedUser, container);
                    if (user != null)
                    {
                        var dbGroups = from ug in user.UserGroups
                                       let gr = ug.Group
                                       where ug.DateCreated > since || gr.DateModified > since
                                       orderby gr.DateModified descending
                                       select gr;

                        foreach (var group in dbGroups)
                        {
                            var groupReport = new ServiceEntities.CentralService.v2.GroupReport()
                            {
                                Name = group.Name,
                                Description = group.Description,
                                Id = group.Id
                            };

                            groupReport.Tags = new List<string>();

                            foreach (var tag in group.Tags)
                                groupReport.Tags.Add(tag.Value);

                            groups.Add(groupReport);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Controller.WriteException("GetGroups", ex.Message);
            }

            return groups;
        }

        public List<ServiceEntities.CentralService.v2.GroupRecommendationReport> GetGroupRecommendations(int groupId, DateTime since, AuthenticatedUser authenticatedUser)
        {
            var recommendations = new List<ServiceEntities.CentralService.v2.GroupRecommendationReport>();
            try
            {
                using (DataModelContainer container = new DataModelContainer())
                {
                    if (UserAuthentication.GetUserByAuthentication(authenticatedUser, container) != null)
                    {
                        var groups = container.Groups.Where(g => g.Id == groupId);
                        if (groups.Any())
                        {
                            var group = groups.First();
                            var groupRecommendations = group.Recommendations.Where(r => r.DateCreated > since);

                            foreach (var recommedation in groupRecommendations)
                            {
                                recommendations.Add(new ServiceEntities.CentralService.v2.GroupRecommendationReport()
                                {
                                    AbsoluteUri = recommedation.AbsoluteUri,
                                    Description = recommedation.Description,
                                    Title = recommedation.Title,
                                    GroupId = groupId
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Controller.WriteException("GetGroupRecommendations", ex.Message);
            }

            return recommendations;
        }

        public WebsiteContent GetWebsiteContent(string url)
        {
            WebClient client = new WebClient();
            var websiteContent = new WebsiteContent()
            {
                Content = "",
                Success = false
            };
            try
            {
                string requestUri = string.Format("http://boilerpipe-web.appspot.com/extract?url={0}&extractor=ArticleExtractor&output=htmlFragment",
                    System.Web.HttpUtility.UrlEncode(url));

                WebRequest request = WebRequest.Create(requestUri);
                WebResponse response = request.GetResponse();
                Stream data = response.GetResponseStream();
                using (StreamReader sr = new StreamReader(data))
                {
                    websiteContent.Content = sr.ReadToEnd();
                }
                websiteContent.Success = true;
            }
            catch (Exception ex)
            {
                Controller.WriteException("GetWebsiteContent", ex.Message);
            }
            return websiteContent;
        }

        public List<ServiceEntities.CentralService.v2.ServerInfo> GetAllServerInfos()
        {
            return CentralService.Controller.GetAllServerInfos();
        }

        public AuthenticateUserResult Authenticate(AuthenticateUserRequest request)
        {
            return UserAuthentication.Authenticate(request);
        }
    }
}
