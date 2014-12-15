using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ServiceEntities;
using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.My.Tag
{
    class TagResponder : MyDomainResponder
    {
        protected override string ThisUrl { get { return "tag/"; } }
        public TagResponder() : base() 
        { }

        protected override void InitRoutes()
        {
            Routes.RegisterRoute("GET", "cloud", Cloud)
                .RegisterRoute("POST", "create", Create)
                .RegisterRoute("GET", "read", Read)
                .RegisterRoute("POST", "delete", Delete);
        }

        // GET: my.ownet/tag/cloud
        private static ResponseResult Cloud(string method, string relativeUrl, RequestParameters parameters)
        {
            return new ResponseResult() { Data = ReplyToTagCloud() };
        }

        // GET: my.ownet/tag/read?page={page}
        private static ResponseResult Read(string method, string relativeUrl, RequestParameters parameters)
        {
            string tpage = parameters.GetValue("page");
            List<string> tags = ServiceCommunicator.ReceiveTags(tpage);
            string tjson = ConvertListToJsonArray(tags);
            tjson = "{ \"tags\" : " + tjson + " }";
            return SimpleOKResponse(tjson);
        }

        // POST: my.ownet/tag/create
        private static ResponseResult Create(string method, string relativeUrl, RequestParameters parameters)
        {
            List<string> ptags = ServiceCommunicator.SendTags(parameters);
            string tpjson = ConvertListToJsonArray(ptags);
            tpjson = "{ \"tags\" : " + tpjson + " }";
            return SimpleOKResponse(tpjson);
        }

        // POST: my.ownet/tag/delete
        private static ResponseResult Delete(string method, string relativeUrl, RequestParameters parameters)
        {
            string dtag = parameters.GetValue("tag");
            bool deltag = ServiceCommunicator.DeleteTag(dtag);
            if (deltag)
                return SimpleOKResponse();
            return SimpleOKResponse(" { \"status\" : \"FAILED\" }");
        }


        private static  byte[] ReplyToTagCloud()
        {
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();


            TagCloud cloud = ServiceCommunicator.ReceiveTagCloud();
            if (cloud.Any())
            {
                htmlDoc.LoadHtml("<div id=\"tagcloud\"></div>");

                int maxCount = cloud.Max(t => t.Value);
                double sizeFactor = (100.0 / (double)maxCount);

                HtmlNode resultsNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"tagcloud\"]");
                resultsNode.RemoveAllChildren();
                HtmlNode spanNode = htmlDoc.CreateElement("span");
                string taga = "<a href=\"search/?query={0}&type=tags\" style=\"font-size: {1}%;\"><strong>{0}</strong></a>";
                if (Settings.UserTeacher)
                {
                    taga = " <span onmouseover=\"showDeleteTag(this);\" onmouseout=\"hideDeleteTag(this);\">" + taga + "<a style=\"display: none\" href=\"javascript:void(0);\" onclick=\"var that = this; confirm('Do you want to remove tag <strong>{0}</strong>?',function() {{deleteTag('{0}', that);}})\"><img src=\"graphics/delete.png\" class=\"img-4\" alt=\"Remove tag\" title=\"Remove tag\"/></a></span>";
                }
                else
                {
                    taga = " <span>" + taga + "</span>";
                }
                //" + Convert.ToString(size) + "
                foreach (string tag in cloud.Keys)
                {
                    int size = 100 + (int)(cloud[tag] * sizeFactor);
                    spanNode.InnerHtml += String.Format(taga, tag, size);
                }
                resultsNode.AppendChild(spanNode);
            }
            else
            {
                IncludeNoResultsMessage(htmlDoc, "No tags have been submitted yet.");
            }

            return Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml);

        }

    }
}
