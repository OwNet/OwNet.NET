using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ServiceEntities;
using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.My.Rating
{
    class RatingResponder : MyDomainResponder
    {
        protected override string ThisUrl { get { return "rating/"; } } 
        
        public RatingResponder() : base() 
        {
        }
        protected override void InitRoutes()
        {
            Routes
                .RegisterRoute("GET", "", Index)
                .RegisterRoute("GET", "index.html", Index)
                .RegisterRoute("GET", "list/.*", List);
               // .RegisterRoute("GET", "read", Read)
               // .RegisterRoute("POST", "create", Create);
        }

       
        // GET: my.ownet/rating/
        // GET: my.ownet/rating/index.html
        private static ResponseResult Index(string method, string relativeUrl, RequestParameters parameters)
        {
            return new ResponseResult() { Data = LocalHelpers.MenuHelper.ReplyToPageWithMenu("rated.html") };
        }
        // POST: my.ownet/rating/create
        private static ResponseResult Create(string method, string relativeUrl, RequestParameters parameters)
        {
            if (Controller.UseServer)
            {
                if (ServiceCommunicator.SendRating(parameters))
                    return SimpleOKResponse();
            }
            return SimpleOKResponse("{ \"status\" : \"FAILED\" }");
        }
        // GET: my.ownet/rating/read?page={page}
        private static ResponseResult Read(string method, string relativeUrl, RequestParameters parameters)
        {
            double avgrating = 0.0;
            int rating = 0;
            bool ret = ServiceCommunicator.ReceiveRating(parameters.GetValue("page"), out rating, out avgrating);
            string json = "{ \"rating\" : \"" + rating + "\", \"avgrating\" : \"" + Convert.ToString(avgrating).Replace(',', '.') + "\"}";
            return SimpleOKResponse(json);
        }

        // GET: my.ownet/rating/list/{group}?page={page}
        private static ResponseResult List(string method, string relativeUrl, RequestParameters parameters)
        {
            byte[] data = null;
            if (RequestParameters.IsNullOrEmpty(parameters))
            {
                data = ReplyToRated(relativeUrl);
            }
            else
            {
                data = ReplyToRated(relativeUrl, parameters.GetValue("page"));
            }
            return new ResponseResult() { Data = data };
        }

        private static byte[] ReplyToRated(string relativeUrl, string page = "1")
        {
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            if (LocalHelpers.LoginHelper.IncludeLoginRequiredMessage(htmlDoc) == false)
            {
                htmlDoc.LoadHtml("<div id=\"include\"></div>");
                int pag = Convert.ToInt32(page);
                RatedPages recs;
                string group;
                if (relativeUrl.Equals("list/user"))
                {
                    group = "user";
                    recs = ServiceCommunicator.ReceiveUserTopRated(pag);

                }
                else
                {
                    group = "average";
                    recs = ServiceCommunicator.ReceiveAllTopRated(pag);
                }

                IncludeRatedPages(htmlDoc, recs);
                IncludePaging(htmlDoc, recs.TotalPages, recs.CurrentPage, group, "rating/" + relativeUrl);
            }
            return Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml);

        }
        private static void IncludeRatedPages(HtmlDocument htmlDoc, RatedPages recs)
        {
            if (recs == null || !recs.Ratings.Any())
            {
                IncludeNoResultsMessage(htmlDoc, "No page has been rated yet.");
                return;
            }
            HtmlNode root = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"include\"]");
            if (root == null) return;
            HtmlNode resultsNode = htmlDoc.CreateElement("table");
            resultsNode.SetAttributeValue("class", "table_links");
            root.AppendChild(resultsNode);

            HtmlNode linkNode;
            foreach (PageObjectWithRating rec in recs.Ratings)
            {
                linkNode = htmlDoc.CreateElement("tr");

                linkNode.InnerHtml = String.Format("<td class=\"table_star\">"
                    + "	<ul class=\"star-rating tabsmargin\" id=\"star-rating-1\">"
                        + "	<li class=\"current-rating\" id=\"current-rating-1\" style=\"width: {0}%;\"></li>"
                        + "</ul>"
                    + "</td>"
                    + "<td class=\"table_link\">"
                        + "<a href=\"{1}\" target=\"_blank\" class=\"weblink\" onclick=\"return useRating({3});\">{2}</a>" //<br />"
                    //  + "<small>{3}<small>"
                    + "</td>"
                    + "<td class=\"table_date\"> </td>",
                    //<img src=\"graphics/time.png\" class=\"img-4\" alt=\"\"/> {4} <br />"
                    // + "<small>{5}</small></td>", //včera 23.1.2012 12:58</td>
                                Convert.ToInt32(rec.AvgRating * 20),
                                rec.AbsoluteURI,
                                (rec.Title.Length > 80 ? rec.Title.Substring(0, 75) + "&hellip;" : rec.Title),
                                rec.Id);
                resultsNode.AppendChild(linkNode);
            }
        }

    }
}
