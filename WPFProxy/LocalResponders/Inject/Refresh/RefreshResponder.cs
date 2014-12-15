using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WPFProxy.Proxy;
using WPFProxy.Cache;

namespace WPFProxy.LocalResponders.Inject.Refresh
{
    class RefreshResponder : InjectDomainResponder
    {
        protected override string ThisUrl { get { return "refresh/"; } }
        public RefreshResponder()
            : base()
        {
        }
        protected override void InitRoutes()
        {
            Routes
                .RegisterRoute("GET", "start", delegate(string method, string relativeUrl, RequestParameters parameters)
            {
                ProxyCache.RefreshAll = true;
                return SimpleOKResponse("", "js");
            })
                .RegisterRoute("GET", "stop", delegate(string method, string relativeUrl, RequestParameters parameters)
            {
                ProxyCache.RefreshAll = false;
                CacheReporter.ReportToServer();
                return SimpleOKResponse("", "js");
            });
        }
    }
}
