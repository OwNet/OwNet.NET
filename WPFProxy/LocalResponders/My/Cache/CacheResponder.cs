using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ClientAndServerShared;
using WPFProxy.Proxy;
using WPFProxy.Cache;
using WPFProxy.Database;

namespace WPFProxy.LocalResponders.My.Cache
{
    class CacheResponder : MyDomainResponder
    {
        protected override string ThisUrl { get { return "cache/"; } }

        public CacheResponder() : base() 
        {
        }
        protected override void InitRoutes()
        {
            Routes
                .RegisterRoute("GET", "settings", Settings)
                .RegisterRoute("POST", "activate", Activate)
                .RegisterRoute("POST", "new_exception", NewException)
                .RegisterRoute("POST", "remove_exception", RemoveException);
        }

        // POST: my.ownet/cache/activate
        internal static ResponseResult Activate(string method, string relativeUrl, RequestParameters parameters)
        {
            string caching = parameters.GetValue("status");
            switch (caching)
            {
                case "on":
                    Controller.DoNotCache = false;
                    break;

                case "off":
                    Controller.DoNotCache = true;
                    break;
            }
            return SimpleOKResponse("", "js");
        }
       
        // GET: my.ownet/cache/settings
        private static ResponseResult Settings(string method, string relativeUrl, RequestParameters parameters)
        {
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.Load(Controller.GetAppResourcePath("Html/_caching.html"));

            try
            {
                DatabaseEntities database = Controller.GetDatabase();
                LocalHelpers.CacheHelper.SettingsInit(htmlDoc, database.Blacklist.OrderBy(b => b.Title).ToList());
            }
            catch (Exception ex)
            {
                LogsController.WriteException("Settings()", ex.Message);
            }

            return new ResponseResult() { Data = Encoding.GetEncoding("utf-8").GetBytes(htmlDoc.DocumentNode.OuterHtml) };
        }

        // POST: my.ownet/cache/new_exception
        internal static ResponseResult NewException(string method, string relativeUrl, RequestParameters parameters)
        {
            StringBuilder regexStr = new StringBuilder();
            regexStr.Append(@"^http://(?:www\.)?");

            string subdomain = System.Web.HttpUtility.UrlDecode(parameters.GetValue("subdomain_select"));
            string domain = System.Web.HttpUtility.UrlDecode(parameters.GetValue("domain_select"));
            string relative = System.Web.HttpUtility.UrlDecode(parameters.GetValue("relative_select"));
            string postrelative = System.Web.HttpUtility.UrlDecode(parameters.GetValue("postrelative_select"));

            // SUBDOMAIN
            if (subdomain != "")
            {
                if (subdomain == "*")
                    regexStr.Append(@"[A-Za-z0-9\-\._]*");
                else
                {
                    regexStr.Append(subdomain);
                    regexStr.Append(".");
                }
            }

            // DOMAIN
            if (domain == "*")
                regexStr.Append(@".*");
            else
                regexStr.Append(domain);

            // RELATIVE PATH
            if (relative == "*")
            {
                regexStr.Append(@"/?.*");

                // ENDING
                if (postrelative != "*" && postrelative != "")
                    regexStr.Append(postrelative);
            }
            else if (relative != "")
            {
                regexStr.Append(@"/?");
                regexStr.Append(relative);
                regexStr.Append(".");

                // ENDING
                if (postrelative == "*")
                    regexStr.Append(@"/?.*");
                else if (postrelative != "")
                {
                    regexStr.Append(@"/?");
                    regexStr.Append(postrelative);
                }
            }

            regexStr.Append(@"/?$");
            Blacklist item = CachingExceptions.AddException(regexStr.ToString(),
                parameters.GetValue("dont_cache_on_server") == "1",
                System.Web.HttpUtility.UrlDecode(parameters.GetValue("title")));
            
            return new ResponseResult()
            {
                Filename = "new_exception.html",
                Data = System.Text.Encoding.UTF8.GetBytes(item == null ? "" :
                    LocalHelpers.CacheHelper.ExeptionsTableRow(new TagBuilder(), item).ToString())
            };
        }

        // POST: my.ownet/cache/remove_exception?id={exception_id}
        internal static ResponseResult RemoveException(string method, string relativeUrl, RequestParameters parameters)
        {
            CachingExceptions.DeleteException(Convert.ToInt32(parameters.GetValue("id")));
            
            return SimpleOKResponse();
        }
    }
}
