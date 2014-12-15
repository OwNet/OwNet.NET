using System;
using System.Web;
using ServiceEntities;
using WPFProxy.Cache;
using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.Inject.History
{
    class HistoryResponder : InjectDomainResponder
    {
        protected override string ThisUrl { get { return "history/"; } }
        public HistoryResponder() : base()
        {
        }
        protected override void InitRoutes()
        {
            Routes.RegisterRoute("GET", "create", Create)
                .RegisterRoute("GET", "read", Read);
        }

        // GET: inject.ownet/history/create
        private static ResponseResult Create(string method, string relativeUrl, RequestParameters parameters)
        {
            string page = HttpUtility.UrlDecode(parameters.GetValue("page"));
            string referrer = HttpUtility.UrlDecode(parameters.GetValue("ref"));
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(delegate(object pars)
            {
                // TODO refactor 

                if (Controller.UseServer)
                {
                    if (String.IsNullOrWhiteSpace(referrer) || Settings.RealTimePrefetchingEnabled == false)
                    {
                        ServiceCommunicator.RegisterSearch(page);
                    }
                    else
                    {
                        ServiceCommunicator.RegisterEdge(referrer, page);
                        if (Settings.RealTimePrefetchingEnabled) 
                            Prefetcher.DisablePredictions(referrer);
                    }

                    if (Settings.RealTimePrefetchingEnabled) 
                    {
                        //List<string> links = Cache.CacheResponder.GetLinks(page);
                        //if (links != null)
                        //{
                        //    List<string> cachedLinks = Cache.CacheResponder.SelectCachedLinks(page, links, true);
                        //    links = links.Except(cachedLinks).ToList();
                        PrefetchLinks toPrefetch = ServiceCommunicator.ReceivePrefetchPrediction(page, null);
                        if (toPrefetch != null)
                        {
                            Prefetcher.RegisterPredictions(toPrefetch.From, toPrefetch.Links);
                        }
                        //}
                    }
                }
                ProxyCache.PrimaryPageReported(page);
            }));

            return SimpleOKResponse("", "js");
        }
        // GET: inject.ownet/history/read
        private static ResponseResult Read(string method, string relativeUrl, RequestParameters parameters)
        {
            if (Settings.RealTimePrefetchingEnabled)
            {
                string page = HttpUtility.UrlDecode(parameters.GetValue("page"));
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(delegate(object pars)
                {
                    Prefetcher.EnablePredictions(page);
                }));
            }
            return SimpleOKResponse("", "js");
        }




    }
}
