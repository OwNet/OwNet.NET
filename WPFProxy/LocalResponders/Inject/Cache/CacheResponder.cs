using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClientAndServerShared;
using HtmlAgilityPack;
using WPFProxy.Cache;
using WPFProxy.Database;
using WPFProxy.Proxy;

namespace WPFProxy.LocalResponders.Inject.Cache
{
    class CacheResponder : InjectDomainResponder
    {
        protected override string ThisUrl { get { return "cache/"; } }
        public CacheResponder()
            : base()
        { }

        protected override void InitRoutes()
        {
            Routes
                .RegisterRoute("GET", "links", Links)
                .RegisterRoute("POST", "activate", Activate)
                .RegisterRoute("POST", "new_exception", NewException)
                .RegisterRoute("POST", "remove_exception", RemoveException);
        }

        // POST: inject.ownet/cache/activate
        private static ResponseResult Activate(string method, string relativeUrl, RequestParameters parameters)
        {
            return LocalResponders.My.Cache.CacheResponder.Activate(method, relativeUrl, parameters);
        }

        // GET: inject.ownet/cache/links?page={page}
        private static ResponseResult Links(string method, string relativeUrl, RequestParameters parameters)
        {
            List<string> links = GetCachedLinks(HttpUtility.UrlDecode(parameters.GetValue("page")), Controller.UseDataService);
            string linksArray = ConvertListToJsonArray(links);
            return SimpleOKResponse("owNetAVAILABLEURIS = " + linksArray, "js");
        }

        // gets all links available from the given uri (<a href=...> only)
        internal static Dictionary<int, string> GetLinks(string uri)
        {
            Dictionary<int, string> ret = new Dictionary<int, string>();
            try
            {
                if (!String.IsNullOrWhiteSpace(uri))
                {
                    uri = HttpUtility.UrlDecode(uri);
                    string filePath = SharedProxy.Cache.CacheFile.CacheFilePathAsOnePart(uri);
                    if (filePath == "")
                        return ret;

                    HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                    htmlDoc.Load(filePath);

                    if (htmlDoc.DocumentNode != null)
                    {
                        HtmlNodeCollection coll = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
                        Uri uriObject;
                        Uri baseUrl = new Uri(uri);

                        foreach (string url in (from s in coll
                                                select new { f = s.GetAttributeValue("href", "") } into g
                                                group g by g.f into y
                                                select y.Key).ToList())
                        {
                            uriObject = new Uri(baseUrl, url);
                            ret[ProxyCache.GetUriHash(uriObject.AbsoluteUri)] = uriObject.AbsoluteUri;
                        }
                    }
                }
            }
            catch {}

            return ret ?? new Dictionary<int, string>();
        }

        // selects cached links from the list of links
        internal static List<int> SelectCachedLinks(string uri, Dictionary<int, string> links, bool excludeSelf = false)
        {
            List<int> retLinks = new List<int>();

            if (links.Any())
            {
                DatabaseEntities database = Controller.GetDatabase();
                try
                {
                    List<int> linksList = links.Keys.ToList();
                    retLinks = database.Caches.Where(p => linksList.Contains(p.ID)).Select(p => p.ID).ToList();
                }
                catch (Exception e)
                {
                    LogsController.WriteException("GetCachedLinks()", e.Message);
                }
                finally
                {
                    database.Dispose();
                }
            }
            return retLinks;
        }

        // gets the links which are cached on client and server from the given uri (if wanted)
        private static List<string> GetCachedLinks(string uri, bool checkServer = false)
        {
            List<string> retLinks = new List<string>();
            List<int> foundIds = new List<int>();

            try
            {
                Dictionary<int, string> allLinks = GetLinks(uri);

                foundIds = SelectCachedLinks(uri, allLinks);

                if (checkServer)
                {
                    List<int> notFound = allLinks.Keys.Except(foundIds).ToList();

                    if (notFound.Any())
                    {
                        List<int> foundLinks = ServiceCommunicator.ReceiveCachedLinks(notFound);
                        foundIds.AddRange(foundLinks);
                    }
                }

                foreach (int id in foundIds)
                    retLinks.Add(allLinks[id]);
            }
            catch (Exception ex)
            {
                LogsController.WriteException("GetCachedLinks()", ex.Message);
            }

            return retLinks;
        }

        // POST: inject.ownet/cache/new_exception
        private static ResponseResult NewException(string method, string relativeUrl, RequestParameters parameters)
        {
            return LocalResponders.My.Cache.CacheResponder.NewException(method, relativeUrl, parameters);
        }

        // POST: inject.ownet/cache/remove_exception?id={exception_id}
        private static ResponseResult RemoveException(string method, string relativeUrl, RequestParameters parameters)
        {
            return LocalResponders.My.Cache.CacheResponder.RemoveException(method, relativeUrl, parameters);
        }
    }
}
