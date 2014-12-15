using System;
using System.Collections.Generic;
using System.Linq;
using CentralServerShared;
using ServiceEntities;

namespace CentralService
{
    public class ServerController
    {
        private int _id;
        private object _serverLock = new object();
        List<ServiceEntities.CentralService.v2.AccessLogReport> _lastAccessLogs = null;
        
        public ServerController(int id)
        {
            _id = id;
        }

        public List<ServiceEntities.CentralService.v2.AccessLogReport> LastReports
        {
            set { _lastAccessLogs = value; }
        }

        public void ReceivedAccessLogs()
        {
            List<ServiceEntities.CentralService.v2.AccessLogReport> accessLogs = _lastAccessLogs;
            _lastAccessLogs = null;
            if (accessLogs == null)
                return;

            try
            {
                lock (_serverLock)
                {
                    DataModelContainer container = new DataModelContainer();
                    Server server = container.Servers.FirstOrDefault(s => s.Id == _id);
                    if (server == null)
                        return;

                    foreach (ServiceEntities.CentralService.v2.AccessLogReport report in accessLogs)
                    {
                        if (HttpGetRetriever.MatchesBlacklist(report.AbsoluteUri))
                            continue;

                        int uriHash = Helpers.Proxy.ProxyCache.GetUriHash(report.AbsoluteUri);
                        Cache cache = server.Caches.FirstOrDefault(c => c.UriHash == uriHash);
                        if (cache == null)
                        {
                            cache = new Cache()
                            {
                                AbsoluteUri = report.AbsoluteUri,
                                DateCreated = DateTime.Now,
                                DateUpdated = DateTime.Now,
                                UriHash = uriHash,
                                Type = report.Type
                            };
                            server.Caches.Add(cache);
                            CacheUpdater.UpdateCacheHash(cache);
                        }
                        else
                        {
                            if ((report.DownloadedFrom & (int)Helpers.Proxy.ProxyEntry.DownloadMethodElements.RefreshedCache) > 0)
                            {
                                CacheUpdater.UpdateCacheHash(cache);
                            }
                        }

                        if ((report.DownloadedFrom & (int)Helpers.Proxy.ProxyEntry.DownloadMethodElements.AccessedByUser) > 0)
                        {
                            cache.AccessLogs.Add(new AccessLog()
                            {
                                FetchDuration = report.FetchDuration,
                                AccessedAt = report.AccessedAt,
                                DownloadedFrom = report.DownloadedFrom,
                                CacheCreatedAt = cache.DateUpdated
                            });

                            if (cache.UpdateAt == null && cache.AccessLogs.Count > CacheUpdater.MinCacheAccessesBeforeUpdate)
                            {
                                cache.UpdateAt = CacheUpdater.CalculateNextUpdateDate(cache);
                            }
                        }
                    }

                    container.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Controller.WriteException("ReceivedAccessLogs", ex.Message);
            }
        }

        public List<ServiceEntities.CentralService.v2.LinkToUpdate> GetRecommendedUpdates()
        {
            DataModelContainer container = new DataModelContainer();
            Server server = container.Servers.FirstOrDefault(s => s.Id == _id);
            if (server == null)
                return null;

            List<ServiceEntities.CentralService.v2.LinkToUpdate> links = new List<ServiceEntities.CentralService.v2.LinkToUpdate>();
            try
            {
                var recommendations = from r in server.RecommendedUpdates
                                      where r.Sent == false
                                      orderby r.Priority descending
                                      select r;
                if (recommendations.Any())
                {
                    foreach (RecommendedUpdate update in recommendations)
                    {
                        links.Add(new ServiceEntities.CentralService.v2.LinkToUpdate()
                        {
                            AbsoluteUri = update.Cache.AbsoluteUri,
                            Priority = update.Priority
                        });
                        update.Sent = true;
                    }
                    container.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Controller.WriteException("Get links to update", ex.Message);
            }

            return links;
        }

        public void ReceivedLogsReport(List<ServiceEntities.CentralService.v2.ActivityLogReport> activityLogs)
        {
            if (activityLogs == null || !activityLogs.Any())
                return;

            DataModelContainer container = new DataModelContainer();
            Server server = container.Servers.FirstOrDefault(s => s.Id == _id);
            if (server == null)
                return;

            foreach (ServiceEntities.CentralService.v2.ActivityLogReport activity in activityLogs)
            {
                ActivityLog act = new ActivityLog()
                {
                    AbsoluteUri = activity.AbsoluteURI,
                    DateTime = activity.DateTime,
                    Action = (int)activity.Action,
                    Message = activity.Message,
                    Title = activity.Title,
                    Type = (int)activity.Type,
                    UserFirstname = activity.UserFirstname,
                    UserSurname = activity.UserSurname
                };
                server.ActivityLogs.Add(act);
            }
            container.SaveChanges();
        }

        internal void ReceivedGroupsReport(List<ServiceEntities.CentralService.v2.GroupReport> groupLogs,
            List<ServiceEntities.CentralService.v2.GroupRecommendationReport> recommendationLogs)
        {
            GroupReportsHandler.ProcessReport(groupLogs, recommendationLogs, _id);
        }

        internal void ReceivedUsersReport(List<ServiceEntities.CentralService.v3.UserReport> userReports)
        {
            UserReportsHandler.ProcessReport(userReports, _id);
        }

        internal void ReceivedUserGroupsReport(List<ServiceEntities.CentralService.v3.UserGroupReport> userGroups)
        {
            UserReportsHandler.ProcessUserGroupsReport(userGroups, _id);
        }

        internal List<ServiceEntities.CentralService.v2.GroupRecommendationReport> GetNewRecommendations(DateTime lastUpdate)
        {
            return GroupUpdatesHandler.GetNewRecommendations(lastUpdate, _id);
        }
    }
}