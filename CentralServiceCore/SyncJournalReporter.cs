using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralServiceCore.Data;
using CentralServiceCore.Helpers;
using Newtonsoft.Json;

namespace CentralServiceCore
{
    public class SyncJournalReporter : WorkspaceAction
    {
        public Dictionary<string, Dictionary<string, int>> GetSyncState()
        {
            var syncJournalState = new Dictionary<string, Dictionary<string, int>>();

            var container = DataController.Container;

            foreach (var clientState in GetWorkspace(container).SyncJournalStates)
            {
                if (syncJournalState.ContainsKey(Convert.ToString(clientState.GroupId)))
                {
                    syncJournalState[Convert.ToString(clientState.GroupId)]
                        .Add(clientState.ClientId, clientState.LastClientRecordNumber);
                }
                else
                {
                    syncJournalState.Add(clientState.GroupId.HasValue ? Convert.ToString(clientState.GroupId) : "",
                        new Dictionary<string, int>()
                    {
                        { clientState.ClientId, clientState.LastClientRecordNumber }
                    });
                }
            }

            return syncJournalState;
        }

        public bool SaveReport(IEnumerable<Dictionary<string, object>> items)
        {
            var container = DataController.Container;
            var workspace = GetWorkspace(container);
            var lastRecordNumbers = new Dictionary<string,int>();

            foreach (var item in items)
            {
                var journalItem = new SyncJournalItem()
                {
                    GroupId = item["group_id"].ToString() == "" ? null : (int?)Convert.ToInt32(item["group_id"]),
                    ClientId = item["client_id"].ToString(),
                    ClientRecordNumber = Convert.ToInt32(item["client_rec_num"]),
                    OperationType = (SyncJournalItem.OperationTypes)Convert.ToInt32(item["operation_type"]),
                    Uid = item["uid"].ToString(),
                    SyncWith = item["sync_with"].ToString(),
                    TableName = item["table_name"].ToString(),
                    Columns = item.ContainsKey("columns") ? item["columns"].ToString() : "",
                    DateCreated = DateTime.Parse(item["date_created"].ToString()),
                    Workspace = workspace
                };
                workspace.SyncJournalItems.Add(container.SyncJournalItems.Add(journalItem));

                switch (journalItem.TableName)
                {
                    case "client_caches":
                        ProcessClientCacheJournalItem(journalItem, container);
                        break;
                }

                string key = string.Format("{0}-{1}", journalItem.ClientId, journalItem.GroupId);
                if (!lastRecordNumbers.ContainsKey(key) || lastRecordNumbers[key] < journalItem.ClientRecordNumber)
                    lastRecordNumbers[key] = journalItem.ClientRecordNumber;
            }

            foreach (var pair in lastRecordNumbers)
            {
                string[] ids = pair.Key.Split('-');
                string clientId = ids.First();
                int? groupId = null;
                if (ids.Last() != "")
                    groupId = Convert.ToInt32(ids.Last());
                var state = workspace.SyncJournalStates.Where(s =>
                    s.ClientId == clientId && s.GroupId == groupId).FirstOrDefault();

                if (state == null)
                {
                    state = new SyncJournalState()
                    {
                        ClientId = clientId,
                        LastClientRecordNumber = pair.Value,
                        DateCreated = DateTime.Now,
                        Workspace = workspace
                    };
                    if (groupId.HasValue)
                        state.GroupId = groupId;
                    workspace.SyncJournalStates.Add(container.SyncJournalStates.Add(state));
                }
                else
                {
                    if (state.LastClientRecordNumber < pair.Value)
                        state.LastClientRecordNumber = pair.Value;
                }
            }
            
            container.SaveChanges();

            return true;
        }

        void ProcessClientCacheJournalItem(SyncJournalItem item, ICentralDataModelContainer container)
        {
            if (item.OperationType == SyncJournalItem.OperationTypes.InsertOrUpdate)
            {
                Dictionary<string, string> columns = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.Columns);
                if (!columns.ContainsKey("cache_id"))
                    return;

                long cacheId = Convert.ToInt64(columns["cache_id"]);

                var webObject = container.WebObjects.Where(o => o.CacheId == cacheId).FirstOrDefault();
                if (webObject == null)
                {
                    webObject = new WebObject()
                    {
                        CacheId = cacheId,
                        AbsoluteUri = "",
                        DateCreated = DateTime.Now,
                        DateUpdated = DateTime.Now,
                        IsTracked = false
                    };
                    container.WebObjects.Add(webObject);
                }
                ClientCache cache = new ClientCache()
                {
                    Workspace = item.Workspace,
                    WebObject = webObject,
                    ClientId = columns["client_id"],
                    DateCreated = DateTime.Parse(columns["date_created"]),
                    Uid = item.Uid
                };
                item.Workspace.ClientCaches.Add(cache);
                webObject.ClientCaches.Add(cache);
                container.ClientCaches.Add(cache);
            }
            else // Remove
            {
                var clientCaches = container.ClientCaches.Where(c => c.Uid == item.Uid);
                foreach (var clientCache in clientCaches.ToArray())
                    container.ClientCaches.Remove(clientCache);
                container.SaveChanges();
            }
        }
    }
}
