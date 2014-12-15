using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.Http;
using ClientAndServerShared;
using ServiceEntities.Cache;
using WPFProxy.Proxy;
using WPFProxy.Database;

namespace WPFProxy.Cache
{
    class CacheMaintainer
    {
        public static long MaximumCacheSize { get { return (long)Properties.Settings.Default.MaximumCacheSize * Helpers.Common.BytesInMB; } }

        public static void DeleteOutdatedCache()
        {
            if (!Controller.UseDataService) return;

            DateTime lastSynced = Properties.Settings.Default.LastSyncedWithServerCache;
            DateTime syncedAt = DateTime.Now;
            List<CacheLog> itemsToReport = new List<CacheLog>();

            try
            {
                HttpQueryString vars = new HttpQueryString();
                vars.Add("since", HttpUtility.UrlEncode(lastSynced.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK")));

                NewCacheList results = ServerRequestManager.Get<NewCacheList>(new Uri("cache/new_items/", UriKind.Relative), vars);

                if (results == null) return;

                Dictionary<int, DateTime> items = results.Items;

                using (DatabaseEntities db = Controller.GetDatabase())
                {
                    var caches = from p in db.Caches
                                 where items.Keys.Contains(p.ID)
                                 select p;

                    bool changes = false;
                    foreach (Caches cache in caches)
                    {
                        if (cache.DateModified < items[cache.ID] && items[cache.ID] - cache.DateModified > TimeSpan.FromSeconds(10))
                        {
                            try
                            {
                                for (int i = 0; i < cache.Parts; ++i)
                                {
                                    FileInfo info = new FileInfo(Controller.GetCacheFilePath(cache.AbsoluteUri, i));
                                    if (info.Exists)
                                        info.Delete();
                                }

                                db.Caches.DeleteObject(cache);
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
                        Controller.SubmitDatabaseChanges(db);

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
                    lock (CacheReporter.NotReportedItems)
                        CacheReporter.NotReportedItems.AddRange(itemsToReport);
            }
        }

        public static void CleanCache()
        {
            long maximumCacheSize = MaximumCacheSize;
            long cacheFolderSize = Helpers.Common.GetDirectorySize(Controller.CacheFolder);

            cacheFolderSize += Math.Min(maximumCacheSize / 10, Helpers.Cache.MaximumCacheReserve);
            if (cacheFolderSize < Helpers.Cache.MinimumCacheSize)
                return;

            List<CacheLog> itemsToReport = new List<CacheLog>();
            DateTime tenMinutesAgo = DateTime.Today.Subtract(TimeSpan.FromMinutes(10));
            try
            {
                if (cacheFolderSize > maximumCacheSize)
                {
                    DatabaseEntities db = Controller.GetDatabase();

                    var caches = from p in db.Caches
                                 where p.DateModified < tenMinutesAgo
                                 orderby p.AccessValue
                                 select p;

                    foreach (Caches cache in caches)
                    {
                        try
                        {
                            for (int i = 0; i < cache.Parts; ++i)
                            {
                                FileInfo info = new FileInfo(Controller.GetCacheFilePath(cache.AbsoluteUri, i));
                                if (info.Exists)
                                {
                                    long size = info.Length;
                                    info.Delete();
                                    cacheFolderSize -= size;
                                }
                            }
                            db.Caches.DeleteObject(cache);

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
                            LogsController.WriteException("Clean cache", ex.Message);
                        }
                        if (cacheFolderSize <= maximumCacheSize)
                            break;
                    }

                    Controller.SubmitDatabaseChanges(db);
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException("Clean cache", ex.Message);
            }
            finally
            {
                if (itemsToReport.Count > 0)
                    lock (CacheReporter.NotReportedItems)
                        CacheReporter.NotReportedItems.AddRange(itemsToReport);
            }
        }

        public static bool DeleteCache(CancelObject cancelObject)
        {
            try
            {
                string[] fileNames = Directory.GetFiles(Controller.CacheFolder, "*.*");

                foreach (string name in fileNames)
                {
                    if (cancelObject.IsCanceled)
                        return false;

                    FileInfo info = new FileInfo(name);
                    info.Delete();
                }

                DatabaseEntities database = Controller.GetDatabase();
                database.ExecuteStoreCommand("DELETE FROM Caches");
                database.Dispose();
            }
            catch (Exception ex)
            {
                LogsController.WriteException("Delete cache", ex.Message);
                return false;
            }
            LogsController.WriteMessage("Cache deleted.");
            return true;
        }
    }
}
