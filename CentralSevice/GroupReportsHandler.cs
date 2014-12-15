using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceEntities;
using ServiceEntities.CentralService.v2;

namespace CentralServerShared
{
    public class GroupReportsHandler
    {
        public static void ProcessReport(List<GroupReport> GroupLogs, List<GroupRecommendationReport> GroupRecomLogs, int serverId)
        {
            try
            {
                DataModelContainer container = new DataModelContainer();

                Server server = container.Servers.FirstOrDefault(s => s.Id == serverId);
                if (server == null)
                    return;

                if (GroupLogs != null)
                {
                    foreach (GroupReport gR in GroupLogs)
                    {
                        var groups = container.Groups.Where(g => g.Id == gR.Id);
                        if (groups.Any())
                        {
                            var group = groups.First();
                            group.DateModified = DateTime.Now;
                            group.Name = gR.Name;
                            group.Description = gR.Description;

                            group.Tags.ToList().ForEach(tag => group.Tags.Remove(tag));
                            
                            foreach (string tag in gR.Tags)
                                group.Tags.Add(new Tag()
                                {
                                    Value = tag,
                                    DateCreated = DateTime.Now
                                });
                        }
                        else
                        {
                            var g = new Group()
                            {
                                Id = gR.Id,
                                DateCreated = DateTime.Now,
                                DateModified = DateTime.Now,
                                Name = gR.Name,
                                Description = gR.Description
                            };

                            foreach (string tag in gR.Tags)
                                g.Tags.Add(new Tag()
                                {
                                    Value = tag,
                                    DateCreated = DateTime.Now
                                });

                            g.GroupServers.Add(new GroupServer()
                            {
                                Server = server,
                                DateCreated = DateTime.Now
                            });

                            container.Groups.AddObject(g);
                        }
                    }
                    container.SaveChanges();
                }

                if (GroupRecomLogs != null)
                {
                    foreach (var recom in GroupRecomLogs)
                    {
                        var group = container.Groups.Where(g => g.Id == recom.GroupId);
                        if (group.Any())
                        {
                            Group gr = group.First();

                            gr.Recommendations.Add(new Recommendation()
                            {
                                AbsoluteUri = recom.AbsoluteUri,
                                Description = recom.Description,
                                DateCreated = DateTime.Now,
                                Title = recom.Title,
                                Server = server
                            });
                        }
                    }
                    container.SaveChanges();
                }
            }

            catch (Exception ex)
            {
                Controller.WriteException("Get links to update", ex.Message);
            }
        }
    }
}