using System;
using System.Linq;
using ClientAndServerShared;

namespace WPFCentralServer
{
    public class CacheUpdater : CentralServerShared.CacheUpdater
    {
        private static object _downloadPlannedUpdatesLock = new object();

        public static void DownloadPlannedUpdates()
        {
            LogsController.WriteMessage("Started downloading planned updates.");
            lock (_downloadPlannedUpdatesLock)
            {
                try
                {
                    CentralServerShared.DataModelContainer container = new CentralServerShared.DataModelContainer();
                    var caches = from c in container.Caches
                                 where c.UpdateAt != null
                                 select c;

                    foreach (CentralServerShared.Cache cache in caches)
                    {
                        CheckCacheForUpdates(cache);
                        cache.UpdateAt = null;
                    }
                    container.SaveChanges();
                }
                catch (Exception ex)
                {
                    Controller.WriteException("Download planned updates", ex.Message);
                }
            }
            LogsController.WriteMessage("Finished downloading planned updates.");
        }

        public static void CheckCacheForUpdates(CentralServerShared.Cache cache)
        {
            LogsController.LogPageAccess(cache.AbsoluteUri);
            long size = 0;
            string hash = CentralServerShared.HttpGetRetriever.DownloadNewHash(cache.AbsoluteUri, out size);
            if (hash != null)
            {
                bool changed = cache.Hash != hash;

                CreatePreviousUpdate(cache, changed);

                if (changed)
                {
                    cache.RecommendedUpdates.Add(new CentralServerShared.RecommendedUpdate()
                    {
                        DateCreated = DateTime.Now,
                        Sent = false,
                        Server = cache.Server
                    });
                    LogsController.WriteMessage("Cache has changed: " + cache.AbsoluteUri);
                }
            }
        }
    }
}
