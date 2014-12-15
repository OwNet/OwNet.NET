using System;

namespace WPFProxy.Proxy
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
            SharedProxy.Services.Client.CacheServiceRequester.ReportItem(new ServiceEntities.Cache.NewCacheReport()
            {
                AbsoluteUri = url,
                DateModified = dateModified
            });
        }
    }
}
