using System;
using System.Collections.Generic;
using WPFServer.DatabaseContext;

namespace WPFServer.Clients
{
    public class ClientCacheLogsReporter
    {
        internal static void ClientCachesReported(ServiceEntities.Cache.CacheBatchRequest batchRequest, string ip)
        {
            if (batchRequest.Reports == null)
                return;

            try
            {
                ClientsController.ClientIsOnline(batchRequest.ClientName, ip);

                using (MyDBContext con = new MyDBContext())
                {
                    var client = con.FetchOrCreate<Client>(new Client()
                    {
                        ClientName = batchRequest.ClientName
                    });
                    client.LastIP = ip;

                    foreach (var report in batchRequest.Reports)
                    {
                        int hash = Helpers.Proxy.ProxyCache.GetUriHash(report.AbsoluteUri);
                    
                        var cacheLink = con.FetchOrCreate<CacheLink>(new CacheLink()
                        {
                            Id = hash,
                            AbsoluteURI = report.AbsoluteUri,
                            LastModified = report.DateModified
                        });

                        var clientCacheLink = con.FetchOrCreate<ClientCacheLink>(new ClientCacheLink()
                        {
                            Client = client,
                            CacheLink = cacheLink
                        });
                    }

                    con.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Server.WriteException("ClientCacheReported", e.Message);
            }
        }

        internal static void ClientCacheReported(string clientName, string ip, string url, DateTime dateModified)
        {
            ClientCachesReported(new ServiceEntities.Cache.CacheBatchRequest()
            {
                ClientName = clientName,
                Reports = new List<ServiceEntities.Cache.NewCacheReport>()
                {
                    new ServiceEntities.Cache.NewCacheReport()
                    {
                        AbsoluteUri = url,
                        DateModified = dateModified
                    }
                }
            }, ip);
        }
    }
}
