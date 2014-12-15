using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ServiceEntities;
using System.Web;
using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.My.Live
{
    class LiveResponder : MyDomainResponder
    {
        protected override string ThisUrl  { get { return "live/"; } }


        public LiveResponder() : base()
        {
        }

        protected override void InitRoutes()
        {
            Routes
                 .RegisterRoute("GET", "", Index)
                 .RegisterRoute("GET", "index.html", Index)
                 .RegisterRoute("GET", "stream", Stream)
                 .RegisterRoute("POST", "send", Send);
        }


        // GET: my.ownet/live/
        // GET: my.ownet/live/index.html
        private static ResponseResult Index(string method, string relativeUrl, RequestParameters parameters)
        {
            return new ResponseResult() { Data = LocalHelpers.MenuHelper.ReplyToPageWithMenu("live.html") };
        }

        // GET: my.ownet/live/stream?last_activity={date}
        private static ResponseResult Stream(string method, string relativeUrl, RequestParameters parameters)
        {
            byte[] data = null;
            if (RequestParameters.IsNullOrEmpty(parameters)) {
                data = Stream();
            }
            else {
                data = Stream(parameters.GetValue("last_activity"));
            }
            return new ResponseResult() { Data = data, Filename = "stream.html" };
        }

        // POST: my.ownet/live/send
        private static ResponseResult Send(string method, string relativeUrl, RequestParameters parameters)
        {
            if (RequestParameters.IsNullOrEmpty(parameters))
            {
                return SimpleOKResponse("{ \"status\" : \"FAILED\" }");
            }
            SendMessage(parameters.GetValue("message"));
            return SimpleOKResponse();
        }

        internal static byte[] Stream()
        {
            return Stream(ServiceCommunicator.GetLiveStream());
        }

        internal static byte[] Stream(string since)
        {
            return Stream(ServiceCommunicator.GetLiveStream(since));
        }

        private static byte[] Stream(ActivityList activities)
        {
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            if (LocalHelpers.LoginHelper.IncludeLoginRequiredMessage(htmlDoc) == false)
            {
                htmlDoc.LoadHtml(LocalHelpers.ActivitiesHelper.ActivityRows(activities).ToString());
            }
            return Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml);
        }

        internal static void SendMessage(string message)
        {
            if (!Controller.UseServer || !Settings.IsLoggedIn()) return;
            System.Text.RegularExpressions.Regex rreg = new System.Text.RegularExpressions.Regex(@"[\r\n\t\'""]", System.Text.RegularExpressions.RegexOptions.Compiled);
            try
            {
                ServiceEntities.Activity activity = new ServiceEntities.Activity();
                activity.User.Id = Settings.UserID;
                activity.Type = ActivityType.Message;
                activity.Action = ActivityAction.Create;
                activity.DateTime = DateTime.Now;
                activity.Message = message;
                activity.Message = rreg.Replace(HttpUtility.UrlDecode(message), " ");

                ServerRequestManager.PostWithoutResponse<ServiceEntities.Activity>("activity/messages/create/", activity);
            }
            catch (Exception e)
            {
                ClientAndServerShared.LogsController.WriteException("SendMessage()", e.Message);
            }
        }
    }
}
