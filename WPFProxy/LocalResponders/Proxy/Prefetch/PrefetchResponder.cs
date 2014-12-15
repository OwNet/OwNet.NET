using System.Text;
using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.Proxy.Prefetch
{
    class PrefetchResponder : ProxyDomainResponder
    {
        protected override string ThisUrl { get { return "prefetch/"; } }
        public PrefetchResponder()
            : base()
        {
        }

        protected override void InitRoutes()
        {
            Routes
                .RegisterRoute("GET", "load", Redirect);
        }

        // GET: proxy.ownet/prefetch/load
        private static ResponseResult Redirect(string method, string relativeUrl, RequestParameters parameters)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            if (!RequestParameters.IsNullOrEmpty(parameters))
            {
                string url = System.Web.HttpUtility.UrlDecode(parameters.GetValue("page"));
                htmlDoc.LoadHtml("<html><body onload=\"document.links[0].click();\"><a id=\"clickme\" href=\"" + url + "\">prefetch this</a></body></html>");
                return new ResponseResult() { Data = Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml) };
            }
            return null;
        }
    }
}
