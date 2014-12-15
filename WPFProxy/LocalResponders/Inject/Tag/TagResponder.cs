using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ServiceEntities;
using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.Inject.Tag
{
    class TagResponder : InjectDomainResponder
    {
        protected override string ThisUrl { get { return "tag/"; } }
        public TagResponder()
            : base()
        { }

        protected override void InitRoutes()
        {
            Routes
                .RegisterRoute("POST", "create", Create)
                .RegisterRoute("GET", "read", Read);
               
            
        }

        // GET: inject.ownet/tag/read?page={page}
        private static ResponseResult Read(string method, string relativeUrl, RequestParameters parameters)
        {
            string tpage = parameters.GetValue("page");
            List<string> tags = ServiceCommunicator.ReceiveTags(tpage);
            string tjson = ConvertListToJsonArray(tags);
            tjson = "{ \"tags\" : " + tjson + " }";
            return SimpleOKResponse(tjson);
        }

        // POST: inject.ownet/tag/create
        private static ResponseResult Create(string method, string relativeUrl, RequestParameters parameters)
        {
            List<string> ptags = ServiceCommunicator.SendTags(parameters);
            string tpjson = ConvertListToJsonArray(ptags);
            tpjson = "{ \"tags\" : " + tpjson + " }";
            return SimpleOKResponse(tpjson);
        }

    }
}
