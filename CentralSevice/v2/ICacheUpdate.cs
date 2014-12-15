using System.Collections.Generic;
using System.ServiceModel;
using ServiceEntities;
using System;
using ServiceEntities.CentralService.v2;

namespace CentralService.v2
{
    [ServiceContract]
    public interface ICacheUpdate
    {
        [OperationContract]
        Updates GetLinksToUpdate(DateTime lastUpdate, ServerInfo info);

        [OperationContract]
        void Report(ReportLog reports, ServerInfo info);
    }
}
