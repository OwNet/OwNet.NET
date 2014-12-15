using System.ServiceModel;
using ServiceEntities;
using ServiceEntities.Cache;

namespace WPFServer
{
    [ServiceContract(Namespace = @"http://imaginecup.fiit.stuba.sk/2012/")]
    public interface ICacheService
    {
        [OperationContract]
        CacheCreateResponses Create(CacheBatchRequest cacheBatchRequest);

        [OperationContract]
        NewCacheList NewItems(string since);

        [OperationContract]
        ServerSettings ReportLogs(CacheLogsReport logsReport);

        [OperationContract]
        ServerSettings Ping(string clientName);

        [OperationContract]
        string ServerName();
    }
}
