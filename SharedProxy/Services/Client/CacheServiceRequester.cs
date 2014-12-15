using System;
using System.Collections.Generic;
using System.Threading;
using ServiceEntities.Cache;

namespace SharedProxy.Services.Client
{
    public class CacheServiceRequester
    {
        private static List<CacheServiceRequest> _unhandledRequests = new List<CacheServiceRequest>();
        private static List<NewCacheReport> _unhandledReports = new List<NewCacheReport>();
        private static Dictionary<int, CacheServiceRequest> _sentRequests = new Dictionary<int, CacheServiceRequest>();

        private static int _lastRequestId = 0;
        private static int _numSentRequests = 0;
        private static object _sentRequestsLock = new object();

        internal static CacheCreateResponse GetResponse(CacheRequest request)
        {
            var serviceRequest = new CacheServiceRequest() { Request = request };
            lock (_unhandledRequests)
            {
                serviceRequest.Request.RequestId = _lastRequestId++;
                _unhandledRequests.Add(serviceRequest);
            }
            serviceRequest.Wait();
            return serviceRequest.Response;
        }

        public static void ReportItem(NewCacheReport report)
        {
            lock (_unhandledReports)
                _unhandledReports.Add(report);
        }

        internal static void RequestService()
        {
            List<CacheServiceRequest> requests = null;
            List<NewCacheReport> reports = null;

            lock (_unhandledRequests)
            {
                if (_unhandledRequests.Count > 0)
                {
                    requests = new List<CacheServiceRequest>(_unhandledRequests);
                    _unhandledRequests.Clear();
                }
            }
            lock (_unhandledReports)
            {
                if (_unhandledReports.Count > 0)
                {
                    reports = new List<NewCacheReport>(_unhandledReports);
                    _unhandledReports.Clear();
                }
            }

            if (requests == null && reports == null)
            {
                if (_sentRequests.Count == 0)
                    return;

                lock (_sentRequestsLock)
                    if (_numSentRequests > 0)
                        return;
            }

            CacheBatchRequest batchRequest = new CacheBatchRequest()
            {
                ClientName = Controller.AppSettings.ClientName(),
                Requests = new List<CacheRequest>(),
                Reports = reports
            };

            if (requests != null)
            {
                foreach (var request in requests)
                {
                    batchRequest.Requests.Add(request.Request);
                    _sentRequests[request.Request.RequestId] = request;
                }
            }

            try
            {
                lock (_sentRequestsLock)
                    _numSentRequests++;

                CacheCreateResponses cacheResponses = ServerRequestManager.Post<CacheBatchRequest, CacheCreateResponses>("cache/create", batchRequest);

                lock (_sentRequestsLock)
                    _numSentRequests--;

                foreach (CacheCreateResponse response in cacheResponses)
                {
                    if (_sentRequests.ContainsKey(response.RequestId))
                    {
                        var item = _sentRequests[response.RequestId];
                        _sentRequests.Remove(response.RequestId);
                        if (response.ClientIP == "127.0.0.1" || response.ClientIP == "localhost")
                            response.ClientIP = Controller.AppSettings.ServerIP();
                        item.Response = response;
                        item.Release();
                    }
                }
            }
            catch (Exception ex)
            {
                Controller.WriteException("RequestCacheService", ex.Message);

                if (requests != null)
                    foreach (var request in requests)
                        request.Release();
            }
        }

        class CacheServiceRequest
        {
            public CacheRequest Request = null;
            public CacheCreateResponse Response = null;
            private Semaphore _sem = new Semaphore(0, 1);

            public void Wait()
            {
                _sem.WaitOne();
            }

            public void Release()
            {
                _sem.Release();
            }
        }
    }
}
