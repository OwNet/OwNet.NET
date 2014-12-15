using System;
using System.Collections.Generic;
using ClientAndServerShared;
using WPFServer.LocalServerCentralService;
using System.Linq;

namespace WPFServer.CentralService
{
    public class CentralServiceCommunicator
    {
        public static void Register()
        {
            string randomString = Helpers.Common.RandomString(8, false);
            Properties.Settings.Default.CentralServerUsername = randomString.Substring(0, 8);
            randomString = Helpers.Common.RandomString(20, false);
            Properties.Settings.Default.CentralServerPassword = randomString.Substring(8);
            Properties.Settings.Default.Save();
        }

        public static void ReportToCentralServer()
        {
            var now = DateTime.Now;
            ActivityLogs reporter = new ActivityLogs();
            List<LocalServerCentralService.ActivityLogReport> activities;
            try
            {
                // generovanie a poslanie reportov 
                activities = reporter.GenerateLogsReport();
                if (ReportToCentralServer(activities,
                    GroupReporter.GenerateGroupReport(),
                    GroupReporter.GenerateRecommendationReport(),
                    UserReporter.GenerateUserReport(),
                    UserReporter.GenerateUserGroupsReport()))
                {
                    // update last report date in app settings
                    Properties.Settings.Default.LastCentralServiceReport = now;
                    Properties.Settings.Default.Save();

                    reporter.LogsReported(activities);
                    WPFServer.Proxy.AccessLogs.AccessLogsReporter.DeleteAccessLogs();
                }
            }
            catch (Exception ex)
            {
                Server.WriteException("Report to central server", ex.Message);
            }
        }

        private static bool ReportToCentralServer(List<ActivityLogReport> logs,
            List<GroupReport> groups,
            List<GroupRecommendationReport> groupRecomms,
            List<UserReport> userReports,
            List<UserGroupReport> userGroupReports)
        {
            try
            {
                List<AccessLogReport> reports = WPFServer.Proxy.AccessLogs.AccessLogsReporter.GenerateAccessLogsReport();
                int skip = 0, take = 10000;

                LocalServerServiceClient client = new LocalServerServiceClient();

                bool found = false;
                do
                {
                    found = false;

                    ReportLog report = new ReportLog();
                    if (skip < reports.Count)
                    {
                        report.AccessLogs = reports.Skip(skip).Take(take).ToArray();
                        found = true;
                    }
                    if (skip < logs.Count)
                    {
                        report.ActivityLogs = logs.Skip(skip).Take(take).ToArray();
                        found = true;
                    }
                    if (skip < groups.Count)
                    {
                        report.GroupLogs = groups.Skip(skip).Take(take).ToArray();
                        found = true;
                    }
                    if (skip < groupRecomms.Count)
                    {
                        report.GroupRecommendationLogs = groupRecomms.Skip(skip).Take(take).ToArray();
                        found = true;
                    }
                    if (skip < userReports.Count)
                    {
                        report.UserLogs = userReports.Skip(skip).Take(take).ToArray();
                        found = true;
                    }
                    if (skip < userGroupReports.Count)
                    {
                        report.UserGroupReports = userGroupReports.Skip(skip).Take(take).ToArray();
                        found = true;
                    }

                    client.Report(report, CentralServiceCommunicator.GetServerInfo());

                    skip += take;
                } while (found);

                client.Close();
            }
            catch (Exception ex)
            {
                LogsController.WriteException("Report to central server", ex.Message);
                return false;
            }
            return true;
        }

        public static LocalServerCentralService.ServerInfo GetServerInfo()
        {
            LocalServerCentralService.ServerInfo info = new LocalServerCentralService.ServerInfo()
            {
                Username = Server.Settings.CentralServerUsername(),
                Password = Server.Settings.CentralServerPassword(),
                ServerName = Server.Settings.ServerName()
            };

            return info;
        }

        public static bool ReportNow(CancelObject cancelObject)
        {
            Jobs.ReportToCentralServer();
            return true;
        }

        public static bool GetUpdatesNow(CancelObject cancelObject)
        {
            Jobs.GetLinksToUpdate();
            return true;
        }
    }
}
