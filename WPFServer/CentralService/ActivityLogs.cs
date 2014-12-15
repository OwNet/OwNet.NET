using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WPFServer.LocalServerCentralService;
using WPFServer.DatabaseContext;

namespace WPFServer.CentralService
{
    public class ActivityLogs
    {
        private List<int> logs = null;

        public List<ActivityLogReport> GenerateLogsReport()
        {
            List<ActivityLogReport> ret = new List<ActivityLogReport>();
            ActivityLogReport item;

            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    IQueryable<Activity> acts = con.Fetch<Activity>(a => a.Reported == false);
                    if (acts.Any())
                    {
                        logs = new List<int>(acts.Count());
                        List<Activity> list = acts.ToList();
                        foreach (Activity newsItem in list)
                        {
                            item = new ActivityLogReport();
                            item.DateTime = newsItem.TimeStamp;
                            item.Message = newsItem.Message;
                            item.UserFirstname = newsItem.User.Firstname;
                            item.UserSurname = newsItem.User.Surname;
                            if (newsItem.PageId != null)
                            {
                                item.AbsoluteURI = newsItem.Page.AbsoluteURI;
                                item.Title = newsItem.Page.Title;
                            }
                            else if (newsItem.FileId != null)
                            {
                                item.AbsoluteURI = newsItem.File.FileName;
                                item.Title = newsItem.File.Title;
                            }
                            else
                            {
                                item.AbsoluteURI = "";
                                item.Title = "";
                            }
                            item.Action = (LocalServerCentralService.ActivityAction)((int)newsItem.Action);
                            item.Type = (LocalServerCentralService.ActivityType)((int)newsItem.Type);

                            logs.Add(newsItem.Id);
                            ret.Add(item);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Server.WriteException("Generate log report: ", e.Message);
            }

            return ret;
        }

        public void LogsReported(List<ActivityLogReport> activities)
        {
            if (logs == null || logs.Count <= 0) return;

            int[] actions =
            {
                (int)WPFServer.DatabaseContext.ActivityAction.Create,
                (int)WPFServer.DatabaseContext.ActivityAction.Update,
                (int)WPFServer.DatabaseContext.ActivityAction.Delete
            };
            int[] types =
            {
                (int)WPFServer.DatabaseContext.ActivityType.Share,
                (int)WPFServer.DatabaseContext.ActivityType.Tag,
                (int)WPFServer.DatabaseContext.ActivityType.Register,
                (int)WPFServer.DatabaseContext.ActivityType.Recommend,
                (int)WPFServer.DatabaseContext.ActivityType.Rating
            };

            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    System.Data.Entity.DbSet<Activity> set = con.FetchSet<Activity>();
                    IQueryable<Activity> acts = set.Where(a => logs.Contains(a.Id) || (a.Reported == true && a.Visible == false));

                    foreach (Activity act in acts)
                    {
                        if (act.Visible == true)
                        {
                            act.Reported = true;
                        }
                        else
                        {
                            set.Remove(act);
                        }
                    }
                    con.SaveChanges();

                    acts = set.Where(a => types.Contains(a.TypeValue) && actions.Contains(a.ActionValue) && a.Visible == true).OrderByDescending(a => a.TimeStamp).Skip(45);

                    foreach (Activity act in acts)
                    {
                        act.Visible = false;
                    }
                    con.SaveChanges();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
