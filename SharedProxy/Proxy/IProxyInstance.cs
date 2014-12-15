using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedProxy.Proxy
{
    public interface IProxyInstance
    {
        ProxyEntry CreateCacheEntry(string url);

        ProxyEntry CreateCacheEntry(ServiceEntities.Cache.CacheRequest request);

        void ReportCacheToMainServer(string url, DateTime dateModified);
    }
}
