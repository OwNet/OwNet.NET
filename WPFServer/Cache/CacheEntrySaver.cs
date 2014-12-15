using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientAndServerShared;
using System.Threading;
using WPFServer.DatabaseContext;
using WPFServer.Proxy;

namespace WPFServer.Cache
{
    public class CacheEntrySaver : SharedProxy.Cache.CacheEntrySaver
    {
        protected override void ExecuteSave(Dictionary<int, SaveItem> copyToSave)
        {
            if (copyToSave.Count == 0)
                return;

            var keys = copyToSave.Keys.ToList();

            MyDBContext context = null;
            try
            {
                context = new MyDBContext();
                var caches =  context.Fetch<DatabaseContext.Cache.Cache>(c => keys.Contains(c.Id));

                if (caches.Any())
                {
                    foreach (var cache in caches)
                    {
                        SaveItem item = copyToSave[cache.Id];
                        if (item.Entry != null)
                        {
                            ((ProxyEntry) item.Entry).UpdateInDatabase(item.IncreaseAccessCount, cache, context);
                        }
                        else
                        {
                            ProxyEntry.IncreaseAccessCount(item.IncreaseAccessCount, item.Size, cache);
                        }
                        keys.Remove(cache.Id);
                    }
                }

                foreach (int id in keys)
                {
                    SaveItem item = copyToSave[id];
                    if (item.Entry != null)
                    {
                        ((ProxyEntry)item.Entry).UpdateInDatabase(item.IncreaseAccessCount, null, context);
                    }
                }

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                LogsController.WriteException("SaveNow", ex.Message);
            }
            finally
            {
                if (context != null)
                    context.Dispose();
            }
        }
    }
}
