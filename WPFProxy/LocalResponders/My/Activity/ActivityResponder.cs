using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ServiceEntities;
using System.Web;
using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.My.Activity
{
    class ActivityResponder : MyDomainResponder
    {
        protected override string ThisUrl { get { return "activity/"; } }
        public ActivityResponder() : base()
        {
        }
        protected override void InitRoutes()
        {
            Routes//.RegisterRoute("GET", "", Index)
                //.RegisterRoute("GET", "index.html", Index)
                .RegisterRoute("POST", "create/.*", Create)
                .RegisterRoute("GET", "list", List)
                .RegisterRoute("POST", "delete", Delete);
        }

        
       

        // GET: my.ownet/activity/list?page={page}
        private static ResponseResult List(string method, string relativeUrl, RequestParameters parameters)
        {
            byte[] data = null;
            if (RequestParameters.IsNullOrEmpty(parameters))
            {
                data = ReplyToActivity();
            }
            else
            {
                data = ReplyToActivity(parameters.GetValue("page"));
            }
            return new ResponseResult() { Data = data };
        }

        private static byte[] ReplyToActivity(string page = null)
        {
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            int pag = 1;
            if (String.IsNullOrEmpty(page)) pag = 1;
            else { pag = Convert.ToInt32(page); }
            if (LocalHelpers.LoginHelper.IncludeLoginRequiredMessage(htmlDoc) == false)
            {
                htmlDoc.LoadHtml("<div id=\"include\"></div>");
                ActivityList recs = ServiceCommunicator.ReceiveActivityList(pag);
                LocalHelpers.ActivitiesHelper.IncludeActivities(htmlDoc, recs);
                IncludePaging(htmlDoc, recs.TotalPages, recs.CurrentPage, "activitystream", "activity/list");
//                htmlDoc.LoadHtml(LocalHelpers.ActivitiesHelper.ActivityRows(activities).ToString());
            }
            return Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml);

        }

        // POST: my.ownet/activity/create
        private static ResponseResult Create(string method, string relativeUrl, RequestParameters parameters)
        {
            string acttype = new System.Text.RegularExpressions.Regex("^create/", System.Text.RegularExpressions.RegexOptions.Compiled).Replace(relativeUrl, "");
            switch (acttype)
            {
                case "recommend":
                    ServiceCommunicator.SendUsageById(ActivityType.Recommend, parameters);
                    break;
                case "rating":
                    ServiceCommunicator.SendUsageById(ActivityType.Rating, parameters);
                    break;
                case "history":
                    ServiceCommunicator.SendUsageById(ActivityType.Visit, parameters);
                    break;
                case "search":
                    ServiceCommunicator.SendUsage(ActivityType.Search, parameters);
                    break;
                case "share":
                    ServiceCommunicator.SendUsage(ActivityType.Share, parameters);
                    break;
            }
            return SimpleOKResponse();
        }

        // POST: my.ownet/activity/delete
        private static ResponseResult Delete(string method, string relativeUrl, RequestParameters parameters)
        {
            string aid = parameters.GetValue("id");
            bool delact = ServiceCommunicator.DeleteActivity(aid);
            if (delact)
                return SimpleOKResponse();
            return SimpleOKResponse("{ \"status\" : \"FAILED\" }");
            
        }

       



    }
}
