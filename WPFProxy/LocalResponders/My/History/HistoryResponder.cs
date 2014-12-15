using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ServiceEntities;
using System.Web;
using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.My.History
{
    class HistoryResponder : MyDomainResponder
    {
        protected override string ThisUrl { get { return "history/"; } }
        public HistoryResponder() : base()
        {
        }
        protected override void InitRoutes()
        {
            Routes.RegisterRoute("GET", "", Index)
                .RegisterRoute("GET", "index.html", Index)
                .RegisterRoute("GET", "list/.*", List)
                .RegisterRoute("POST", "delete", Delete);
        }

        // GET: my.ownet/history/
        // GET: my.ownet/history/index.html
        private static ResponseResult Index(string method, string relativeUrl, RequestParameters parameters)
        {

            return new ResponseResult() { Data = LocalHelpers.MenuHelper.ReplyToPageWithMenu("history.html"), Filename = "index.html" };
        }

        // GET: my.ownet/history/list/{group}?page={page}
        private static ResponseResult List(string method, string relativeUrl, RequestParameters parameters)
        {
            byte[] data = null;
            if (RequestParameters.IsNullOrEmpty(parameters))
            {
                data = ReplyToHistory(relativeUrl);
            }
            else
            {
                data = ReplyToHistory(relativeUrl, parameters.GetValue("page"));
            }
            return new ResponseResult() { Data = data };
        }

        // POST: my.ownet/history/delete
        private static ResponseResult Delete(string method, string relativeUrl, RequestParameters parameters)
        {
            string hid = parameters.GetValue("id");
            bool delhis = ServiceCommunicator.DeleteHistory(hid);
            if (delhis)
                return SimpleOKResponse();
            return SimpleOKResponse("{ \"status\" : \"FAILED\" }");
        }

        private static byte[] ReplyToHistory(string relativeUrl, string page = "1")
        {
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            if (LocalHelpers.LoginHelper.IncludeLoginRequiredMessage(htmlDoc) == false)
            {
                htmlDoc.LoadHtml("<div id=\"include\"></div>");
                int pag = Convert.ToInt32(page);
                VisitedPages recs = null;
                string period = System.Text.RegularExpressions.Regex.Replace(relativeUrl, "^list/", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).ToLower();

                recs = ServiceCommunicator.ReceiveUserHistory(period, pag);


                IncludeHistoryPages(htmlDoc, recs ?? new VisitedPages());
                IncludePaging(htmlDoc, recs.TotalPages, recs.CurrentPage, period, "history/" + relativeUrl);
            }
            return Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml);

        }

        private static void IncludeHistoryPages(HtmlDocument htmlDoc, VisitedPages recs)
        {
            if (recs == null || !recs.Visits.Any())
            {
                IncludeNoResultsMessage(htmlDoc, "You have not visited any webpage for this period.");
                return;
            }

            HtmlNode root = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"include\"]");
            if (root == null) return;
            HtmlNode resultsNode = htmlDoc.CreateElement("table");
            resultsNode.SetAttributeValue("class", "table_links");
            root.AppendChild(resultsNode);

            HtmlNode linkNode;
            foreach (PageObjectWithDateTime rec in recs.Visits)
            {
                linkNode = htmlDoc.CreateElement("tr");
                //      linkNode.SetAttributeValue("onmouseover", "Color_tr(this, '#fef2dc');");
                //      linkNode.SetAttributeValue("onmouseout", "Color_tr_off(this);");
                /*
                    </td>
                    <td class="table_date"><img src="graphics/time.png" class="img-4" alt=""/> today 23.1.2012 12:58</td>
                </tr>*/
                linkNode.InnerHtml = String.Format("<td class=\"table_history\">"
                    + "	<img src=\"graphics/folder_link.png\" class=\"img-4\" alt=\"\"/>"
                    + " <a href=\"{0}\" target=\"_blank\" class=\"weblink\" onclick=\"return useHistory({5});\">{1}</a><br />"
                    + " <a href=\"{0}\" target=\"_blank\" onclick=\"return useHistory({5});\"><small>{2}</small></a>"
                    + "</td> <td class=\"table_date\"><span class=\"activity_time\">{3} {4}</span></td>"
                    + "<td class=\"\" style=\"width: 20px;\"><a href=\"javascript:void(0);\" onclick=\"var that = this; confirm('Do you want to remove page <strong>{1}</strong> from your history?<br />If you rated this page, rating would be removed too.', function () {{ deleteHistory({5}, that); }});\"><img src=\"graphics/delete.png\" class=\"img-4\" alt=\"Remove page\" title=\"Remove page\"/></a></td>",
                                rec.AbsoluteURI,
                                (rec.Title.Length > 200 ? rec.Title.Substring(0, 195) + "&hellip;" : rec.Title),
                                (rec.AbsoluteURI.Length > 400 ? rec.AbsoluteURI.Substring(0, 395) + "&hellip;" : rec.AbsoluteURI),
                                (rec.VisitTimeStamp.Date.Equals(DateTime.Today) ? "today" : ""),
                                rec.VisitTimeStamp.ToString("dd.MM.yyyy HH:mm"),
                                rec.Id);
                resultsNode.AppendChild(linkNode);
            }
        }



    }
}
