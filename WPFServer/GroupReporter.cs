using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientAndServerShared;
using WPFServer.DatabaseContext;

namespace WPFServer
{
    class GroupReporter
    {
        public static List<LocalServerCentralService.GroupReport> GenerateGroupReport()
        {
            List<LocalServerCentralService.GroupReport> ret = new List<LocalServerCentralService.GroupReport>();

            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    var groups = con.Fetch<Group>(g => g.DateCreated > Properties.Settings.Default.LastCentralServiceReport && g.Location);

                    foreach (Group g in groups)
                    {
                        var gR = new LocalServerCentralService.GroupReport()
                        {

                            Description = g.Description,
                            Name = g.Name,
                            Id = g.Id
                        };

                        var listS = new List<string>();

                        foreach (var t in g.GroupTags)
                        {
                            listS.Add(t.Value);
                        }

                        gR.Tags = listS.ToArray();
                        ret.Add(gR);
                    }
                }
            }
            catch(Exception e)
            {
                LogsController.WriteException( "GroupReporter",e.Message);
            }

            return ret;

        }

        public static List<LocalServerCentralService.GroupRecommendationReport> GenerateRecommendationReport()
        {
            List<LocalServerCentralService.GroupRecommendationReport> ret = new List<LocalServerCentralService.GroupRecommendationReport>();

            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    var recomms = con.Fetch<GroupRecommendation>(g =>
                        g.DateTime > Properties.Settings.Default.LastCentralServiceReport
                        && g.Group.Location
                        && g.User.Username != "ownetglobal");

                    foreach (var r in recomms)
                    {
                        ret.Add(new LocalServerCentralService.GroupRecommendationReport()
                        {
                            AbsoluteUri = r.Page.AbsoluteURI,
                            Description = r.Description,
                            GroupId = r.GroupId,
                            Title = r.Title
                        });
                    }
                }
            }
            catch (Exception e)
            {
                LogsController.WriteException("GroupReporter", e.Message);

            }

            return ret;
        }
    }
}
