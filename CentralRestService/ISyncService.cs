using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CentralRestService
{
    [ServiceContract]
    interface ISyncService
    {
        [OperationContract]
        Message GetSyncState(string workspaceId);

        [OperationContract]
        void ReportUpdates(Stream stream);

        [OperationContract]
        void ReportHistory(Stream stream);

        [OperationContract]
        Message GetUpdates(Stream stream);
    }
}
