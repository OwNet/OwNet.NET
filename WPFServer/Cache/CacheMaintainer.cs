using System;
using System.IO;
using System.Linq;
using SharedProxy;
using System.Collections.Generic;
using ServiceEntities.Cache;
using ClientAndServerShared;
using Helpers.Proxy;
using WPFServer.DatabaseContext;

namespace WPFServer.Cache
{
    public static class CacheMaintainer
    {
        private static object _updatingCacheLock = new object();

        private static void Report(List<CacheLog> itemsToReport)
        {
            WPFServer.Proxy.AccessLogs.AccessLogsReporter.ReceivedReportFromClient(new CacheLogsReport()
            {
                ClientName = Controller.AppSettings.ClientName(),
                Logs = itemsToReport
            });
        }

        public static bool DeleteCache(CancelObject cancelObject)
        {
            try
            {
                MyDBContext context = new MyDBContext();
                context.Database.ExecuteSqlCommand("DELETE FROM [CacheHeader]");
                context.Database.ExecuteSqlCommand("DELETE FROM [CacheUpdate]");
                context.Database.ExecuteSqlCommand("DELETE FROM [Cache]");
                context.Dispose();

                string[] files = System.IO.Directory.GetFiles(SharedProxy.Controller.CacheFolderPath);
                foreach (string file in files)
                    File.Delete(file);

            }
            catch (Exception e)
            {
                Controller.WriteException(e.Message);
                return false;
            }
            return true;
        }

