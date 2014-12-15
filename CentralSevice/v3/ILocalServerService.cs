using System.Collections.Generic;
using System.ServiceModel;
using ServiceEntities;
using System;
using ServiceEntities.CentralService.v3;

namespace CentralService.v3
{
    [ServiceContract]
    public interface ILocalServerService
    {
        [OperationContract]
        ServiceEntities.CentralService.v2.Updates Update(DateTime lastUpdate, ServiceEntities.CentralService.v2.ServerInfo info);

        [OperationContract]
        void Report(ReportLog reports, ServiceEntities.CentralService.v2.ServerInfo info);
    }
}
