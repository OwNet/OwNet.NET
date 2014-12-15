using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralServiceCore.Data;
using Newtonsoft.Json;

namespace CentralServiceCore
{
    public class SyncJournalInjection : WorkspaceAction
    {
        public static string ClientId = "WEB";
        public static int CacheGroupId = 10;

        public string Inject(string tableName, Dictionary<string, object> columns, SyncJournalItem.OperationTypes operationType = SyncJournalItem.OperationTypes.InsertOrUpdate, int? groupId = null, string syncWith = "", string uid = "")
        {
            var container = DataController.Container;
            var workspace = GetWorkspace(container);
            Properties.Settings.Default.LastClientRecordNumber++;

            if (uid == "")
                uid = string.Format("{0}_{1}", ClientId,
                    Properties.Settings.Default.LastClientRecordNumber);
            
            workspace.SyncJournalItems.Add(container.SyncJournalItems.Add(new SyncJournalItem()
            {
                ClientId = ClientId,
                ClientRecordNumber = Properties.Settings.Default.LastClientRecordNumber,
                TableName = tableName,
                Columns = JsonConvert.SerializeObject(columns),
                OperationType = operationType,
                GroupId = groupId,
                Uid = uid,
                DateCreated = DateTime.Now,
                SyncWith = syncWith,
                Workspace = workspace
            }));

            var syncState = workspace.SyncJournalStates.Where(s => s.ClientId == ClientId && s.GroupId == groupId).FirstOrDefault();
            if (syncState == null)
            {
                syncState = new SyncJournalState()
                {
                    ClientId = ClientId,
                    GroupId = groupId,
                    LastClientRecordNumber = Properties.Settings.Default.LastClientRecordNumber,
                    DateCreated = DateTime.Now,
                    Workspace = workspace
                };
                workspace.SyncJournalStates.Add(container.SyncJournalStates.Add(syncState));
            }
            else
            {
                syncState.LastClientRecordNumber = Properties.Settings.Default.LastClientRecordNumber;
            }
            container.SaveChanges();
            Properties.Settings.Default.Save();

            return uid;
        }
    }
}
