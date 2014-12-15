using System.Collections.Generic;
using ServiceEntities.Cache;

namespace WPFServer.Proxy
{
    class RequestProcessing
    {
        private static Dictionary<string, ClientRequestProcessing> _clientProcessors = new Dictionary<string, ClientRequestProcessing>();

        public static void ProcessRequests(CacheBatchRequest batchRequest)
        {
            if (batchRequest.Requests == null)
                return;

            ClientRequestProcessing processor = null;
            lock (_clientProcessors)
            {
                if (!_clientProcessors.ContainsKey(batchRequest.ClientName))
                {
                    processor = new ClientRequestProcessing(batchRequest.ClientName);
                    _clientProcessors[batchRequest.ClientName] = processor;
                }
                else
                    processor = _clientProcessors[batchRequest.ClientName];
            }
            processor.ProcessRequests(batchRequest);
        }

        public static void PrepareResponses()
        {
            foreach (var pair in _clientProcessors)
            {
                pair.Value.PrepareResponses();
            }
        }

        public static CacheCreateResponses GetResponses(string clientName)
        {
            return _clientProcessors[clientName].GetResponses();
        }
    }
}
