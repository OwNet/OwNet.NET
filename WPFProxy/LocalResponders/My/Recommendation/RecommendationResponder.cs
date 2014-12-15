using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceEntities;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Web;
using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.My.Recommendation
{
    class RecommendationResponder : MyDomainResponder
    {
        protected override string ThisUrl { get { return "recommend/"; } }
        public RecommendationResponder()
            : base()
        { }

        protected override void InitRoutes()
        {
            Routes
                .RegisterRoute("GET", "", Index)
                .RegisterRoute("GET", "index.html", Index)
                .RegisterRoute("GET", "list/.*", List)
                .RegisterRoute("POST", "delete", Delete)
                .RegisterRoute("GET", "not_displayed", GetCountOfNotDisplayedRecomms);
        }

        // POST: my.ownet/recommend/delete
        private static ResponseResult Delete(string method, string relativeUrl, RequestParameters parameters)
        {
            bool delret = ServiceCommunicator.DeleteExplicitRecommendation(parameters.GetValue("id"), parameters.GetValue("group"));
            if (delret)
                return SimpleOKResponse();
            return SimpleOKResponse("{ \"status\" : \"FAILED\" }");
        }

        // GET: my.ownet/recommend/read
        private static ResponseResult Read(string method, string relativeUrl, RequestParameters parameters)
        {
            string rpage = parameters.GetValue("page");
            UserRecommendedPage recommend = ServiceCommunicator.ReceiveExplicitRecommendation(rpage);
            string rjson;
            Regex rreg = new Regex(@"[\r\n\t\'""]", RegexOptions.Compiled);
            if (recommend == null || recommend.Recommendation.IsSet == false)
            {
                Tuple<string, string> titleAndDescription = HtmlResponder.GetWebsiteTitleAndDescription(System.Web.HttpUtility.UrlDecode(rpage));
                string title = titleAndDescription.Item1;
                title = rreg.Replace(title, " ");
                title = (title.Length > 70) ? title.Substring(0, 70) : title;
                string desc = titleAndDescription.Item2;
                desc = rreg.Replace(desc, " ");
                desc = (desc.Length > 300) ? desc.Substring(0, 300) : title;
                rjson = "{ \"set\" : \"0\", \"title\" : \"" + title + "\", \"desc\" : \"" + desc + "\", \"user\" : \"\", \"group\" : \"\", \"edit\" : \"0\" }";

            }
            else
            {
                bool edit = false;
                string user = recommend.User.Firstname + " " + recommend.User.Surname;
                if (Settings.UserName.Equals(recommend.User.Username))
                {
                    user = "you";
                    edit = true;
                }
                if (Settings.UserTeacher)
                {
                    edit = true;
                }// TODO: ak je string so specialnymi znakmi \r\n a pod., tak je to zly json -> Encode/Decode
                rjson = "{ \"set\" : \"1\", \"title\" : \"" + rreg.Replace(recommend.Recommendation.Title, " ") + "\", \"user\" : \"" + user + "\", \"desc\" : \"" + rreg.Replace(recommend.Recommendation.Description, " ") + "\", \"group\" : \"" + recommend.Recommendation.Group.Name + "\", \"edit\" : \"" + ((edit) ? "1" : "0") + "\"}";
            }
            return SimpleOKResponse(rjson);
        }

        // GET: my.ownet/recommend/
        // GET: my.ownet/recommend/index.html
        private static ResponseResult Index(string method, string relativeUrl, RequestParameters parameters)
        {
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

            htmlDoc.Load(Controller.GetAppResourcePath("Html/recommended.html"), Encoding.UTF8);
            LocalHelpers.MenuHelper.IncludeLocalMenu(htmlDoc);
            if (htmlDoc.DocumentNode == null)
                throw new Exception("No document node.");

            ServiceEntities.GroupsList gl = ServiceCommunicator.GetLastUsedGroups();

            if (gl != null)
            {
                HtmlNode tabNode = htmlDoc.DocumentNode.SelectSingleNode("//ul[@id=\"nav_groups\"]");
                
                int i = 0;

                foreach(var group in gl){
                    i++;
                    TagBuilder tagB = new TagBuilder();

                    tagB = tagB.StartTag("li")
                           .AddClass("nav-category")
                           .StartTag("a")
                               .AddAttribute("href", "#group"+i.ToString())
                               .AddAttribute("onclick", "TabLoad('group" + i.ToString() + "', 'group/recommended/?group=" + group.Id.ToString()+"');")
                               .StartTag("strong")
                                   .AddContent(group.Name)
                               .EndTag()//strong
                           .EndTag()//a
                       .EndTag();

                    tabNode.InnerHtml += tagB.ToString();
                }
            }

            return new ResponseResult() { Data = Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml)};

        }

        // GET: my.ownet/recommend/list/recent?page={page}
        // GET: my.ownet/recommend/list/group/{group}?page={page}
        private static ResponseResult List(string method, string relativeUrl, RequestParameters parameters)
        {
            byte[] data = null;
            if (RequestParameters.IsNullOrEmpty(parameters))
            {
                data = ReplyToRecommended(relativeUrl);
            }
            else
            {
                data = ReplyToRecommended(relativeUrl, parameters.GetValue("page"));
            }
            return new ResponseResult() { Data = data };
        }

        private static byte[] ReplyToRecommended(string relativeUrl, string page = "1")
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            if (LocalHelpers.LoginHelper.IncludeLoginRequiredMessage(htmlDoc) == false)
            {
                htmlDoc.LoadHtml("<div id=\"include\"></div>");



                int pag = Convert.ToInt32(page);
                UserRecommendations recs;
                string group = System.Text.RegularExpressions.Regex.Replace(relativeUrl, "^list/", "");
                if (group.Equals("recent"))
                {
                    recs = ServiceCommunicator.ReceiveExplicitRecommendationsMostRecentWithVisitInformation();
                }
                else
                {
                    group = System.Text.RegularExpressions.Regex.Replace(group, "^group/", "");
                    recs = ServiceCommunicator.ReceiveExplicitRecommendationsInGroupWithVisitInformation(group, pag);
                }

                IncludeRecommendedPages(htmlDoc, recs);
                IncludePaging(htmlDoc, recs.TotalPages, recs.CurrentPage, group, "recommend/" + relativeUrl);
            }
            return Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml);
        }

        private static void IncludeRecommendedPages(HtmlDocument htmlDoc, UserRecommendations recs)
        {
            if (recs == null || !recs.Recommendations.Any())
            {
                IncludeNoResultsMessage(htmlDoc, "No page has been recommended yet.");
                return;
            }
            HtmlNode root = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"include\"]");
            if (root == null) return;
            HtmlNode resultsNode = htmlDoc.CreateElement("table");
            resultsNode.SetAttributeValue("class", "table_links");
            root.AppendChild(resultsNode);
            HtmlNode linkNode;

            string tabletd = "<td class=\"table_star\">"
                    + "	<ul class=\"star-rating tabsmargin\" id=\"star-rating-1\">"
                        + "	<li class=\"current-rating\" id=\"current-rating-1\" style=\"width: {0}%;\"></li>"
                        + "</ul>"
                    + "</td>"
                    + "<td class=\"table_link\">"
                        + "<a href=\"{1}\" target=\"_blank\" class=\"weblink{9}\" onclick=\"return useRecommendation({10});\">{2}</a><br />"
                        + "<small>{3}<small>"
                    + "</td>"
                    + "<td class=\"table_date\"><span class=\"activity_time\">{4} {5}</span><br />"
                    + "<small {7}>{6}</small></td>"
                    + ((Settings.IsTeacher()) ? "<td class=\"\" style=\"width: 20px;\"><a href=\"javascript:void(0);\" onclick=\"var that = this; confirm('Do you want to remove recommendation <strong>{2}</strong>?', function () {{ deleteRecommendation({10}, {11}, that); }});\"><img src=\"graphics/delete.png\" class=\"img-4\" alt=\"Remove recommendation\" title=\"Remove recommendation\"/></a></td>" : "");

            foreach (UserRecommendedPageWithRating rec in recs.Recommendations)
            {
                linkNode = htmlDoc.CreateElement("tr");

                 if (rec.IsNew)
                     linkNode.SetAttributeValue("style", "background-color : #fee2b4;");

                linkNode.InnerHtml = String.Format(tabletd,
                                Convert.ToInt32(rec.Page.AvgRating * 20),
                                rec.Page.AbsoluteURI,
                                (rec.Recommendation.Title.Length > 70 ? rec.Recommendation.Title.Substring(0, 65) + "&hellip;" : rec.Recommendation.Title),
                               (rec.Recommendation.Description.Length > 300 ? rec.Recommendation.Description.Substring(0, 295) + "&hellip;" : rec.Recommendation.Description),
                    //rec.User != null ? 
                                (rec.RecommendationTimeStamp.Date.Equals(DateTime.Today) ? "today" : ""),
                                rec.RecommendationTimeStamp.ToString("dd.MM.yyyy HH:mm"),// : "",
                    //rec.User != null ? 
                                //"Recommended by " + 
                                rec.User.Firstname + " " + rec.User.Surname + ".",
                                rec.User.IsTeacher ? "class=\"recomm_teacher\"" : "class=\"recomm_student\"",
                                HttpUtility.UrlEncode(rec.Page.AbsoluteURI),
                                (rec.Visited ? " visited" : ""),
                                rec.Page.Id,
                                rec.Recommendation.Group.Id);// : "Not yet recommeded.");

                resultsNode.AppendChild(linkNode);
            }
        }

        private static ResponseResult GetCountOfNotDisplayedRecomms(string method, string relativeUrl, RequestParameters parameters)
        {
            if (Settings.IsLoggedIn())
            {
                int count = ServiceCommunicator.ReceiveCountNotDisplayedRecomms();
                string rjson = "{ \"count\" : \"" + count.ToString() + "\"}";
                return SimpleOKResponse(rjson);
            }
            return SimpleOKResponse();
        }
    }
}
