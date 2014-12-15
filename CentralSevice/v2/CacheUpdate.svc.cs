using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ServiceEntities;
using CentralServerShared;
using ServiceEntities.CentralService.v2;

namespace CentralService.v2
{
    public class CacheUpdate : ICacheUpdate
    {
        public Updates GetLinksToUpdate(DateTime lastUpdate, ServerInfo info)
        {
            ServerController serverController = Controller.GetServerController(info);
            var updates = new Updates()
            {
                LinksToUpdate = serverController.GetRecommendedUpdates(),
                ModifiedGroupLogs = GroupUpdatesHandler.GetModifiedGroups(lastUpdate),
                NewGroupRecommendationLogs = serverController.GetNewRecommendations(lastUpdate)
            };
            return updates;
        }

        public void Report(ReportLog reports, ServerInfo info)
        {
            ServerController serverController = Controller.GetServerController(info);
            Controller.ReceivedAccessLogsFromServer(reports.AccessLogs, serverController);
            serverController.ReceivedLogsReport(reports.ActivityLogs);
            serverController.ReceivedGroupsReport(reports.GroupLogs, reports.GroupRecommendationLogs);
        }
    }
}
