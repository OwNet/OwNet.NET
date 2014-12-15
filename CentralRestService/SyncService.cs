using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using CentralServiceCore;
using System.Linq;
using System;
using Newtonsoft.Json;

namespace CentralRestService
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class SyncService : ISyncService
    {
        [WebInvoke(Method = "GET", UriTemplate = "/{workspaceId}/GetSyncState", ResponseFormat = WebMessageFormat.Json)]
        public Message GetSyncState(string workspaceId)
        {
            SyncJournalReporter reporter = new SyncJournalReporter();
            reporter.WorkspaceId = workspaceId;

            return JsonHelper.Serialize(reporter.GetSyncState());
        }

        [WebInvoke(Method = "POST", UriTemplate = "/ReportUpdates")]
        public void ReportUpdates(Stream stream)
        {
            var dict = JsonHelper.Deserialize<Dictionary<string, object>>(stream);

            SyncJournalReporter reporter = new SyncJournalReporter();
            reporter.WorkspaceId = dict["workspace_id"] as string;
            reporter.WorkspaceName = dict["workspace_name"] as string;
            var updates = JsonHelper.Deserialize<IEnumerable<Dictionary<string, object>>>(dict["updates"].ToString());

            reporter.SaveReport(updates);
        }

        [WebInvoke(Method = "POST", UriTemplate = "/ReportHistory")]
        public void ReportHistory(Stream stream)
        {
            var dict = JsonHelper.Deserialize<Dictionary<string, object>>(stream);

            AccessLogReporter reporter = new AccessLogReporter();
            reporter.WorkspaceId = dict["workspace_id"] as string;
            var updates = JsonHelper.Deserialize<IEnumerable<Dictionary<string, object>>>(dict["history"].ToString());

            reporter.SaveReport(updates);
        }

        [WebInvoke(Method = "POST", UriTemplate = "/GetUpdates")]
        public Message GetUpdates(Stream stream)
        {
            var dict = JsonHelper.Deserialize<Dictionary<string, object>>(stream);
            SyncJournalUpdater updater = new SyncJournalUpdater();
            updater.WorkspaceId = dict["workspace_id"].ToString();
            var updates = JsonHelper.Deserialize<Dictionary<string, Dictionary<string, int>>>(dict["client_record_numbers"].ToString());
            return JsonHelper.Serialize(updater.GetUpdates(updates).Take(500));
        }
    }
}