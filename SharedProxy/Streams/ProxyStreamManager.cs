using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceEntities;
using System.Threading;
using ClientAndServerShared;
using SharedProxy.Streams.Input;
using SharedProxy.Services.Host;
using SharedProxy.Streams.Output;
using SharedProxy.Proxy;

namespace SharedProxy.Streams
{
    public class ProxyStreamManager
    {
        private static Dictionary<int, StreamItem> _streams = new Dictionary<int, StreamItem>();

        public static ServiceItem CreateServiceItem(ProxyEntry entry, bool downloadIfNotFound)
        {
            ProxyServiceContextEntities context = new ProxyServiceContextEntities();
            ServiceItem item = context.GetFreeItem();
            if (!item.CreateStreamItem(entry, downloadIfNotFound))
            {
                ProxyServiceContextEntities.DisposeItem(item);
                return null;
            }

            return item;
        }

        public static StreamItem CreateStreamItem(ProxyEntry entry, out int downloadMethod, out int readerId, bool increaseTraffic = true, bool cacheOnly = false)
        {
            int hash = Helpers.Proxy.ProxyCache.GetUriHash(entry.AbsoluteUri);
            StreamItem item = null;
            InMemoryStreamItem inMemoryStreamItem = null;
            bool needsInit = false;
            bool update = false;

            if (increaseTraffic)
            {
                SharedProxy.Proxy.ProxyTraffic.IncreaseCurrentTraffic();
                LogsController.LogPageAccess(entry.AbsoluteUri);
            }

            // Check if it is currently being downloaded
            lock (_streams)
            {
                if (_streams.ContainsKey(hash))
                {
                    item = _streams[hash];
                    if (entry.Refresh && item.Source == StreamItem.SourceTypes.Cache)
                    {
                        _streams.Remove(hash);
                        item.IsReferenced = false;
                        item = null;
                    }
                    else
                    {
                        downloadMethod = (int)ProxyEntry.DownloadMethods.FromCacheOverServer;
                        readerId = item.RegisterReader();
                        entry.Update(item.CacheEntry);
                        return item;
                    }
                }
            }

            // If can cache on server or client, check if it is in cache
            if (entry.CanCache > 0 && !entry.Refresh)
            {
                item = new CacheStreamItem();
                if (!item.Init(entry))
                    item = null;
                else
                    update = true;

                if (item != null)
                {
                    downloadMethod = (int)item.DownloadMethod;
                    readerId = item.RegisterReader();
                    return item;
                }
                if (cacheOnly)
                {
                    downloadMethod = readerId  = -1;
                    return null;
                }
            }

            // If it cant be downloaded from the Web, or data service
            if (cacheOnly)
            {
                downloadMethod = 0;
                readerId = -1;
                return null;
            }

            bool canCache = Controller.IsOnServer ? (entry.CanCache & (int)ProxyEntry.CanCacheOptions.CanCacheOnServer) > 0
                : (entry.CanCache & (int)ProxyEntry.CanCacheOptions.CanCacheOnClient) > 0;
            bool dataServiceChecked = false;

            // If no caching is allowed
            if (!canCache && !cacheOnly)
            {
                bool useServer = entry.UseServer;
                if (useServer && Controller.AppSettings.DownloadEverythingThroughServer())
                    useServer = true;
                else if (!Controller.IsOnServer && (entry.CanCache & (int)ProxyEntry.CanCacheOptions.CanCacheOnServer) == 0)
                    useServer = false;

                if (useServer)
                {
                    item = new DirectDataServiceStreamItem();
                    if (!item.Init(entry))
                    {
                        canCache = (entry.CanCache & (int)ProxyEntry.CanCacheOptions.CanCacheOnServer) > 0;
                        item = null;
                    }
                    dataServiceChecked = true;
                }
                if (!canCache)
                {
                    if (item == null)
                    {
                        item = new DirectWebStreamItem();
                        item.Init(entry);
                    }

                    ThreadPool.QueueUserWorkItem(new WaitCallback(Download), item);
                    item.IsReferenced = false;
                    readerId = item.RegisterReader();
                    downloadMethod = (int)item.DownloadMethod;
                    return item;
                }
            }

            if (item == null)
            {
                if (entry.UseServer && !dataServiceChecked)
                {
                    item = inMemoryStreamItem = new DataServiceStreamItem();
                    if (!item.Init(entry))
                        item = null;
                }
                if (item == null)
                {
                    item = inMemoryStreamItem = new WebStreamItem();
                    needsInit = true;
                }
            }
            
            downloadMethod = (int)item.DownloadMethod;

            int cacheWriterId = -1;
            lock (_streams)
            {
                if (_streams.ContainsKey(hash))
                {
                    if (entry.Refresh && _streams[hash].Source == StreamItem.SourceTypes.Cache)
                    {
                        _streams[hash].IsReferenced = false;
                        _streams.Remove(hash);
                    }
                    else
                    {
                        downloadMethod = (int)ProxyEntry.DownloadMethods.FromCacheOverServer;
                        
                        item.DisposeItem();

                        item = _streams[hash];
                        readerId = item.RegisterReader();
                        entry.Update(item.CacheEntry);
                        return item;
                    }
                }
                readerId = item.RegisterReader();
                if (canCache)
                    cacheWriterId = item.RegisterReader();
                _streams.Add(hash, item);
            }

            if (needsInit)
                item.Init(entry);

            if (!item.IsSuccessful)
            {
                item.DisposeAllReaders();
                return null;
            }

            ThreadPool.QueueUserWorkItem(new WaitCallback(Download), item);

            if (canCache)
            {
                if ((!entry.CanCacheHtml && entry.IsHTMLDocument()) || inMemoryStreamItem == null)
                    item.DisposeReader(cacheWriterId);
                else
                    RegisterCacheWriter(inMemoryStreamItem, update, cacheWriterId);
            }

            return item;
        }

        private static void RegisterCacheWriter(InMemoryStreamItem item, bool update, int readerId)
        {
            item.IsSavedToCache = true;
            CacheWriter writer = new CacheWriter(item, update, readerId);
            ThreadPool.QueueUserWorkItem(new WaitCallback(SaveToCache), writer);
        }

        private static void Download(Object obj)
        {
            StreamItem item = (StreamItem)obj;
            try
            {
                item.Download();
            }
            catch (Exception ex)
            {
                Controller.WriteException("Download", ex.Message);
            }
        }

        private static void SaveToCache(Object obj)
        {
            CacheWriter item = (CacheWriter)obj;
            Controller.ProxyInstance.ReportCacheToMainServer(item.AbsoluteUri, item.Entry.DateModified);
            try
            {
                item.Save();
            }
            catch (Exception ex)
            {
                Controller.WriteException("Save to cache", ex.Message);
            }
        }

        public static void DisposeStreamItem(StreamItem proxyStreamItem)
        {
            lock (_streams)
            {
                if (proxyStreamItem.IsUsed)
                    return;
                
                if (proxyStreamItem.IsReferenced)
                    _streams.Remove(proxyStreamItem.UriHash);
                proxyStreamItem.IsReferenced = false;
            }
            proxyStreamItem.DisposeItem();
        }
    }
}
