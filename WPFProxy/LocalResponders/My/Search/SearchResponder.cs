using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using ServiceEntities;
using HtmlAgilityPack;
using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.My.Search
{
    class SearchResponder : MyDomainResponder
    {
        protected override string ThisUrl { get { return "search/"; } } 
        public SearchResponder()
            : base()
        {
        }
        protected override void InitRoutes()
        {
            Routes
                .RegisterRoute("GET", "", Index)
                .RegisterRoute("GET", "index.html", Index)
                .RegisterRoute("GET", "results", Results);
        }
        // GET: my.ownet/search/
        // GET: my.ownet/search/index.html
        private static ResponseResult Index(string method, string relativeUrl, RequestParameters parameters)
        {
            return new ResponseResult() { Data = LocalHelpers.MenuHelper.ReplyToPageWithMenu("search.html") };
        }

        // GET: my.ownet/search/results?query={query}
        private static ResponseResult Results(string method, string relativeUrl, RequestParameters parameters)
        {
            return new ResponseResult() { Data = ReplyToSearch(relativeUrl, parameters) };
        }

        private static byte[] ReplyToSearch(string relativeUrl, RequestParameters pars)
        {
            string query = pars.GetValue("query");
            string type = pars.GetValue("type");
            string dquery = HttpUtility.UrlDecode(query);

            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml("<div id=\"include\"></div>");

            if (type.Equals("server") && Controller.UseDataService)
            {
                ServerSearchResults serverResults = ServiceCommunicator.ReceiveServerSearchResults(dquery, HttpUtility.UrlDecode(pars.GetValue("clients")));
                string nextSearchId = "show_more" + Helpers.Common.RandomString(5, false);

                IncludeSearchedPages(htmlDoc, serverResults.Results);
                string onclick = "TabLoad('" + nextSearchId + "', 'search/results?query={0}&clients={1}&type={2}');return false;";
                TagBuilder tag = new TagBuilder()
                    .StartTag("div")
                        .AddAttribute("class", "paging")
                        .StartTag("a")
                            .AddAttribute("href", "javascript:void(0);")
                            .AddAttribute("onclick", String.Format(onclick, query,
                            HttpUtility.UrlEncode(Helpers.ConvertDictionary.GetString(serverResults.SearchedClients)), type))
                            .AddContent("Show more")
                        .EndTag() // a
                    .EndTag(); // div

                HtmlNode root = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"include\"]");
                if (root != null)
                {
                    HtmlNode resultsNode = htmlDoc.CreateElement("div");
                    resultsNode.SetAttributeValue("id", nextSearchId);
                    root.AppendChild(resultsNode);
                    resultsNode.InnerHtml = tag.ToString();
                }
            }
            else
            {
                string page = pars.GetValue("page");
                if (String.IsNullOrWhiteSpace(page)) page = "1";
                int pag = Convert.ToInt32(page);
                SearchResults recs = null;

                if (type.Equals("tags")) // && Controller.UseContentService)
                    recs = ServiceCommunicator.ReceiveSearchTagsResults(dquery, pag);
                else if (type.Equals("local"))
                    recs = SearchDatabase.Search(dquery, pag);

                IncludeSearchedPages(htmlDoc, recs.Results);
                IncludePaging(htmlDoc, recs.TotalPages, recs.CurrentPage, "search_results", "search/" + relativeUrl, "query=" + query + "&type=" + type);
            }

            return Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml);

        }

        private static void IncludeSearchedPages(HtmlDocument htmlDoc, List<PageObjectWithContent> recs)
        {
            if (recs == null || !recs.Any())
            {
                IncludeNoResultsMessage(htmlDoc, "There are no results for given query.");
                return;
            }

            HtmlNode root = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"include\"]");
            if (root == null) return;
            HtmlNode resultsNode = htmlDoc.CreateElement("div");
            root.AppendChild(resultsNode);

            HtmlNode linkNode;
            foreach (PageObjectWithContent rec in recs)
            {
                linkNode = htmlDoc.CreateElement("div");
                linkNode.SetAttributeValue("class", "search-results");

                linkNode.InnerHtml = String.Format("<h3><a href=\"{0}\" target=\"_blank\" onclick=\"return useSearch(this);\">{1}</a></h3>" +
                                "<p>{2}</p>" +
                                "<a href=\"{0}\" class=\"link-web\" target=\"_blank\" onclick=\"return useSearch(this);\">{0}</a>",
                                rec.AbsoluteURI,
                                rec.Title,
                                rec.Content);

                resultsNode.AppendChild(linkNode);
            }
        }
    }
}
