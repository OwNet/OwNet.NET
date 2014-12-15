using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WPFProxy.Proxy;
using System.Web;
using HtmlAgilityPack;

namespace WPFProxy.LocalResponders.Proxy.Errors
{
    class ErrorsResponder : ProxyDomainResponder
    {
        protected override string ThisUrl { get { return "errors/"; } }
        public ErrorsResponder()
            : base()
        {
        }

        protected override void InitRoutes()
        {
            Routes
                .RegisterRoute("GET", "offline.html", Offline);
        }

        // GET: proxy.ownet/errors/offline.html
        private static ResponseResult Offline(string method, string relativeUrl, RequestParameters parameters)
        {
            if (!RequestParameters.IsNullOrEmpty(parameters))
            {
                string page = HttpUtility.UrlDecode(parameters.GetValue("page"));
                string refe = HttpUtility.UrlDecode(parameters.GetValue("ref"));


                HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

                htmlDoc.Load(Controller.GetAppResourcePath("Html/offline.html"), Encoding.UTF8);

              //  HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"fromtitle\"]");

              //  node.SetAttributeValue("value", HtmlCacheResponder.GetWebsiteTitleAndDescription(refe).Item1);
                HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"fromuri\"]");
                node.SetAttributeValue("value", refe);
                node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"touri\"]");
                node.SetAttributeValue("value", page);
                return new ResponseResult() { Data = Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml) };
            }
            return LocalResponder.LoadProxyLocalFile("offlinenew.html");
            //return new ResponseResult() { Data = LocalHelpers.MenuHelper.ReplyToPageWithMenu("404new.html"), Filename = "index.html" };
        }


    }
}
