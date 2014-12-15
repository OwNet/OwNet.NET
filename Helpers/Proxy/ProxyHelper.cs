using System;
using System.IO;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace Helpers.Proxy
{
    public class ProxyHelper
    {
        public static string UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:9.0.1) Gecko/20100101 Firefox/9.0.1";

        public static HttpWebRequest CreateWebRequest(string url, string method)
        {
            HttpWebRequest webReq = null;
            try
            {
                webReq = (HttpWebRequest)HttpWebRequest.Create(url);
            }
            catch (Exception e)
            {
                throw e;
            }
            webReq.Method = method;
            webReq.ProtocolVersion = new Version(1, 0);

            webReq.Proxy = null;
            webReq.KeepAlive = false;
            webReq.AllowAutoRedirect = false;
            webReq.AutomaticDecompression = DecompressionMethods.None;

            return webReq;
        }

        public static Dictionary<string, string> CreateArtificialRequestHeaders()
        {
            return new Dictionary<string, string>()
                {
                    { "user-agent", UserAgent },
                    { "content-type", "*/*" },
                    { "accept", "*/*" }
                };
        }

        public static Tuple<HtmlDocument, Encoding> GetHtmlAgilityDocument(string filePath, string defaultEncoding = "")
        {
            try
            {
                return GetHtmlAgilityDocument(File.OpenRead(filePath), defaultEncoding);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Tuple<HtmlDocument, Encoding> GetHtmlAgilityDocument(byte[] bytes, string defaultEncoding = "")
        {
            return GetHtmlAgilityDocument(new MemoryStream(bytes), defaultEncoding);
        }

        public static Tuple<HtmlDocument, Encoding> GetHtmlAgilityDocument(Stream stream, string defaultEncodingStr = "")
        {
            HtmlDocument doc = new HtmlDocument();
            Encoding encoding = null;

            try
            {
                Encoding defaultEncoding = null;
                if (defaultEncodingStr != null && defaultEncodingStr != "")
                {
                    try
                    {
                        defaultEncoding = Encoding.GetEncoding(defaultEncodingStr);
                    }
                    catch (Exception) { }
                }
                if (defaultEncoding == null)
                    defaultEncoding = Encoding.UTF8;

                encoding = doc.DetectEncoding(stream) ?? defaultEncoding;
                stream.Seek(0, SeekOrigin.Begin);
                using (var sr = new StreamReader(stream, encoding))
                {
                    doc.LoadHtml(sr.ReadToEnd());
                }
            }
            catch (Exception)
            {
                return null;
            }

            return new Tuple<HtmlDocument,Encoding>(doc, encoding);
        }
    }
}
