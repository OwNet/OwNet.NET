using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.Inject.Prefetch
{

    class PrefetchResponder : InjectDomainResponder
    {
        protected override string ThisUrl { get { return "prefetch/"; } }
        public PrefetchResponder() : base()
        {
        }
        protected override void InitRoutes()
        {
            Routes
                .RegisterRoute("GET", "done", Done)
                .RegisterRoute("GET", "cancel", Cancel);
        }

        // GET: inject.ownet/prefetch/done
        private static ResponseResult Done(string method, string relativeUrl, RequestParameters parameters)
        {
            if (!RequestParameters.IsNullOrEmpty(parameters))
            {
                string page = System.Web.HttpUtility.UrlDecode(parameters.GetValue("page"));
                string from = parameters.GetValue("for");
                if (from.Equals("proxy"))
                {
                    System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(page, @"^http://proxy.ownet/prefetch/load\?page=(.*)");
                    if (match.Success)
                    {
                        page = match.Groups[1].Value;
                        page = System.Web.HttpUtility.UrlDecode(page);
                    }

                    System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(delegate(object pars) { Prefetcher.DownloadCompleted((string)pars); }), (object)page);
                }
                else
                {
                    System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(page, @"^http://server.ownet/prefetch/load\?page=(.*)");
                    if (match.Success)
                    {
                        page = match.Groups[1].Value;
                        page = System.Web.HttpUtility.UrlDecode(page);
                    }
                    ServiceCommunicator.ReportPrefetchCompleted(page);
                }
            }
            return SimpleOKResponse("", "js");
        }

        // GET: inject.ownet/prefetch/cancel
        private static ResponseResult Cancel(string method, string relativeUrl, RequestParameters parameters)
        {
            if (Settings.RealTimePrefetchingEnabled && !RequestParameters.IsNullOrEmpty(parameters))
            {
                string page = System.Web.HttpUtility.UrlDecode(parameters.GetValue("page"));
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(delegate(object pars) { Prefetcher.DisablePredictions(page); }));
            }
            return SimpleOKResponse("", "js");
        }
    }
}
