using System;
using System.IO;
using System.Linq;
using SharedProxy;
using WPFServer.DatabaseContext;
using System.Collections.Generic;
using ClientAndServerShared;
using Helpers.Proxy;
using WPFServer.LocalServerCentralService;

namespace WPFServer.CentralService
{
    public static class CacheUpdates
    {
        private static object _updatingCacheLock = new object();

        public static void GetLinksToUpdate()
        {
            LocalServerServiceClient client = new LocalServerServiceClient();
            System.Collections.Generic.List<LinkToUpdate> linksToUpdate = null;
            System.Collections.Generic.List<ServiceEntities.CentralService.v2.LinkToUpdate> linksToPrefetch = null;

            // call service for links to update and prefetch
            try
            {
                /* poslem cas posledneho updatu*/
                DateTime lastUpdate = DateTime.Now;

                /* ide na central service */
                var response = client.Update( Properties.Settings.Default.LastUpdateFromCentralService, CentralServiceCommunicator.GetServerInfo());
                var links = response.LinksToUpdate;

                Properties.Settings.Default.LastUpdateFromCentralService = lastUpdate;
                Properties.Settings.Default.Save();

                GroupUpdatesReceiver.Process(response.ModifiedGroupLogs, response.NewGroupRecommendationLogs);
                
                linksToUpdate = links.Where(l => l.Priority > 0).ToList();
                linksToPrefetch = links.Where(l => l.Priority <= 0).Select(x => new ServiceEntities.CentralService.v2.LinkToUpdate()
                    {
                        AbsoluteUri = x.AbsoluteUri,
                        Priority = x.Priority
                    }).ToList();

            }
            catch (Exception e)
            {
                Controller.WriteException("Update cache - services", e.Message);
            }
            finally 
            {
                client.Close();
            }

            // process links to update
            try
            {
                if (linksToUpdate != null && linksToUpdate.Any())
                {
                    using (MyDBContext context = new MyDBContext())
                    {
                        DateTime dateRecommended = DateTime.Now;
                        Dictionary<int, List<LinkToUpdate>> clientUpdates = new Dictionary<int, List<LinkToUpdate>>();

                        foreach (LinkToUpdate link in linksToUpdate)
                        {
                            int uriHash = ProxyCache.GetUriHash(link.AbsoluteUri);
                            var caches = from p in context.FetchSet<CacheLink>()
                                         where p.Id == uriHash
                                         select p;

                            if (caches.Any())
                            {
                                CacheLink cache = caches.First();
                                foreach (var clientCacheLink in cache.ClientCacheLinks)
                                {
                                    if (!clientUpdates.ContainsKey(clientCacheLink.ClientId))
                                        clientUpdates[clientCacheLink.ClientId] = new List<LinkToUpdate>();
                                    clientUpdates[clientCacheLink.ClientId].Add(link);
                                }

                                //LogsController.WriteMessage("Update recommended " + link.AbsoluteUri);
                            }
                        }

                        foreach (var pair in clientUpdates)
                        {
                            var clientItem = context.Fetch<Client>(c => c.Id == pair.Key).First();
                            if (clientItem.ClientName == AppSettings.ServerClientName)
                            {
                                List<ServiceEntities.Cache.CacheLinkToUpdate> serverUpdates = new List<ServiceEntities.Cache.CacheLinkToUpdate>();
                                pair.Value.Select(c => new ServiceEntities.Cache.CacheLinkToUpdate()
                                {
                                    AbsoluteUri = c.AbsoluteUri,
                                    Priority = c.Priority
                                });
                                Cache.CacheMaintainer.ProcessLinksToUpdate(serverUpdates);
                            }
                            else
                            {
                                foreach (LinkToUpdate link in pair.Value)
                                {
                                    context.Create<GlobalCacheUpdate>(new GlobalCacheUpdate()
                                    {
                                        AbsoluteUri = link.AbsoluteUri,
                                        DateRecommended = DateTime.Now,
                                        ClientId = pair.Key,
                                        Priority = link.Priority
                                    });
                                }
                            }
                        }
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Controller.WriteException("Update cache - update", ex.Message);
            }

            if (linksToPrefetch != null && linksToPrefetch.Count > 0)
                WPFServer.Prefetching.Prefetcher.RegisterPredictions(linksToPrefetch);
        }
    }
}
