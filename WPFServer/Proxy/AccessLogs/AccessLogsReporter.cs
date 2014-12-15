using System;
using System.Collections.Generic;
using System.Linq;
using WPFServer.LocalServerCentralService;
using WPFServer.DatabaseContext;
using ClientAndServerShared;

namespace WPFServer.Proxy.AccessLogs
{
    public class AccessLogsReporter
    {
        public static void ReceivedReportFromClient(ServiceEntities.Cache.CacheLogsReport logsReport)
        {
            if (logsReport.Logs.Count == 0)
                return;

            try
            {
                using (MyDBContext context = new MyDBContext())
                {
                    var clients = context.Fetch<Client>(c => c.ClientName == logsReport.ClientName);
                    Client client = null;
                    if (clients.Any())
                    {
                        client = clients.First();
                    }
                    else
                    {
                        client = context.Create<Client>(new Client()
                        {
                            ClientName = logsReport.ClientName,
                            LastIP = Clients.ClientsController.GetClientIP(logsReport.ClientName)
                        });
                    }

                    foreach (ServiceEntities.Cache.CacheLog log in logsReport.Logs)
                    {
                        int hash = Helpers.Proxy.ProxyCache.GetUriHash(log.AbsoluteUri);
                        try
                        {
                            context.Create<AccessLog>(new AccessLog()
                            {
                                CacheId = hash,
                                AbsoluteUri = log.AbsoluteUri,
                                AccessedAt = log.AccessedAt,
                                FetchDuration = log.FetchDuration,
                                DownloadedFrom = log.DownloadedFrom,
                                Type = log.Type
                            });

                            if (log.Type == (int)Helpers.Proxy.ProxyEntry.HttpTypes.DELETEFROMCACHE)
                            {
                                var linkItems = context.Fetch<ClientCacheLink>(item =>
                                    item.ClientId == client.Id && item.CacheLinkId == hash);
                                if (linkItems.Any())
                                    foreach (var item in linkItems)
                                        context.Remove<ClientCacheLink>(item);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogsController.WriteException("Save report", ex.Message);
                        }
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException("Received report", ex.Message);
            }
        }

        internal static List<AccessLogReport> GenerateAccessLogsReport()
        {
            List<AccessLogReport> reports = new List<AccessLogReport>();

            MyDBContext context = new MyDBContext();
            var accessLogs = context.FetchSet<AccessLog>();
            
            foreach (AccessLog log in accessLogs)
            {
                reports.Add(new AccessLogReport()
                {
                    AbsoluteUri = log.AbsoluteUri,
                    AccessedAt = log.AccessedAt,
                    DownloadedFrom = log.DownloadedFrom,
                    FetchDuration = log.FetchDuration,
                    Type = log.Type
                });
            }
            return reports;
        }

        public static void DeleteAccessLogs()
        {
            try
            {
                MyDBContext context = new MyDBContext();
                context.Database.ExecuteSqlCommand(@"DELETE FROM AccessLog");
                context.Dispose();
            }
            catch
            {
                throw;
            }
        }

        internal static Dictionary<int, DateTime> NewCacheItemsSince(DateTime dtSince)
        {
            Dictionary<int, DateTime> items = new Dictionary<int, DateTime>();

            var cacheLinks = from c in (new MyDBContext()).FetchSet<CacheLink>()
                         where c.LastModified > dtSince
                         select c;

            foreach (CacheLink item in cacheLinks)
                items.Add(item.Id, item.LastModified);

            return items;
        }
    }
}
