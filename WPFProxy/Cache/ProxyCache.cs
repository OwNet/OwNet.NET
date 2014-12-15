using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace WPFProxy.Cache
{
    public class ProxyCache : Helpers.Proxy.ProxyCache
    {
        public static bool RefreshAll = false;

        public static void PrimaryPageReported(string url)
        {
            SearchDatabase.SaveFromCache(url);
        }
    }
}
