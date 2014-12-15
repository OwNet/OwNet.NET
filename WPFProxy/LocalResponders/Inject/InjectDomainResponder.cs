using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ServiceEntities;
using System.Text.RegularExpressions;
using WPFProxy.Cache;
using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.Inject
{
    class InjectDomainResponder : LocalResponder
    {
        protected override string ThisUrl  { get { return "inject.ownet/"; } }


        public InjectDomainResponder() : base()
        {
        }

        protected override void InitRoutes()
        {
            Routes.RegisterRoute(null, "cache/.*", new Cache.CacheResponder().Respond)
                .RegisterRoute(null, "history/.*", new History.HistoryResponder().Respond)
                .RegisterRoute(null, "rating/.*", new Rating.RatingResponder().Respond)
                .RegisterRoute(null, "recommend/.*", new Recommendation.RecommendationResponder().Respond)
                .RegisterRoute(null, "refresh/.*", new Refresh.RefreshResponder().Respond)
                .RegisterRoute(null, "tag/.*", new Tag.TagResponder().Respond)
                .RegisterRoute(null, "prefetch/.*", new Prefetch.PrefetchResponder().Respond)
                .RegisterRoute("GET", "script.js", InjectScript)
                .RegisterRoute("POST", "user/.*", new User.UserResponder().Respond)
                .RegisterRoute("GET", "", Index)
                .RegisterRoute("GET", "index.html", Index)
                .RegisterRoute("GET", "caching.html", Caching);
            InitContentRoutes();
        }

        private static ResponseResult Index(string method, string relativeUrl, RequestParameters parameters)
        {
            if (Settings.IsLoggedIn())
            {
                HtmlDocument htmlDoc = LocalHelpers.FrameHelper.GetHtmlDocument("iframe.html", parameters.GetValue("parent"));

                HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"parent-uri\"]");
                string url = System.Web.HttpUtility.UrlDecode(parameters.GetValue("parent"));

                node.SetAttributeValue("value", parameters.GetValue("parent"));
                node = htmlDoc.DocumentNode.SelectSingleNode("//a[@id=\"login-username\"]");
                node.InnerHtml = Settings.UserFirstname + " " + Settings.UserSurname;
                if (Settings.UserTeacher)
                {
                    node.InnerHtml += " (teacher)";
                }
                else
                {
                    node.InnerHtml += " (student)";
                }

               node = htmlDoc.DocumentNode.SelectSingleNode("//select[@id=\"rselect-group\"]");


               GroupsList groups = ServiceCommunicator.GetUserGroupsList();

               TagBuilder tag = new TagBuilder();

               foreach (ServiceEntities.Group group in groups)
               {

                   tag = tag.StartTag("option")
                            .AddAttribute("value",group.Name)
                            .AddContent(group.Name)
                        .EndTag();
               }

                node.InnerHtml += tag.ToString();

                List<ServiceEntities.Recommendation> previousRecommendations = ServiceCommunicator.GetPreviousRecommendationsForPage(url);

                if (previousRecommendations.Any())
                {
                    tag = new TagBuilder();
                    foreach (var recommendation in previousRecommendations)
                    {
                        tag = tag
                                .StartTag("tr")
                                    .StartTag("td")
                                        .AddContent(recommendation.Title)
                                    .EndTag() // td
                                    .StartTag("td")
                                        .AddContent(recommendation.Group.Name)
                                    .EndTag() // td
                                .EndTag() // tr
                                ;
                    }

                    node = htmlDoc.DocumentNode.SelectSingleNode("//tbody[@id=\"previousrecommendations_tbody\"]");
                    node.InnerHtml = tag.ToString();
                }

                return new ResponseResult() { Filename = "index.html", Data = Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml) };
            }
            return LocalHelpers.FrameHelper.FrameLogin();
        }

        private static ResponseResult InjectScript(string method, string relativeUrl, RequestParameters parameters)
        {
            return LoadProxyLocalFile("js/tabframeinject.js");
        }


        internal static ResponseResult Caching(string method, string relativeUrl, RequestParameters parameters)
        {
            if (Settings.IsLoggedIn())
            {
                HtmlDocument htmlDoc = LocalHelpers.HtmlHelper.HtmlDocumentWithLayout("_caching.html", LocalResponders.LocalResponder.LocalLayout.Frame);
                HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//input[@id=\"parent-uri\"]");
                node.SetAttributeValue("value", parameters.GetValue("parent"));

                string url = System.Web.HttpUtility.UrlDecode(parameters.GetValue("parent"));

                Regex domainRegex = new Regex(@"http://(?:[A-Za-z0-9\-\._]*\.)?([A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+)");
                if (domainRegex.IsMatch(url))
                {
                    HtmlNode optionNode = htmlDoc.CreateElement("option");
                    optionNode.InnerHtml = domainRegex.Match(url).Groups[1].Value;
                    optionNode.SetAttributeValue("selected", "selected");
                    htmlDoc.DocumentNode.SelectSingleNode("//select[@name='domain_select']").AppendChild(optionNode);
                }

                htmlDoc.DocumentNode.SelectSingleNode("//input[@name='parent_url']").SetAttributeValue("value", url);
                htmlDoc.DocumentNode.SelectSingleNode("//input[@name='title']").SetAttributeValue("value",
                    HtmlResponder.GetWebsiteTitleAndDescription(url).Item1);
                htmlDoc.DocumentNode.SelectSingleNode("//div[@id='user-info']").InnerHtml = new TagBuilder()
                    .StartTag("span")
                        .StartTag("a")
                            .AddAttribute("href", "javascript:void(0);")
                            .AddAttribute("id", "login-username")
                            .AddContent(Settings.UserFirstname + " " + Settings.UserSurname +
                                (Settings.UserTeacher ? " (teacher)" : " (student)"))
                        .EndTag() // a
                        .AddContent(" | ")
                        .StartTag("a")
                            .AddAttribute("href", "javascript:void(0);")
                            .AddAttribute("onclick", "logout(); return false;")
                            .AddContent("Logout")
                        .EndTag() // a
                        .AddContent(" »")
                    .EndTag() //span
                    .ToString();

                LocalHelpers.CacheHelper.SettingsInit(htmlDoc, CachingExceptions.MatchingExpressions(url));

                return new ResponseResult() { Filename = "caching.html", Data = Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml) };
            }
            return LocalHelpers.FrameHelper.FrameLogin();
        }
    }
}
