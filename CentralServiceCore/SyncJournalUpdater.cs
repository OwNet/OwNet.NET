using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralServiceCore.Data;
using Newtonsoft.Json;

namespace CentralServiceCore
{
    public class SyncJournalUpdater : WorkspaceAction
    {
        public List<Dictionary<string, object>> GetUpdates(Dictionary<string, Dictionary<string, int>> clientState, bool syncAllGroups = true)
        {
            if (!syncAllGroups)
                clientState[""] = new Dictionary<string, int>() { { "", 0 } };
            var container = DataController.Container;
            var workspace = GetWorkspace(container);
            var results = from i in workspace.SyncJournalItems
                            where
                            ((clientState.ContainsKey(Convert.ToString(i.GroupId)) &&
                                (!clientState[Convert.ToString(i.GroupId)].ContainsKey(i.ClientId) ||
                                    clientState[Convert.ToString(i.GroupId)][i.ClientId] < i.ClientRecordNumber)) ||
                            (syncAllGroups && !clientState.ContainsKey(Convert.ToString(i.GroupId)))) &&
                            (i.GroupId != SyncJournalInjection.CacheGroupId || i.ClientId == SyncJournalInjection.ClientId)
                          orderby i.DateCreated
                          select i;

            var updates = new List<Dictionary<string, object>>();
            foreach (var item in results)
            {
                updates.Add(new Dictionary<string,object>()
                {
                    { "client_id", item.ClientId },
                    { "client_rec_num",item.ClientRecordNumber },
                    { "table_name", item.TableName },
                    { "uid", item.Uid },
                    { "operation_type", (int)item.OperationType },
                    { "group_id", item.GroupId },
                    { "sync_with", item.SyncWith },
                    { "date_created", item.DateCreated.ToString("yyyy-MM-ddTHH:mm:ss") },
                    { "columns", JsonConvert.DeserializeObject<Dictionary<string, string>>(item.Columns) }
                });
            }

            return updates;
        }
    }
}
