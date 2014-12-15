
namespace WPFProxy.LocalResponders.Proxy
{
    class ProxyDomainResponder : LocalResponder
    {
        protected override string ThisUrl { get { return "proxy.ownet/"; } }


        public ProxyDomainResponder()
            : base()
        {
        }

        protected override void InitRoutes()
        {
            Routes
                .RegisterRoute(null, "errors/.*", new Errors.ErrorsResponder().Respond)
                .RegisterRoute("GET", "prefetch/.*", new Prefetch.PrefetchResponder().Respond);
            InitContentRoutes();
        }
    }
}
