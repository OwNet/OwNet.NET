using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using ClientAndServerShared;
using HtmlAgilityPack;
using WPFProxy.Cache;

namespace WPFProxy.Proxy
{
    public class HtmlResponder
    {
        static Regex _htmlRegex = new Regex("<.*?>", RegexOptions.Compiled);
        static Dictionary<int, HtmlInfo> _visitedHtmlInfos = new Dictionary<int, HtmlInfo>();

        private bool _shouldUpdateHeaders = false;
        private int _responseBytesLength = 0;
        private byte[] _bytes = null;

        public ProxyEntry CacheEntry;

        public HtmlResponder(byte[] bytes)
        {
            _bytes = bytes;
        }

        public byte[] GetResponseBytes()
        {
            if (CacheEntry.IsInFrame)
                return _bytes;

            try
            {
                Tuple<HtmlDocument, Encoding> htmlDocAndEncoding = Helpers.Proxy.ProxyHelper.GetHtmlAgilityDocument(_bytes, CacheEntry.CharacterSet);
                if (htmlDocAndEncoding == null)
                    return _bytes;
                
                HtmlDocument htmlDoc = htmlDocAndEncoding.Item1;
                if (htmlDoc == null || htmlDoc.DocumentNode == null)
                    return _bytes;

                HtmlNode bodyDocNode = htmlDoc.DocumentNode.SelectSingleNode("/html/body");
               
                if (bodyDocNode != null)
                {
                    HtmlNode frameDiv = htmlDoc.CreateElement("div");
                    HtmlNode scriptTagInject = htmlDoc.CreateElement("script");

                    scriptTagInject.SetAttributeValue("type", "text/javascript");
                    scriptTagInject.SetAttributeValue("src", "http://inject.ownet/script.js?id=" + Convert.ToString(new Random().Next(1000)));

                    frameDiv.SetAttributeValue("id", "owNet-div");
                    bodyDocNode.ChildNodes.Prepend(frameDiv);
                    frameDiv.ChildNodes.Append(scriptTagInject);
                }
                _bytes = htmlDocAndEncoding.Item2.GetBytes(htmlDoc.DocumentNode.OuterHtml);

                _shouldUpdateHeaders = true;
                _responseBytesLength = _bytes.Length;
                
                CreateHtmlInfo(CacheEntry.ID, htmlDoc);
            }
            catch (Exception ex)
            {
                LogsController.WriteException("GetResponseBytes()", ex.Message);
            }

            return _bytes;
        }

        public List<Tuple<string, string>> GetHeaders()
        {
            if (_shouldUpdateHeaders)
            {
                List<Tuple<string, string>> headers = new List<Tuple<string,string>>();

                foreach (Tuple<string, string> header in CacheEntry.ResponseHeaders)
                {
                    string item2;

                    if (header.Item1.ToLower().Contains("content-length"))
                        item2 = Convert.ToString(_responseBytesLength);
                    else
                        item2 = header.Item2;

                    headers.Add(new Tuple<string, string>(header.Item1, item2));
                }

                return headers;
            }

            return CacheEntry.ResponseHeaders;
        }

        public static Tuple<string, string> GetWebsiteTitleAndDescription(string url)
        {
            int hash = ProxyCache.GetUriHash(url);
            if (_visitedHtmlInfos.ContainsKey(hash))
            {
                HtmlInfo info = _visitedHtmlInfos[hash];
                return new Tuple<string, string>(info.Title, info.Description);
            }

            try
            {
                string filePath = SharedProxy.Cache.CacheFile.CacheFilePathAsOnePart(url);
                if (filePath != "")
                {
                    HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                    htmlDoc.Load(filePath);

                    if (htmlDoc.DocumentNode != null)
                    {
                        HtmlInfo info = CreateHtmlInfo(hash, htmlDoc);
                        return new Tuple<string, string>(info.Title, info.Description);
                    }
                }
            }
            catch (Exception)
            {
            }
            return new Tuple<string, string>(new Regex(@"^.{0,5}://").Replace(url, ""), "");
        }

        private static HtmlInfo CreateHtmlInfo(int hash, HtmlDocument htmlDoc)
        {
            if (_visitedHtmlInfos.Count > 100)
                _visitedHtmlInfos.Clear();

            HtmlNode titleNode = htmlDoc.DocumentNode.SelectSingleNode("//title");
            HtmlNode descriptionNode = null;
            if (htmlDoc.DocumentNode.Descendants("meta").Any())
            {
                descriptionNode = descriptionNode = htmlDoc.DocumentNode.Descendants("meta")
                 .Where(m => m.Attributes["name"] != null)
                 .Where(m => m.Attributes["name"].Value.Equals("description", StringComparison.OrdinalIgnoreCase))
                     .FirstOrDefault();
                
            }
            HtmlInfo info = new HtmlInfo()
            {
                Title = titleNode == null ? "" : titleNode.InnerText,
                Description = descriptionNode == null ? "" : descriptionNode.GetAttributeValue("content", "")
            };
            _visitedHtmlInfos[hash] = info;

            return info;
        }
    }

    class HtmlInfo
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
