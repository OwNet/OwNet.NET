using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;
using SharedProxy.Services;
using SharedProxy.Streams.Output;
using SharedProxy.Cache;

namespace SharedProxy.Proxy
{
    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Request")]
    public abstract class ProxyEntry : Helpers.Proxy.ProxyEntry
    {
        protected long _size = 0;
        protected bool _increasedAccessCount = false;
        protected int _numFileParts = 0;

        public long Size { get { return _size; } set { _size = value; } }
        public int NumFileParts { get { return _numFileParts; } set { _numFileParts = value; } }

        public static CacheEntrySaver Saver { get; set; }

        public ProxyEntry()
        { }

        public ProxyEntry(string RemoteUri)
        {
            AbsoluteUri = RemoteUri;
            RequestHeaders = Helpers.Proxy.ProxyHelper.CreateArtificialRequestHeaders();
            Method = "GET";
            DateStored = DateTime.Now;

            Init();
        }

        public ProxyEntry(ServiceEntities.Cache.CacheRequest request)
        {
            Update(request);

            Init();
        }

        public ProxyEntry(string absoluteUri, string method, Dictionary<string, string> requestHeaders)
        {
            AbsoluteUri = absoluteUri;
            Method = method;
            RequestHeaders = requestHeaders;
        }

        public void UpdateProxyServiceItem(ServiceItem item)
        {
            try
            {
                item.AbsoluteUri = AbsoluteUri;
                item.SetResponseHeaders(ResponseHeaders);
                item.Expires = Expires;
                item.StatusCode = (int)StatusCode;
                item.StatusDescription = StatusDescription;
                item.ContentType = ContentType;
                item.CharacterSet = CharacterSet;
                item.DateModified = DateModified;
            }
            catch (Exception ex)
            {
                Controller.WriteException("Update service item", ex.Message);
            }
        }

        public void Update(MainServerService.ServiceItem serviceItem)
        {
            ContentType = serviceItem.ContentType;
            Expires = serviceItem.Expires;
            DateModified = serviceItem.DateModified;
            StatusCode = (HttpStatusCode)serviceItem.StatusCode;
            StatusDescription = serviceItem.StatusDescription;
            CharacterSet = serviceItem.CharacterSet;
            DownloadedFrom = (ProxyEntry.DownloadMethods)serviceItem.DownloadMethod;
            ResponseHeaders = GetNoCacheItemResponseHeaders(serviceItem.ResponseHeaders);
        }

        private List<Tuple<String, String>> GetNoCacheItemResponseHeaders(string headersString)
        {
            List<Tuple<string, string>> headers = new List<Tuple<string, string>>();
            // Divide all pairs (remove empty strings)
            string[] tokens = headersString.Split(new string[] { "|||", "<->" },
                StringSplitOptions.RemoveEmptyEntries);
            // Walk through each item
            for (int i = 0; i + 1 < tokens.Length; i += 2)
            {
                // Fill the value in the sorted dictionary
                headers.Add(new Tuple<string, string>(tokens[i], tokens[i + 1]));
            }
            return headers;
        }

        protected double GetGDSFPriority(int accessCount)
        {
            return GetGDSFPriority(accessCount, Size);
        }

        public static double GetGDSFPriority(int accessCount, long size)
        {
            return GDSFClock.LastClock + accessCount * (100 / Math.Log(size + 1.1));
        }

        public bool ExistsOnDisk()
        {
            bool found = false;
            try
            {
                found = new System.IO.FileInfo(Controller.GetCacheFilePath(Helpers.Proxy.ProxyCache.GetUriHash(AbsoluteUri), 0)).Exists;
            }
            catch (Exception ex)
            {
                ClientAndServerShared.LogsController.WriteException("Find on disk", ex.Message);
            }
            return found;
        }

        public bool UpdateInDatabase(CacheWriter writer)
        {
            Saver.PlanSave(this, writer, !_increasedAccessCount);
            return true;
        }

        public abstract bool UpdateFromDatabase();

        internal void Update(ProxyEntry proxyEntry)
        {
            Update((ServiceEntities.Cache.CacheRequest)proxyEntry);
            ResponseHeaders = new List<Tuple<string,string>>(proxyEntry.ResponseHeaders);
            ContentType = proxyEntry.ContentType;
            Expires = proxyEntry.Expires;
            DateModified = proxyEntry.DateModified;
            StatusCode = proxyEntry.StatusCode;
            StatusDescription = proxyEntry.StatusDescription;
            CharacterSet = proxyEntry.CharacterSet;
            DownloadedFrom = proxyEntry.DownloadedFrom;
        }

        private void Update(ServiceEntities.Cache.CacheRequest request)
        {
            AbsoluteUri = request.AbsoluteUri;
            RequestHeaders = request.RequestHeaders;
            Refresh = request.Refresh;
            Method = request.Method;
            CanCache = request.CanCache;
            CanCacheHtml = request.CanCacheHtml;
            PostData = request.PostData;
        }
    }
}
