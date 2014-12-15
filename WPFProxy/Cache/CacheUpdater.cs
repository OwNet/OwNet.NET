using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedProxy;
using WPFProxy.Proxy;
using WPFProxy.Database;

namespace WPFProxy.Cache
{
    class CacheUpdater
    {
        internal static void ProcessLinksToUpdate(List<ServiceEntities.Cache.CacheLinkToUpdate> linksToUpdate)
        {
            if (linksToUpdate.Count == 0)
                return;

            using (DatabaseEntities context = Controller.GetDatabase())
            {
                List<string> updateUris = new List<string>();
                updateUris.AddRange(linksToUpdate.Select(l => l.AbsoluteUri));

                var existingUpdates = from c in context.CacheUpdates
                                      where updateUris.Contains(c.AbsoluteUri)
                                      select c;
                foreach (var existingUpdate in existingUpdates)
                {
                    updateUris.Remove(existingUpdate.AbsoluteUri);
                }

                foreach (var link in linksToUpdate)
                {
                    context.CacheUpdates.AddObject(new CacheUpdates()
                    {
                        AbsoluteUri = link.AbsoluteUri,
                        Priority = link.Priority,
                        DateCreated = DateTime.Now
                    });
                }
                context.SaveChanges();
            }
        }

        internal static void UpdateCache()
        {
            if (SharedProxy.Proxy.ProxyTraffic.LastTraffic() < 2)
            {
                using (DatabaseEntities context = Controller.GetDatabase())
                {
                    var updates = from p in context.CacheUpdates
                                    orderby p.Priority descending
                                    select p;

                    List<ServiceEntities.Cache.CacheLog> updatedItems = new List<ServiceEntities.Cache.CacheLog>();
                    foreach (var update in updates)
                    {
                        DateTime startTime = DateTime.Now;
                        SharedProxy.Controller.UpdateCache(update.AbsoluteUri, update.DateCreated ?? DateTime.Now);
                        TimeSpan ts = DateTime.Now - startTime;

                        ClientAndServerShared.LogsController.WriteMessage("Cache updated " + update.AbsoluteUri);
                        updatedItems.Add(new ServiceEntities.Cache.CacheLog()
                        {
                            AbsoluteUri = update.AbsoluteUri,
                            FetchDuration = ts.TotalSeconds,
                            AccessedAt = DateTime.Now,
                            DownloadedFrom = (int)ProxyEntry.DownloadMethods.RefreshCacheOnServer,
                            Type = (int)ProxyEntry.HttpTypes.GET
                        });

                        context.CacheUpdates.DeleteObject(update);

                        if (SharedProxy.Proxy.ProxyTraffic.LastTraffic() > 1)
                            break;
                    }

                    if (updatedItems.Count > 0)
                    {
                        lock (CacheReporter.NotReportedItems)
                            CacheReporter.NotReportedItems.AddRange(updatedItems);

                        context.SaveChanges();
                    }
                }
            }
        }
    }
}
