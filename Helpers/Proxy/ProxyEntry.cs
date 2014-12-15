using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;

namespace Helpers.Proxy
{
    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Request")]
    public abstract class ProxyEntry : ServiceEntities.Cache.CacheRequest
    {
        private string _characterSet = "";
        private int _id = 0;
        protected int _contentLength = 0;

        public DownloadMethods DownloadedFrom = DownloadMethods.FromCacheOverLocalProxy;
        public bool IsOffline = false;

        public int ID
        {
            get
            {
                if (_id == 0)
                    _id = ProxyCache.GetUriHash(AbsoluteUri);
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public HttpWebRequest WebRequest { get; set; }
        public string UserAgent { get; set; }

        public DateTime? Expires { get; set; }
        public DateTime DateStored { get; set; }
        public DateTime DateModified { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public String StatusDescription { get; set; }
        public Boolean FlagRemove { get; set; }
        public List<Tuple<string, string>> ResponseHeaders { get; set; }
        public string ContentType { get; set; }
        public int ContentLength { get { return _contentLength; } }
        public bool IsInFrame = false;
        public bool ShouldSave = true;
        public bool IsPost { get { return Method.ToUpper() == "POST"; } }
        public HttpTypes Type { get { return IsPost ? HttpTypes.POST : HttpTypes.GET; } }
        public override int CanCache { get { return IsPost ? 0 : base.CanCache; } }
        public override bool CanCacheHtml { get { return base.CanCacheHtml && CanCache > 0; } }

        public bool UseServer = false;

        protected virtual void Init()
        {
            ID = ProxyCache.GetUriHash(AbsoluteUri);
            DateModified = DateTime.Now;

            WebRequest = ProxyHelper.CreateWebRequest(AbsoluteUri, Method);
            ReadRequestHeaders();

            WebRequest.Timeout = 15000;
            if (PostData != null)
                HttpGetRetriever.WritePostDataToWebRequest(PostData, WebRequest);

            RefreshIfOlderThan = null;
        }
        
        public string CharacterSet
        {
            get { return _characterSet; }
            set
            {
                if (value == "win1250")      // vyhadzovalo exception "win-1250 is not encoding name"
                    value = "windows-1250";  // fix podla http://msdn.microsoft.com/en-us/library/system.text.encoding.aspx
                _characterSet = value;
            }
        }

        public enum DownloadMethodElements
        {
            SavedToCache = 1,
            FromInternet = 2,
            FromCache = 4,
            OverLocalProxy = 8,
            OverServer = 16,
            RefreshedCache = 32,
            AccessedByUser = 64,
            RemovedFromCache = 128
        }

        public enum CanCacheOptions
        {
            CantCache = 0,
            CanCacheOnClient = 1,
            CanCacheOnServer = 2
        }

        public enum DownloadMethods
        {
            FromInternetOverServerWithoutCaching = DownloadMethodElements.FromInternet
                + DownloadMethodElements.OverServer
                + DownloadMethodElements.AccessedByUser,
            FromInternetOverLocalProxyWithoutCaching = DownloadMethodElements.FromInternet
                + DownloadMethodElements.OverLocalProxy
                + DownloadMethodElements.AccessedByUser,
            FromInternetOverLocalProxy = DownloadMethodElements.FromInternet
                + DownloadMethodElements.OverLocalProxy
                + DownloadMethodElements.SavedToCache
                + DownloadMethodElements.AccessedByUser,
            FromCacheOverLocalProxy = DownloadMethodElements.FromCache
                + DownloadMethodElements.OverLocalProxy
                + DownloadMethodElements.AccessedByUser,
            FromInternetOverServer = DownloadMethodElements.FromInternet
                + DownloadMethodElements.OverServer
                + DownloadMethodElements.SavedToCache
                + DownloadMethodElements.AccessedByUser,
            FromCacheOverServer = DownloadMethodElements.FromCache
                + DownloadMethodElements.OverServer
                + DownloadMethodElements.AccessedByUser,
            RefreshCacheOnServer = DownloadMethodElements.RefreshedCache
                + DownloadMethodElements.OverServer
                + DownloadMethodElements.FromInternet
                + DownloadMethodElements.SavedToCache,
            FromInternetOverServerRefreshCache = DownloadMethodElements.RefreshedCache
                + DownloadMethodElements.OverServer
                + DownloadMethodElements.FromInternet
                + DownloadMethodElements.AccessedByUser
                + DownloadMethodElements.SavedToCache
        }

        public enum HttpTypes
        {
            GET = 1,
            POST = 2,
            PUT = 4,
            DELETE = 8,
            DELETEFROMCACHE = 16
        }

        private void ReadRequestHeaders()
        {
            if (RequestHeaders == null)
                return;

            foreach (var pair in RequestHeaders)
            {
                switch (pair.Key)
                {
                    case "host":
                        WebRequest.Host = pair.Value;
                        break;
                    case "user-agent":
                        WebRequest.UserAgent = pair.Value;
                        UserAgent = pair.Value;
                        break;
                    case "accept":
                        WebRequest.Accept = pair.Value;
                        break;
                    case "referer":
                        WebRequest.Referer = pair.Value;
                        break;
                    case "cookie":
                        WebRequest.Headers["Cookie"] = pair.Value;
                        break;
                    case "proxy-connection":
                    case "connection":
                    case "keep-alive":
                    case "accept-encoding":
                    case "expect":
                        //ignore these
                        break;
                    case "content-length":
                        int.TryParse(pair.Value, out _contentLength);
                        break;
                    case "content-type":
                        WebRequest.ContentType = pair.Value;
                        break;
                    case "if-modified-since":
                        String[] sb = pair.Value.Trim().Split(ProxyServer.semiSplit);
                        DateTime d;
                        if (DateTime.TryParse(sb[0], out d))
                            WebRequest.IfModifiedSince = d;
                        break;
                    default:
                        try
                        {
                            WebRequest.Headers.Add(pair.Key, pair.Value);
                        }
                        catch (Exception ex)
                        {
                            Messages.WriteException("Read request headers", String.Format("Could not add header {0}. Exception message:{1}", pair.Key, ex.Message));
                        }
                        break;
                }
            }
        }

        public void Update(HttpWebResponse response)
        {
            DateTime? expires = null;
            ResponseHeaders = ProxyServer.ProcessResponse(response);
            StatusCode = response.StatusCode;
            StatusDescription = response.StatusDescription;
            ProxyCache.CanCache(response.Headers, ref expires);
            Expires = expires;
            ContentType = response.ContentType;
            CharacterSet = response.CharacterSet;
        }

        public void AddResponseHeader(String key, String value)
        {
            if (ResponseHeaders == null)
                ResponseHeaders = new List<Tuple<string, string>>();

            ResponseHeaders.Add(new Tuple<String, String>(key, value));
        }

        public bool IsHTMLDocument()
        {
            if (ResponseHeaders == null)
                return false;

            foreach (Tuple<String, String> header in ResponseHeaders)
            {
                if (header.Item1.ToLower() == "content-type")
                {
                    if (header.Item2.ToLower().Contains("text/html"))
                        return true;
                    else
                        return false;
                }
            }

            return !ProxyServer.HasNonHtmlExtensions(AbsoluteUri);
        }
    }
}
