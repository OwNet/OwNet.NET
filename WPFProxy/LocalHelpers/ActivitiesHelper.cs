using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ServiceEntities;
using System.Web;
using WPFProxy.Proxy;

namespace WPFProxy.LocalHelpers
{
    class ActivitiesHelper
    {
        internal static void IncludeActivities(HtmlDocument htmlDoc, ActivityList activities)
        {
            HtmlNode root = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"include\"]");
            if (root == null) return;
            HtmlNode resultsNode = htmlDoc.CreateElement("table");
            resultsNode.SetAttributeValue("class", "table_links");
            root.AppendChild(resultsNode);
            resultsNode.InnerHtml = ActivityRows(activities).ToString();
        }
        
        internal static TagBuilder ActivityRows(ActivityList activities, bool isLiveStream = false)
        {
            string act, other, acttext, url, link;
            TagBuilder tag = new TagBuilder();

            foreach (Activity newsItem in activities.Activities)
            {
                act = "";
                other = "";
                acttext = "";
                url = "";
                link = "";
                switch (newsItem.Type)
                {
                    case ActivityType.Rating:
                        act = "rated";
                        if (newsItem.Action == ActivityAction.Create || newsItem.Action == ActivityAction.Update)
                            acttext = "rated";
                        else
                            continue;
                        other = " with ";
                        break;
                    case ActivityType.Recommend:
                        act = "recommended";
                        if (newsItem.Action == ActivityAction.Create)
                        {
                            acttext = "recommended";
                            other = "in";
                        }
                        else if (newsItem.Action == ActivityAction.Update)
                        {
                            acttext = "updated recommendation for";
                            other = "in";
                        }
                        else if (newsItem.Action == ActivityAction.Delete)
                        {
                            acttext = "'s recommendation for";
                            other = "was removed";
                        }
                        else
                            continue;
                        break;
                    case ActivityType.Tag:
                        if (newsItem.Action == ActivityAction.Create)
                        {
                            acttext = act = "tagged"; other = "with tags";
                        }
                        else
                            continue;
                        break;
                    case ActivityType.Register:
                        if (newsItem.Action == ActivityAction.Create)
                        {
                            acttext = "";
                            act = "register";
                            other = "registered as a";
                        }
                        else continue;
                        break;
                    case ActivityType.Share:
                        if (newsItem.Action == ActivityAction.Create)
                        {
                            act = "shared";
                            acttext = "shared file"; 
                            other = "in folder";
                        }
                        else continue;
                        break;
                    case ActivityType.Message:
                        acttext = "said";
                        act = "live";
                        break;
                    case ActivityType.Group :
                        if (newsItem.Action == ActivityAction.Create)
                        {
                            acttext = "created group";
                            act = "group";
                            other = "";
                        }
                        else if (newsItem.Action == ActivityAction.Update)
                        {
                            acttext = "joined group";
                            act = "group";
                            other = "";
                        }
                        else continue;
                        break;
                    default:
                        continue;

                }
                if (newsItem.Type == ActivityType.Share)
                {
                    string title = HttpUtility.HtmlEncode(newsItem.File.Title);
                    url = HttpLocalResponder.GetSharedFilePath(newsItem.File);
                    link = (newsItem.File.Title.Length > 40 ? title.Substring(0, 35) + "&hellip;" : title);
                }
                else
                {
                    string title = HttpUtility.HtmlEncode(newsItem.Page.Title);
                    url = (String.IsNullOrWhiteSpace(newsItem.Page.AbsoluteURI) ? "#" : newsItem.Page.AbsoluteURI);
                    link = (newsItem.Page.Title.Length > 35 ?
                        (isLiveStream ? title : title.Substring(0, 30) + "&hellip;")
                        : title);
                }
                tag = tag.StartTag("tr");
                tag =
                    tag.StartTag("td")
                        .AddAttribute("class", "table_activity")
                        .StartTag("span")
                            .AddAttribute("class", "activity_" + act)
                            .StartTag("a")
                                .AddAttribute("class", newsItem.User.IsTeacher ? "class=\"teacher\"" : "")
                                .AddAttribute("href", "user/")
                                .AddEncContent(newsItem.User.Firstname + " " + newsItem.User.Surname)
                            .EndTag() // a
                            .AddContentFormat(" {0} ", acttext);

                if (newsItem.Type == ActivityType.Message)
                {
                    tag = tag
                            .StartTag("span")
                                .AddClass("chat_live")
                                .AddContent(LocalHelpers.ActivitiesHelper.NlToBr(HttpUtility.HtmlEncode(newsItem.Message)))
                            .EndTag(); // span
                }
                else
                {
                    tag = tag
                            .StartTag("a")
                                .AddAttribute("href", url)
                                .AddAttribute("target", "_blank")
                                .AddAttribute("class", "weblink")
                                .AddContent(link)
                            .EndTag() // a
                            .AddContentFormat(" {0} <strong>{1}</strong>.", other, LocalHelpers.ActivitiesHelper.NlToBr(HttpUtility.HtmlEncode(newsItem.Message)));
                }

                tag = tag
                        .EndTag() // span
                    .EndTag() // td
                    .StartTag("td")
                        .AddAttribute("class", "table_date")
                        .StartTag("span")
                            .AddAttribute("class", "activity_time")
                            .AddContentFormat("{0} {1}", (newsItem.DateTime.Date.Equals(DateTime.Today) ? "today" : ""),
                                newsItem.DateTime.ToString("dd.MM.yyyy HH:mm"))
                        .EndTag() // span
                        .StartTag("span")
                            .AddAttribute("class", "full_activity_time")
                            .AddAttribute("style", "display:none;")
                            .AddContent(HttpUtility.UrlEncode(Helpers.Common.FullDateTimeToString(newsItem.DateTime)))
                        .EndTag() // span
                    .EndTag(); //td

                if (!isLiveStream && Settings.IsTeacher())
                    tag =
                        tag.StartTag("td")
                            .AddAttribute("style", "width: 20px;")
                            .StartTag("a")
                                .AddAttribute("href", "javascript:void(0);")
                                .AddAttribute("onclick", String.Format("var that = this; confirm('Do you want to hide activity from {0} {1}?', function () {{ deleteActivity({2}, that); }});",
                                        HttpUtility.HtmlEncode(newsItem.User.Firstname),
                                        HttpUtility.HtmlEncode(newsItem.User.Surname),
                                        newsItem.ActId))
                                .StartTag("img")
                                    .AddAttribute("src", "graphics/delete.png")
                                    .AddAttribute("class", "img-4")
                                    .AddAttribute("alt", "Remove activity")
                                    .AddAttribute("title", "Remove activity")
                                .EndTag() // img
                            .EndTag() // a
                        .EndTag(); // td
                tag = tag.EndTag(); // tr
            }
            return tag;
        }

        internal static string NlToBr(string text)
        {
            return text.Replace("\n", "<br/>");
        }
    }
}
