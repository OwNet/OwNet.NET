using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ServiceEntities;
using System.Web;
using WPFProxy.Proxy;
using WPFProxy.Database;
using System.Text.RegularExpressions;

namespace WPFProxy.LocalResponders.My.Prefetch
{
    class PrefetchResponder : MyDomainResponder
    {
        protected override string ThisUrl { get { return "prefetch/"; } } 
        
        public PrefetchResponder() : base() 
        {
        }
        protected override void InitRoutes()
        {
            Routes
                .RegisterRoute("GET", "", Index)
                .RegisterRoute("GET", "index.html", Index)
                .RegisterRoute("GET", "list/.*", List)
                .RegisterRoute("POST", "delete", Delete)
                .RegisterRoute("POST", "create", Create);
               // .RegisterRoute("GET", "read", Read)
               // .RegisterRoute("POST", "create", Create);
        }

       
        // GET: my.ownet/prefetch/
        // GET: my.ownet/prefetch/index.html
        private static ResponseResult Index(string method, string relativeUrl, RequestParameters parameters)
        {
            return new ResponseResult() { Data = LocalHelpers.MenuHelper.ReplyToPageWithMenu("prefetch.html") };
        }
        

        // GET: my.ownet/prefetch/list/{group}?page={page}
        private static ResponseResult List(string method, string relativeUrl, RequestParameters parameters)
        {
            byte[] data = null;
            if (RequestParameters.IsNullOrEmpty(parameters))
            {
                data = ReplyToPrefetch(relativeUrl);
            }
            else
            {
                data = ReplyToPrefetch(relativeUrl, parameters.GetValue("page"));
            }
            return new ResponseResult() { Data = data };
        }

        // POST: my.ownet/prefetch/create
        private static ResponseResult Create(string method, string relativeUrl, RequestParameters parameters)
        {
            string fromtitle = HttpUtility.UrlDecode(parameters.GetValue("fromtitle")).Replace('+', ' ');
            string totitle = HttpUtility.UrlDecode(parameters.GetValue("totitle")).Replace('+', ' ');
            string fromuri = HttpUtility.UrlDecode(parameters.GetValue("fromuri"));
            string touri = HttpUtility.UrlDecode(parameters.GetValue("touri"));
            Prefetcher.RegisterSchedule(fromtitle, fromuri, totitle, touri);
            return ReplyToRedirect("http://my.ownet/prefetch/");
        }

        private static byte[] ReplyToPrefetch(string relativeUrl, string page = "1")
        {
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
           // if (LocalHelpers.LoginHelper.IncludeLoginRequiredMessage(htmlDoc) == false)
           // {
                htmlDoc.LoadHtml("<div id=\"include\"></div>");
                int pag = Convert.ToInt32(page);
                Prefetcher.PrefetchOrdersList recs;
                string group;
                if (relativeUrl.Equals("list/predicted"))
                {
                    group = "predicted";
                    recs = Prefetcher.GetPrefetchOrders(pag, false);
                }
                else
                {
                    group = "scheduled";
                    recs = Prefetcher.GetPrefetchOrders(pag, true);
                }

                IncludePrefetchOrders(htmlDoc, recs);
                IncludePaging(htmlDoc, recs.TotalPages, recs.CurrentPage, group, "prefetch/" + relativeUrl);
          //  }
            return Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml);

        }

        // POST: my.ownet/prefetch/delete
        private static ResponseResult Delete(string method, string relativeUrl, RequestParameters parameters)
        {
            string did = parameters.GetValue("id");
            try
            {
                bool deloret = Prefetcher.RemoveOrder(Convert.ToInt32(did));
                if (deloret)
                    return SimpleOKResponse();
            }
            catch { }
            return SimpleOKResponse("{ \"status\" : \"FAILED\" }");
        }

        private static void IncludePrefetchOrders(HtmlDocument htmlDoc, Prefetcher.PrefetchOrdersList recs)
        {
            if (recs == null || !recs.Orders.Any())
            {
                IncludeNoResultsMessage(htmlDoc, "No download orders have been submitted.");
                return;
            }
            HtmlNode root = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"include\"]");
            if (root == null) return;
            HtmlNode resultsNode = htmlDoc.CreateElement("table");
            resultsNode.SetAttributeValue("class", "table_links");
            resultsNode.SetAttributeValue("style", "margin-top: 20px");
            root.AppendChild(resultsNode);

            HtmlNode linkNode;
            linkNode = htmlDoc.CreateElement("tr");
            linkNode.InnerHtml =
                        "<th style=\"width: 400px; text-align: left; padding-left: 15px;\">Title &amp; Address</th>"
                        + "<th style=\"width: auto; text-align: left; padding-left: 15px;\">Status</th>"
                        + "<th style=\"width: 180px; text-align: left; padding-left: 15px;\">Created</th>"
                        + "<th style=\"width: 60px; text-align: center;\">Remove</th>";
            resultsNode.AppendChild(linkNode);

            Regex rreg = new Regex(@"[\r\n\t\'""]", RegexOptions.Compiled);
            foreach (PrefetchOrders order in recs.Orders)
            {
                string title = (order.ToTitle.Length > 200 ? order.ToTitle.Substring(0, 195) + "&hellip;" : order.ToTitle);
                linkNode = htmlDoc.CreateElement("tr");
                linkNode.SetAttributeValue("id", "order" + order.Id);
                linkNode.InnerHtml = String.Format(
                    "<td class=\"table_link\">"
                        + "<a href=\"{0}\" target=\"_blank\" class=\"weblink\">{1}</a><br />"
                        + "<a href=\"{0}\" target=\"_blank\" class=\"weblink\"><small>{0}</small></a>"
                    + "</td>"
                    + "<td>{2}</td>"
                    + "<td class=\"table_date\"><span class=\"activity_time\">{3} {4}</span></td>"
                    + "<td class=\"\" style=\"width: 20px;\"><a href=\"javascript:void(0);\" onclick=\"var that = this; confirm('Do you want to remove page <strong>{6}</strong> from your download orders?', function () {{ deleteOrder({5}, that); }});\"><img src=\"graphics/delete.png\" class=\"img-4\" alt=\"Remove page\" title=\"Remove page\"/></a></td>",
                                (order.ToAbsoluteUri.Length > 300 ? order.ToAbsoluteUri.Substring(0, 295) + "&hellip;" : order.ToAbsoluteUri),
                                title,
                                ((Prefetcher.Status)order.Status).ToMessage(),
                                (order.DateCreated.Date.Equals(DateTime.Today) ? "today" : ""),
                                order.DateCreated.ToString("dd.MM.yyyy HH:mm"),
                                order.Id,
                                rreg.Replace(title, " ")
                                );
                resultsNode.AppendChild(linkNode);
            }
        }

    }
}
