using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceEntities;
using ServiceEntities.CentralService.v2;

namespace CentralServerShared
{
    public class GroupUpdatesHandler
    {
        public static List<GroupReport> GetModifiedGroups(DateTime lastUpdate)
        {
            var modifiedGroups = new List<GroupReport>();
            
            try
            {
                using (DataModelContainer container = new DataModelContainer())
                {
                    var groups = container.Groups.Where(g => g.DateModified > lastUpdate).OrderBy(g => g.DateModified);

                    foreach (var group in groups)
                    {
                        var gR = new GroupReport()
                        {
                            Name = group.Name,
                            Description = group.Description,
                            Id = group.Id,
                        };

                        gR.Tags = new List<string>();

                        foreach (var tag in group.Tags)
                        {
                            gR.Tags.Add(tag.Value);
                        }

                        modifiedGroups.Add(gR);
                    }
                }
            }
            catch (Exception e)
            {
                Controller.WriteException("GetModifiedGroups", e.Message);
            }

            return modifiedGroups;
        }

        internal static List<GroupRecommendationReport> GetNewRecommendations(DateTime lastUpdate, int serverId)
        {
            var newRecommendations = new List<GroupRecommendationReport>();
            try
            {
                DataModelContainer container = new DataModelContainer();

                var recomms = container.Recommendations.Where(d => d.DateCreated > lastUpdate &&
                    d.Server.Id != serverId)
                    .OrderBy(g => g.DateCreated);

                foreach (var recomm in recomms)
                {
                    var recommendation = new GroupRecommendationReport()
                    {
                        AbsoluteUri = recomm.AbsoluteUri,
                        Description = recomm.Description,
                        GroupId = recomm.Group.Id,
                        Title = recomm.Title
                    };

                    newRecommendations.Add(recommendation);
                }
            }
            catch (Exception e)
            {
                Controller.WriteException("GetModifiedGroups", e.Message);
            }

            return newRecommendations;
        }
    }
}