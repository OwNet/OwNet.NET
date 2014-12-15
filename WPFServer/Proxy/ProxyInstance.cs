using System;
using System.Collections.Generic;
using ServiceEntities.Cache;
using WPFServer.Proxy;

namespace WPFServer.Proxy
{
    class ProxyInstance : SharedProxy.Proxy.IProxyInstance
    {
        public SharedProxy.Proxy.ProxyEntry CreateCacheEntry(string url)
        {
            return new ProxyEntry(url);
        }

        public SharedProxy.Proxy.ProxyEntry CreateCacheEntry(ServiceEntities.Cache.CacheRequest request)
        {
            return new ProxyEntry(request);
        }

        public void ReportCacheToMainServer(string url, DateTime dateModified)
        {
            WPFServer.Clients.ClientCacheLogsReporter.ClientCacheReported(Properties.Settings.Default.CentralServerUsername, "", url, dateModified);
        }
    }
}
