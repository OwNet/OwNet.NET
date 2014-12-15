using System;
using System.Collections.Generic;
using System.Linq;
using CentralServerShared;
using ClientAndServerShared;

namespace WPFCentralServer
{
    class Prefetcher
    {
        private static object _prefetchForPlannedUpdatesLock = new object();

        
        public static void PrefetchForPlannedUpdates()
        {
            LogsController.WriteMessage("Started prefetching recommendations for updates.");
            lock (_prefetchForPlannedUpdatesLock)
            {
                try
                {
                    using (DataModelContainer container = new DataModelContainer())
                    {
                        var servers = container.Servers;
                        foreach (var server in servers)
                        {
                            if (server.Prediction != null)
                            {
                                if (server.RecommendedUpdates.Any(r => r.Sent == false && r.DateCreated >= server.Prediction.LastPrediction && r.Priority < 0))
                                {
                                    continue;
                                }
                                else
                                {
                                    int pagesCount = server.Prediction.PagesCount;  // pocet stranok
                                    int linksCount = server.Prediction.LinksCount;  // pocet odkazov zo stranok
                                    var predAccess = server // pristup na predchadzajuce odporucania
                                        .RecommendedUpdates
                                        .Where(r => r.DateCreated >= server.Prediction.LastPrediction && r.Priority < 0)
                                        .Select(r => r.Cache.AccessLogs.Count(c => c.AccessedAt >= server.Prediction.LastPrediction));
                                    int all = predAccess.Count();   // pocet vsetkych odporucani
                                    int nodown = predAccess.Count(p => p == 0); // pocet nestiahnutych odporucani
                                    int down = predAccess.Count(p => p >= 1);   // pocet stiahnutych odporucani
                                    int visit = predAccess.Count(p => p > 1);   // pocet navstivenych odporucani

                                    if (pagesCount == 0 || linksCount == 0)
                                    {   // ak pocty klesli k nule, odznovu
                                        pagesCount = 3;
                                        linksCount = 3;
                                    }
                                    else
                                    {
                                        // adaptacia poctu stranok na predpoved
                                        //   podla uspesnosti stiahnutia predchadzajucich predpovedi
                                        if (down == all)
                                        {   // ak sa stiahli vsetky posledne odporucania
                                            pagesCount += 2;    // zvys pocet stranok
                                        }
                                        else if (down == 0)
                                        {   // ak sa nestiahli ziadne, zniz pocet stranok
                                            pagesCount = Math.Max(pagesCount - 2, 0);
                                        }
                                        else if (down <= all / 2)
                                        {   // ak sa stiahlo malo, zniz pocet stranok
                                            pagesCount = Math.Max(pagesCount - 1, 0);
                                        }

                                        // adaptacia poctu odkazov zo stranok na stiahnutie
                                        //   podla navstivenia predchadzajucich odporucani
                                        if (visit == down)
                                        {   // ak sa navstivili vsetky odkazy stranok
                                            linksCount += 2;    // zvys ich pocet
                                        }
                                        else if (visit == 0)
                                        {   // ak sa nenavstivili, zniz pocet odkazov
                                            linksCount = Math.Max(linksCount - 2, 0);
                                        }
                                        else if (visit < down / 2)
                                        {
                                            linksCount = Math.Max(linksCount - 1, 0);
                                        }
                                    }
                                    server.Prediction.LinksCount = linksCount;
                                    server.Prediction.PagesCount = pagesCount;
                                }
                            }
                            else
                            {   // ak server este nemal predpovede
                                server.Prediction = new Prediction() { LastPrediction = DateTime.Now, PagesCount = 3, LinksCount = 3 };
                            }

                            var tops = server.RecommendedUpdates
                                            .Where(r => r.Sent == false && r.Priority > 0)
                                            .OrderByDescending(r => r.Priority)
                                            .ThenByDescending(r => r.Cache.AccessLogs.Count)
                                            .Take(server.Prediction.PagesCount);
                            List<string> allLinks = new List<string>();
                            foreach (var top in tops)
                            {
                                List<string> links = LinksExtractor.GetFirstLinks(CentralServerShared.HttpGetRetriever.DownloadFile(top.Cache.AbsoluteUri), top.Cache.AbsoluteUri, server.Prediction.LinksCount, HttpGetRetriever.MatchesBlacklist);
                                    
                                foreach (string link in links)
                                {
                                    int uriHash = Helpers.Proxy.ProxyCache.GetUriHash(link);
                                    if (!server.Caches.Any(c => c.UriHash == uriHash))
                                    {
                                        Cache cache = new Cache()
                                        {
                                            AbsoluteUri = link,
                                            DateCreated = DateTime.Now,
                                            DateUpdated = DateTime.Now,
                                            UriHash = uriHash,
                                            Type = 1
                                        };
                                        server.Caches.Add(cache);
                                        CacheUpdater.UpdateCacheHash(cache);
                                        cache.RecommendedUpdates.Add(new CentralServerShared.RecommendedUpdate()
                                        {
                                            DateCreated = DateTime.Now,
                                            Sent = false,
                                            Priority = top.Priority * -1,
                                            Server = server
                                        });
                                    }
                                }
                            }
                        }
                        container.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    Controller.WriteException("PrefetchForPlannedUpdates()", e.Message);
                }
            }
        }

    }
}
