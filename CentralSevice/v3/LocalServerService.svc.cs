using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ServiceEntities;
using CentralServerShared;
using ServiceEntities.CentralService.v3;

namespace CentralService.v3
{
    public class LocalServerService : ILocalServerService
    {
        public ServiceEntities.CentralService.v2.Updates Update(DateTime lastUpdate, ServiceEntities.CentralService.v2.ServerInfo info)
        {
            ServerController serverController = Controller.GetServerController(info);
            var updates = new ServiceEntities.CentralService.v2.Updates()
            {
                LinksToUpdate = serverController.GetRecommendedUpdates(),
                ModifiedGroupLogs = GroupUpdatesHandler.GetModifiedGroups(lastUpdate),
                NewGroupRecommendationLogs = serverController.GetNewRecommendations(lastUpdate)
            };
            return updates;
        }

        public void Report(ReportLog reports, ServiceEntities.CentralService.v2.ServerInfo info)
        {
            ServerController serverController = Controller.GetServerController(info);
            Controller.ReceivedAccessLogsFromServer(reports.AccessLogs, serverController);
            serverController.ReceivedLogsReport(reports.ActivityLogs);
            serverController.ReceivedGroupsReport(reports.GroupLogs, reports.GroupRecommendationLogs);
            serverController.ReceivedUsersReport(reports.UserLogs);
            serverController.ReceivedUserGroupsReport(reports.UserGroupReports);
        }
    }
}
