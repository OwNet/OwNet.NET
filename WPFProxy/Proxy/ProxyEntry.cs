using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using ClientAndServerShared;
using WPFProxy.Database;

namespace WPFProxy.Proxy
{
    [DataContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/", Name = "Request")]
    public class ProxyEntry : SharedProxy.Proxy.ProxyEntry
    {
        public bool WasOutput = false;

        public ProxyEntry(string RemoteUri) : base(RemoteUri)
        { }

        public ProxyEntry(ServiceEntities.Cache.CacheRequest request)
            : base(request)
        { }

        public ProxyEntry(string RemoteUri, System.IO.StreamReader clientStreamReader, string method)
        {
            AbsoluteUri = RemoteUri;
            Method = method;
            RequestHeaders = ProxyServer.ReadRequestHeaders(clientStreamReader);
            if (IsPost)
            {
                _contentLength = ProxyServer.GetContentLengthFromHeaders(RequestHeaders);
                PostData = HttpResponder.ReadPostData(_contentLength, clientStreamReader);
            }
            DateStored = DateTime.Now;

            Init();
        }

        public override bool UpdateFromDatabase()
        {
            bool found = false;
            DatabaseEntities context = null;
            try
            {
                context = Controller.GetDatabase();

                var caches = from p in context.Caches
                             where p.ID == ID
                             select p;

                if (caches.Any())
                {
                    Caches cache = caches.First();

                    if (RefreshIfOlderThan != null && cache.DateModified < RefreshIfOlderThan)
                    {
                        found = false;
                    }
                    else
                    {
                        ID = cache.ID;
                        Expires = cache.Expires;
                        DateStored = cache.DateStored ?? DateTime.Now;
                        DateModified = cache.DateModified ?? DateStored;
                        StatusDescription = cache.StatusDescription;
                        StatusCode = (HttpStatusCode)(cache.StatusCode ?? (int)HttpStatusCode.OK);
                        CharacterSet = cache.CharacterSet;
                        ContentType = cache.ContentType;
                        Size = cache.Size ?? 0;
                        NumFileParts = cache.Parts ?? 0;

                        var cacheHeaders = from p in context.CacheHeaders
                                           where p.CacheId == cache.ID
                                           select p;

                        foreach (var cacheHeader in cacheHeaders)
                        {
                            AddResponseHeader(cacheHeader.Key, cacheHeader.Value);
                        }

                        found = ExistsOnDisk();

                        if (found)
                        {
                            Saver.IncreaseAccessCount(ID, Size);
                            _increasedAccessCount = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException("Find cache", ex.Message);
            }
            finally
            {
                if (context != null)
                    context.Dispose();
            }
            return found;
        }

        public void Delete()
        {
            DatabaseEntities context = null;
            try
            {
                context = Controller.GetDatabase();
                var cache = context.Caches.First(c =>
                            c.ID == ID);

                context.Caches.DeleteObject(cache);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                LogsController.WriteException("DeleteCacheEntry", ex.Message);
            }
            finally
            {
                context.Dispose();
            }
        }
        
        public bool AcceptsHtml()
        {
            if (RequestHeaders == null)
                return false;

            foreach (KeyValuePair<String, String> header in RequestHeaders)
            {
                if (header.Key.ToLower() == "accept")
                {
                    if (header.Value.ToLower().Contains("text/html"))
                        return true;
                    else
                        return false;
                }
            }

            return false;
        }
    }
}
