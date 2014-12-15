using System;
using System.Text;
using HtmlAgilityPack;

namespace WPFProxy
{
    public class SearchDatabase : ClientAndServerShared.SearchDatabase
    {
        public class VirtualSearchDatabase : IVirtualSearchDatabase
        {
            public HtmlDocument GetHtmlDocument(string url)
            {
                string filePath = SharedProxy.Cache.CacheFile.CacheFilePathAsOnePart(url);
                if (filePath == "")
                    return null;

                if (!System.IO.File.Exists(filePath))
                    return null;

                Tuple<HtmlDocument, Encoding> htmlDocAndEncoding = Helpers.Proxy.ProxyHelper.GetHtmlAgilityDocument(filePath);

                if (htmlDocAndEncoding == null)
                    return null;
                return htmlDocAndEncoding.Item1;
            }
        }
    }
}
