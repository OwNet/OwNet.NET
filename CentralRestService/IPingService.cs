using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web;

namespace CentralRestService
{
    [ServiceContract]
    interface IPingService
    {
        [OperationContract]
        Message Hey(string workspaceId);
    }
}