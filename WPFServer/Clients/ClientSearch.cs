using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceEntities;
using ClientAndServerShared;
using System.Web;

namespace WPFServer.Clients
{
    class ClientSearch
    {
        internal static ServerSearchResults SearchOnClients(string query, Dictionary<string, int> searchedClients)
        {
            string decodedQuery = HttpUtility.UrlDecode(query);
            List<ClientSearchResult> allClientResults = new List<ClientSearchResult>();

            foreach (var pair in searchedClients)
            {
                SearchResultsCollection clientResults = null;
                if (pair.Key == AppSettings.ServerClientName)
                {
                    try
                    {
                        int count;
                        clientResults = SearchDatabase.SearchFrom(decodedQuery, pair.Value, out count);
                    }
                    catch (Exception ex)
                    {
                        LogsController.WriteException("Search", ex.Message);
                    }
                }
                else
                {
                    try
                    {
                        clientResults = SharedProxy.ServerRequestManager.Get<SearchResultsCollection>(
                            new Uri(string.Format("clientcache/search?query={0}&from={1}", query, pair.Value), UriKind.Relative),
                                null, ClientsController.GetClientIP(pair.Key));
                    }
                    catch (Exception ex)
                    {
                        LogsController.WriteException("Search", ex.Message);
                    }
                }
                if (clientResults != null)
                    allClientResults.Add(new ClientSearchResult()
                    {
                        Results = clientResults,
                        Name = pair.Key
                    });
            }
            ServerSearchResults results = new ServerSearchResults()
            {
                Results = GetBestResults(allClientResults, searchedClients)
            };

            foreach (var client in allClientResults)
                searchedClients[client.Name] += client.Index;

            if (results != null)
                results.SearchedClients = searchedClients;

            return results;
        }

        private static List<PageObjectWithContent> GetBestResults(List<ClientSearchResult> allClientResults, Dictionary<string, int> searchedClients)
        {
            List<PageObjectWithContent> results = new List<PageObjectWithContent>();
            List<string> usedLinks = new List<string>();

            while (results.Count < Helpers.Search.ItemsPerPage)
            {
                ClientSearchResult bestClient = null;
                foreach (var client in allClientResults)
                {
                    if (client.Index >= client.Results.Count)
                        continue;

                    if (bestClient != null && bestClient.NextRank() >= client.NextRank())
                        continue;

                    if (!usedLinks.Contains(client.NextResult().AbsoluteURI))
                    {
                        usedLinks.Add(client.NextResult().AbsoluteURI);
                        bestClient = client;
                    }
                }

                if (bestClient == null)
                    break;

                results.Add(bestClient.NextResult());
                bestClient.Index++;
            }

            return results;
        }

        class ClientSearchResult
        {
            public SearchResultsCollection Results { get; set; }
            public string Name { get; set; }
            public int Index = 0;

            public int NextRank()
            {
                return Results[Index].Rank;
            }

            public PageObjectWithContent NextResult()
            {
                return Results[Index];
            }
        }
    }
}
