using System.ServiceModel;
using ServiceEntities;
using ServiceEntities.Cache;

namespace SharedProxy.Services.Host
{
    [ServiceContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/")]
    public interface IClientCacheService
    {
        [OperationContract]
        CacheCreateResponse Load(CacheRequest cacheRequest);

        [OperationContract]
        bool Ping();

        [OperationContract]
        SearchResultsCollection Search(string query, string from);
    }
}
