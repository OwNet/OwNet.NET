using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralServiceCore.Data;

namespace CentralServiceCore.WebTrackerCommunication
{
    public class WebTrackerUpdater
    {
        static object _lock = new object();

        public static void Update()
        {
            ClientRegistration.RegisterClient();

            lock (_lock)
            {
                try
                {
                    var client = new WebTrackerTrackingService.TrackingServiceClient();
                    DateTime now = DateTime.Now;

                    var urls = client.ListChangedWebsites(Properties.Settings.Default.LastUpdateFromWebTracker,
                        new WebTrackerTrackingService.ClientType()
                        {
                            ClientId = Properties.Settings.Default.WebTrackerClientId,
                            Code = Properties.Settings.Default.WebTrackerClientPassword
                        });

                    var container = DataController.Container;

                    foreach (string url in urls)
                    {
                        var webObject = container.WebObjects.Where(o => o.AbsoluteUri == url).FirstOrDefault();
                        if (webObject == null)
                            continue;

                        var workspaces = from c in webObject.ClientCaches
                                         select c.Workspace;

                        foreach (var workspace in workspaces.Distinct())
                        {
                            SyncJournalInjection injection = new SyncJournalInjection();
                            injection.WorkspaceId = workspace.WorkspaceId;
                            var columns = new Dictionary<string, object>()
                            {
                                { "client_id", SyncJournalInjection.ClientId },
                                { "cache_id", webObject.CacheId },
                                { "date_created", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") }
                            };

                            var clientCache = workspace.ClientCaches.Where(c => c.ClientId == SyncJournalInjection.ClientId && c.WebObject == webObject).FirstOrDefault();
                            if (clientCache == null)
                            {
                                clientCache = new ClientCache()
                                {
                                    Workspace = workspace,
                                    ClientId = SyncJournalInjection.ClientId,
                                    WebObject = webObject
                                };
                                webObject.ClientCaches.Add(container.ClientCaches.Add(clientCache));
                                workspace.ClientCaches.Add(clientCache);
                                clientCache.Uid = injection.Inject("client_caches", columns);
                            }
                            else
                            {
                                injection.Inject("client_caches", columns, SyncJournalItem.OperationTypes.InsertOrUpdate, SyncJournalInjection.CacheGroupId, "", clientCache.Uid);
                            }
                            clientCache.DateCreated = DateTime.Now;
                        }
                    }

                    container.SaveChanges();

                    Properties.Settings.Default.LastUpdateFromWebTracker = now;
                    Properties.Settings.Default.Save();
                }
                catch (Exception e)
                { }
            }
        }
    }
}
