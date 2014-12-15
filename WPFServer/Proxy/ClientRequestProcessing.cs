using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WPFServer.DatabaseContext;
using ServiceEntities.Cache;
using WPFServer.Clients;

namespace WPFServer.Proxy
{
    class ClientRequestProcessing
    {
        string _clientName = "";

        private List<CacheCreateResponse> _responses = new List<CacheCreateResponse>();
        private List<CacheCreateResponse> _responsesToSend = new List<CacheCreateResponse>();
        private Semaphore _responseSem = new Semaphore(0, 1000);

        private int _numUnprocessedRequests = 0;
        private int _numWaitingRequesters = 0;
        private object _requestsLock = new object();

        public ClientRequestProcessing(string name)
        {
            _clientName = name;
        }

        public void ProcessRequests(CacheBatchRequest batchRequest)
        {
            lock (_requestsLock)
            {
                _numUnprocessedRequests += batchRequest.Requests.Count;
                _numWaitingRequesters++;
            }

            if (batchRequest.Requests != null)
                foreach (CacheRequest request in batchRequest.Requests)
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessThread), request);
        }

        private void ProcessThread(Object obj)
        {
            CacheRequest cacheRequest = (CacheRequest)obj;
            CacheCreateResponse response = null;
            int hash = Helpers.Proxy.ProxyCache.GetUriHash(cacheRequest.AbsoluteUri);

            if (Properties.Settings.Default.DownloadEverything)
            {
                response = SharedProxy.Services.Host.ClientCacheService.GetCacheResponse(cacheRequest, true);
            }
            else
            {
                response = new CacheCreateResponse()
                {
                    Found = false,
                    CacheID = hash,
                    ClientIP = ""
                };

                try
                {
                    using (MyDBContext con = new MyDBContext())
                    {
                        var cacheLinks = con.Fetch<CacheLink>(n => n.Id == hash);

                        if (cacheLinks.Any())
                        {
                            var cacheLink = cacheLinks.First();

                            if (cacheRequest.RefreshIfOlderThan != null && cacheLink.LastModified < cacheRequest.RefreshIfOlderThan)
                            {
                                response.Found = false;
                            }
                            else if (cacheLink.ClientCacheLinks.Any())
                            {
                                bool changes = false;
                                foreach (var clientCacheLink in cacheLink.ClientCacheLinks.ToArray())
                                {
                                    if (ClientsController.IsOnline(clientCacheLink.Client.ClientName))
                                    {
                                        try
                                        {
                                            CacheCreateResponse cacheResponse = null;
                                            string clientIP = ClientsController.GetClientIP(clientCacheLink.Client.ClientName);

                                            if (clientCacheLink.Client.ClientName == AppSettings.ServerClientName)
                                            {
                                                cacheResponse = SharedProxy.Services.Host.ClientCacheService.GetCacheResponse(cacheRequest);
                                            }
                                            else
                                            {
                                                cacheResponse = SharedProxy.ServerRequestManager.Post<CacheRequest, CacheCreateResponse>("clientcache/load",
                                                    cacheRequest,
                                                    clientIP);
                                            }

                                            if (cacheResponse.Found)
                                            {
                                                response.Found = true;
                                                response.CacheID = cacheResponse.CacheID;
                                                response.ClientIP = clientIP;
                                                break;
                                            }
                                            else
                                            {
                                                con.Remove<ClientCacheLink>(clientCacheLink);
                                                changes = true;
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            ClientsController.ClientIsOffline(clientCacheLink.Client.ClientName);
                                        }
                                    }
                                }
                                if (changes)
                                    con.SaveChanges();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Server.WriteException(e.Message);
                }
            }

            if (response != null)
            {
                response.RequestId = cacheRequest.RequestId;
                AddResponse(response);
            }
        }

        private void AddResponse(CacheCreateResponse response)
        {
            lock (_responses)
                _responses.Add(response);
        }

        public void PrepareResponses()
        {
            if (_responses.Count == 0)
                return;

            lock (_responsesToSend)
            {
                lock (_responses)
                {
                    if (_responses.Count == 0)
                        return;

                    _responsesToSend.AddRange(_responses);
                    _responses.Clear();
                }
            }
            _responseSem.Release();
        }

        public CacheCreateResponses GetResponses()
        {
            lock (_requestsLock)
            {
                if (_numUnprocessedRequests == 0)
                {
                    _numWaitingRequesters--;
                    return new CacheCreateResponses();
                }
            }

            _responseSem.WaitOne();

            CacheCreateResponses responses = null;

            lock (_responsesToSend)
            {
                responses = new CacheCreateResponses(_responsesToSend);
                _responsesToSend.Clear();
            }

            lock (_requestsLock)
            {
                _numWaitingRequesters--;
                _numUnprocessedRequests -= responses.Count;

                if (_numUnprocessedRequests == 0 && _numWaitingRequesters > 0)
                    _responseSem.Release();
            }

            return responses;
        }
    }
}
