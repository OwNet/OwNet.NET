using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CentralRestService
{
    [ServiceContract]
    interface IFeedbackService
    {
        [OperationContract]
        void Send(Stream stream);
    }
}
