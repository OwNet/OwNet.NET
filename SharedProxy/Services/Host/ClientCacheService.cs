using System;
using System.ServiceModel.Activation;
using System.ServiceModel;
using ServiceEntities;
using System.ServiceModel.Web;
using SharedProxy.Streams.Output;
using System.Web;
using ClientAndServerShared;
using ServiceEntities.Cache;
using SharedProxy.Proxy;

namespace SharedProxy.Services.Host
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(UseSynchronizationContext = false, InstanceContextMode = InstanceContextMode.PerCall)]
    public class ClientCacheService : IClientCacheService
    {
        [WebInvoke(UriTemplate = "load", Method = "POST")]
        public CacheCreateResponse Load(CacheRequest cacheRequest)
        {
            return GetCacheResponse(cacheRequest);
        }

        public static CacheCreateResponse GetCacheResponse(CacheRequest cacheRequest, bool downloadIfNotFound = false)
        {
            int hash = Helpers.Proxy.ProxyCache.GetUriHash(cacheRequest.AbsoluteUri);
            CacheCreateResponse response = new CacheCreateResponse()
            {
                Found = false,
                CacheID = -1,
                ClientIP = ""
            };

            ProxyEntry entry = Controller.ProxyInstance.CreateCacheEntry(cacheRequest);
            //entry.CanCache = (int)(ProxyEntry.CanCacheOptions.CanCacheOnClient | ProxyEntry.CanCacheOptions.CanCacheOnServer);
            
            try
            {
                ServiceItem cache = SharedProxy.Streams.ProxyStreamManager.CreateServiceItem(entry, downloadIfNotFound);
                if (cache != null)
                {
                    response.Found = true;
                    response.CacheID = cache.NoCacheItemId;
                }
            }
            catch (Exception e)
            {
                Controller.WriteException(e.Message);
            }

            return response;
        }

        [WebGet(UriTemplate = "ping")]
        public bool Ping()
        {
            return true;
        }

        [WebGet(UriTemplate = "search?query={query}&from={from}")]
        public SearchResultsCollection Search(string query, string from)
        {
            SearchResultsCollection results = null;

            try
            {
                int itemFrom = Convert.ToInt32(from), count;
                query = HttpUtility.UrlDecode(query);
                results = SearchDatabase.SearchFrom(HttpUtility.UrlDecode(query), itemFrom, out count);
            }
            catch (Exception ex)
            {
                LogsController.WriteException("Search", ex.Message);
            }

            return results ?? new SearchResultsCollection();
        }
    }
}
