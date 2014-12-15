using System.ServiceModel;

namespace SharedProxy.Services.Host
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IProxyDataService" in both code and config file together.
    [ServiceContract]
    public interface IProxyDataService
    {
        [OperationContract]
        void DoWork();
    }
}
