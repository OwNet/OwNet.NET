using System;
using System.Collections.Generic;
using ServiceEntities;
using ClientAndServerShared;
using ServiceEntities.Cache;

namespace WPFProxy.Cache
{
    public class CacheReporter
    {
        private static List<CacheLog> _notReportedItems = new List<CacheLog>();

        public static List<CacheLog> NotReportedItems
        {
            get { return _notReportedItems; }
        }

        public static void ReportToServer()
        {
            if (!Controller.UseServer) return;

            CacheLogsReport logsReport = new CacheLogsReport()
            {
                ClientName = Settings.ClientName,
                Logs = new List<CacheLog>()
            };

            lock (NotReportedItems)
            {
                logsReport.Logs.AddRange(_notReportedItems);
                NotReportedItems.Clear();
            }
            if (logsReport.Logs.Count == 0)
                return;

            try
            {
                ServerSettings settings = ServerRequestManager.Post<CacheLogsReport, ServerSettings>("cache/logs/report", logsReport);

                Settings.Update(settings);
            }
            catch (Exception e)
            {
                LogsController.WriteException("ReportToServer()", e.Message);
                return;
            }
        }
    }
}