        public static void DeleteOutdatedCache()
        {
            DateTime syncedAt = DateTime.Now;
            List<CacheLog> itemsToReport = new List<CacheLog>();

            try
            {
                Dictionary<int, DateTime> items = Proxy.AccessLogs.AccessLogsReporter.NewCacheItemsSince(Properties.Settings.Default.LastSyncedWithServerCache);

                using (MyDBContext context = new MyDBContext())
                {
                    var caches = context.Fetch<DatabaseContext.Cache.Cache>(c => items.Keys.Contains(c.Id));

                    bool changes = false;
                    foreach (var cache in caches)
                    {
                        if (cache.DateModified < items[cache.Id] && items[cache.Id] - cache.DateModified > TimeSpan.FromSeconds(10))
                        {
                            try
                            {
                                for (int i = 0; i < cache.Parts; ++i)
                                {
                                    FileInfo info = new FileInfo(SharedProxy.Controller.GetCacheFilePath(cache.Id, i));
                                    if (info.Exists)
                                        info.Delete();
                                }

                                context.Remove<DatabaseContext.Cache.Cache>(cache);
                                changes = true;

                                itemsToReport.Add(new ServiceEntities.Cache.CacheLog()
                                {
                                    AbsoluteUri = cache.AbsoluteUri,
                                    FetchDuration = 0.0,
                                    AccessedAt = DateTime.Now,
                                    DownloadedFrom = (int)ProxyEntry.DownloadMethodElements.RemovedFromCache,
                                    Type = (int)Helpers.Proxy.ProxyEntry.HttpTypes.DELETEFROMCACHE
                                });
                            }
                            catch (Exception e)
                            {
                                LogsController.WriteException("DeleteOutdatedCache", e.Message);
                            }
                        }
                    }

                    if (changes)
                        context.SaveChanges();

                    Properties.Settings.Default.LastSyncedWithServerCache = syncedAt;
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception e)
            {
                LogsController.WriteException("DeleteOutdatedCache", e.Message);
            }
            finally
            {
                if (itemsToReport.Count > 0)
                    Report(itemsToReport);
            }
        }

        public static void CleanCache()
        {
            long maximumCacheSize = Controller.AppSettings.MaximumCacheSize();
            long cacheFolderSize = Helpers.Common.GetDirectorySize(SharedProxy.Controller.CacheFolderPath);

            cacheFolderSize += Math.Min(maximumCacheSize / 10, Helpers.Cache.MaximumCacheReserve);
            if (cacheFolderSize < Helpers.Cache.MinimumCacheSize)
                return;

            if (cacheFolderSize > maximumCacheSize)
            {
                List<CacheLog> itemsToReport = new List<CacheLog>();

                try
                {
                    MyDBContext context = new MyDBContext();
                    DateTime oneHourAgo = DateTime.Today.Subtract(TimeSpan.FromHours(1));

                    var caches = context
                        .Fetch<DatabaseContext.Cache.Cache>(c => c.AccessCount != 0 || c.DateModified < oneHourAgo)
                        .OrderBy(c => c.AccessValue);

                    foreach (DatabaseContext.Cache.Cache cache in caches)
                    {
                        try
                        {
                            for (int i = 0; i < cache.Parts; ++i)
                            {
                                FileInfo info = new FileInfo(SharedProxy.Controller.GetCacheFilePath(cache.Id, i));
                                if (info.Exists)
                                {
                                    long size = info.Length;
                                    info.Delete();
                                    cacheFolderSize -= size;
                                }
                            }

                            Controller.WriteException("Cache deleted", cache.AbsoluteUri);

                            context.Remove<DatabaseContext.Cache.Cache>(cache);

                            itemsToReport.Add(new ServiceEntities.Cache.CacheLog()
                            {
                                AbsoluteUri = cache.AbsoluteUri,
                                FetchDuration = 0.0,
                                AccessedAt = DateTime.Now,
                                DownloadedFrom = (int)ProxyEntry.DownloadMethodElements.RemovedFromCache,
                                Type = (int)Helpers.Proxy.ProxyEntry.HttpTypes.DELETEFROMCACHE
                            });
                        }
                        catch (Exception ex)
                        {
                            Controller.WriteException("Clean cache", ex.Message);
                        }
                        if (cacheFolderSize <= maximumCacheSize)
                            break;
                    }
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    LogsController.WriteException("Clean cache", ex.Message);
                }
                finally
                {
                    if (itemsToReport.Count > 0)
                        Report(itemsToReport);
                }
            }
        }

        public static void UpdateCache()
        {
            lock (_updatingCacheLock)
            {
                if (SharedProxy.Proxy.ProxyTraffic.LastTraffic() < 2)
                {
                    List<CacheLog> itemsToReport = new List<CacheLog>();
                    try
                    {
                        using (MyDBContext context = new MyDBContext())
                        {
                            var updates = context.FetchSet<DatabaseContext.Cache.CacheUpdate>()
                                .OrderByDescending(c => c.Priority);

                            bool changes = false;
                            foreach (var update in updates)
                            {
                                DateTime startTime = DateTime.Now;
                                SharedProxy.Controller.UpdateCache(update.Cache.AbsoluteUri, update.DateRecommended);
                                TimeSpan ts = DateTime.Now - startTime;

                                Controller.WriteException("Cache updated", update.Cache.AbsoluteUri);

                                itemsToReport.Add(new CacheLog()
                                {
                                    AbsoluteUri = update.Cache.AbsoluteUri,
                                    DownloadedFrom = (int)ProxyEntry.DownloadMethods.RefreshCacheOnServer,
                                    FetchDuration = ts.TotalSeconds,
                                    AccessedAt = DateTime.Now
                                });

                                context.Remove<DatabaseContext.Cache.CacheUpdate>(update);
                                changes = true;

                                if (SharedProxy.Proxy.ProxyTraffic.LastTraffic() > 1)
                                    break;

                            }

                            if (changes)
                            {
                                context.SaveChanges();
                                Report(itemsToReport);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogsController.WriteException("UpdateCache", ex.Message);
                    }
                }
            }
        }

        public static void ProcessLinksToUpdate(List<ServiceEntities.Cache.CacheLinkToUpdate> linksToUpdate)
        {
            try
            {
                if (linksToUpdate.Any())
                {
                    using (MyDBContext context = new MyDBContext())
                    {

                        DateTime dateRecommended = DateTime.Now;
                        foreach (var link in linksToUpdate)
                        {
                            int uriHash = ProxyCache.GetUriHash(link.AbsoluteUri);
                            var caches = context.Fetch<DatabaseContext.Cache.Cache>(c => c.Id == uriHash);

                            if (caches.Any())
                            {
                                DatabaseContext.Cache.Cache cache = caches.First();
                                cache.CacheUpdates.Add(new DatabaseContext.Cache.CacheUpdate()
                                {
                                    Priority = link.Priority,
                                    DateRecommended = dateRecommended
                                });

                                Controller.WriteException("Update recommended", link.AbsoluteUri);
                            }
                        }
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Controller.WriteException("Update cache - update", ex.Message);
            }
        }
    }
}
