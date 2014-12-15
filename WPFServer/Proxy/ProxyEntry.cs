using System;
using System.Linq;
using System.Collections.Generic;
using ServiceEntities.Cache;
using System.Net;
using ClientAndServerShared;

namespace WPFServer.Proxy
{
    public class ProxyEntry : SharedProxy.Proxy.ProxyEntry
    {
        public ProxyEntry(string absoluteUri, string method, Dictionary<string, string> requestHeaders)
            : base(absoluteUri, method, requestHeaders)
        { }

        public ProxyEntry(CacheRequest request)
            : base(request)
        { }

        public ProxyEntry(string RemoteUri)
            : base(RemoteUri)
        { }

        public static void Delete(int id)
        {
            try
            {
                var context = new DatabaseContext.MyDBContext();

                var caches = context.Fetch<DatabaseContext.Cache.Cache>(c => c.Id == id);
                if (caches.Any())
                {
                    var cache = caches.First();
                    for (int i = 0; i < cache.Parts; ++i)
                    {
                        System.IO.FileInfo file = new System.IO.FileInfo(SharedProxy.Controller.GetCacheFilePath(id, i));
                        if (file.Exists)
                            file.Delete();
                    }
                    context.Remove<DatabaseContext.Cache.Cache>(cache);
                }
            }
            catch (Exception e)
            {
                LogsController.WriteException("Delete cache", e.Message);
            }
        }

        internal void Update(DatabaseContext.Cache.Cache cache)
        {
            Expires = cache.Expires;
            StatusCode = (System.Net.HttpStatusCode)cache.StatusCode;
            StatusDescription = cache.StatusDescription;
            ContentType = cache.ContentType;
            //_contentLength = (int)cache.ContentLength;
            CharacterSet = cache.CharacterSet;
            UserAgent = cache.UserAgent;
            DateStored = cache.DateCreated;
            DateModified = cache.DateModified;
            ID = cache.Id;
            NumFileParts = cache.Parts;

            foreach (DatabaseContext.Cache.CacheHeader cacheHeader in cache.CacheHeaders)
            {
                AddResponseHeader(cacheHeader.Key, cacheHeader.Value);
            }
        }

        public override bool UpdateFromDatabase()
        {
            bool found = false;
            var context = new DatabaseContext.MyDBContext();
            try
            {
                var caches = context.Fetch<DatabaseContext.Cache.Cache>(c => c.Id == ID);

                if (caches.Any())
                {
                    DatabaseContext.Cache.Cache cache = caches.First();

                    if (RefreshIfOlderThan != null && cache.DateModified < RefreshIfOlderThan)
                    {
                        found = false;
                    }
                    else
                    {
                        ID = cache.Id;
                        Update(cache);
                        Size = cache.Size;

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
                ClientAndServerShared.LogsController.WriteException("Find cache", ex.Message);
            }
            finally
            {
                if (context != null)
                    context.Dispose();
            }
            return found;
        }

        public void UpdateCacheItem(DatabaseContext.Cache.Cache cache, DatabaseContext.MyDBContext context)
        {
            try
            {
                cache.AbsoluteUri = AbsoluteUri;
                cache.CharacterSet = CharacterSet ?? "";
                cache.ContentLength = ContentLength;
                cache.ContentType = ContentType;
                cache.Expires = Expires ?? DateTime.Today;
                cache.StatusCode = (int)StatusCode;
                cache.StatusDescription = StatusDescription;
                cache.UserAgent = UserAgent;
                cache.Size = Size;
                cache.Parts = NumFileParts;

                foreach (Tuple<string, string> header in ResponseHeaders)
                {
                    context.FetchSet<DatabaseContext.Cache.CacheHeader>().Add(new DatabaseContext.Cache.CacheHeader()
                    {
                        Key = header.Item1,
                        Value = header.Item2,
                        Cache = cache
                    });
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException("Update cache item", ex.Message);
            }
        }

        internal bool UpdateInDatabase(int increaseAccessCount, DatabaseContext.Cache.Cache cache, DatabaseContext.MyDBContext context)
        {
            bool isUpdate = cache != null;

            try
            {
                int hash = Helpers.Proxy.ProxyCache.GetUriHash(AbsoluteUri);

                if (cache == null)
                {
                    cache = new DatabaseContext.Cache.Cache()
                    {
                        Id = hash,
                        DateCreated = DateTime.Now,
                        AccessCount = 0
                    };
                }
                else
                {
                    cache.CacheHeaders.ToList().ForEach(c => context.RemoveWithoutSaving<DatabaseContext.Cache.CacheHeader>(c));
                }

                if (!isUpdate)
                    context.FetchSet<DatabaseContext.Cache.Cache>().Add(cache);

                UpdateCacheItem(cache, context);
                cache.DateModified = DateModified;

                if (!_increasedAccessCount)
                {
                    cache.AccessCount++;
                    cache.AccessValue = GetGDSFPriority(cache.AccessCount);
                    _increasedAccessCount = true;
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException("UpdateInDatabase()", ex.Message);
            }

            return false;
        }

        internal static void IncreaseAccessCount(int num, long size, DatabaseContext.Cache.Cache cache)
        {
            cache.AccessCount += num;
            cache.AccessValue = GetGDSFPriority(cache.AccessCount, size);
        }
    }
}