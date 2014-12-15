using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Web;
using ClientAndServerShared;
using ServiceEntities;
using ServiceEntities.Cache;
using WPFServer.DatabaseContext;
using WPFServer.Clients;

namespace WPFServer
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(UseSynchronizationContext = false, InstanceContextMode = InstanceContextMode.PerCall)]
    public class CacheService : ICacheService
    {
        [WebInvoke(UriTemplate = "create", Method = "POST")]
        public CacheCreateResponses Create(CacheBatchRequest cacheBatchRequest)
        {
            Proxy.RequestProcessing.ProcessRequests(cacheBatchRequest);

            OperationContext context = OperationContext.Current;
            MessageProperties messageProperties = context.IncomingMessageProperties;
            RemoteEndpointMessageProperty endpointProperty = messageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

            ClientsController.ClientIsOnline(cacheBatchRequest.ClientName, endpointProperty.Address);
            ClientCacheLogsReporter.ClientCachesReported(cacheBatchRequest, endpointProperty.Address);

            return Proxy.RequestProcessing.GetResponses(cacheBatchRequest.ClientName);
        }

        [WebGet(UriTemplate = "new_items/?since={since}")]
        public NewCacheList NewItems(string since)
        {
            NewCacheList newCaches = new NewCacheList();
            try
            {
                newCaches.Items = Proxy.AccessLogs.AccessLogsReporter.NewCacheItemsSince(DateTime.Parse(HttpUtility.UrlDecode(since)));
            }
            catch (Exception e)
            {
                Server.WriteException(e.Message); 
            }
            return newCaches;
        }

        [WebInvoke(UriTemplate = "logs/report", Method = "POST")]
        public ServerSettings ReportLogs(CacheLogsReport logsReport)
        {
            try
            {
                OperationContext context = OperationContext.Current;
                MessageProperties messageProperties = context.IncomingMessageProperties;
                RemoteEndpointMessageProperty endpointProperty = messageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

                ClientsController.ClientIsOnline(logsReport.ClientName, endpointProperty.Address);

                Proxy.AccessLogs.AccessLogsReporter.ReceivedReportFromClient(logsReport);
            }
            catch (Exception e)
            {
                Server.WriteException(e.Message);
            }
            return CreateServerSettings(logsReport.ClientName);
        }

        [WebGet(UriTemplate = "ping/?clientName={clientName}")]
        public ServerSettings Ping(string clientName)
        {
            OperationContext context = OperationContext.Current;
            MessageProperties messageProperties = context.IncomingMessageProperties;
            RemoteEndpointMessageProperty endpointProperty = messageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

            ClientsController.ClientIsOnline(clientName, endpointProperty.Address);

            return CreateServerSettings(clientName);
        }

        [WebGet(UriTemplate = "server_name")]
        public string ServerName()
        {
            return Server.Settings.ClientName();
        }

        private static ServerSettings CreateServerSettings(string clientName)
        {
            var settings = new ServerSettings()
            {
                DownloadEverythingThroughServer = Properties.Settings.Default.DownloadEverything,
                Updates = new List<CacheLinkToUpdate>()
            };

            try
            {
                using (MyDBContext context = new MyDBContext())
                {
                    var clients = context.Fetch<Client>(c => c.ClientName == clientName);
                    if (clients.Any())
                    {
                        var client = clients.First();
                        var updates = context.Fetch<GlobalCacheUpdate>(c => c.ClientId == client.Id);
                        foreach (var update in updates)
                        {
                            settings.Updates.Add(new CacheLinkToUpdate()
                            {
                                AbsoluteUri = update.AbsoluteUri,
                                Priority = update.Priority
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException("CreateServerSettings", ex.Message);
            }

            return settings;
        }
    }
}