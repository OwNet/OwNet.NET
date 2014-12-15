using System;
using System.Data.Services;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace WPFServer.SharedFiles
{
    public class ProxyDataServiceHost
    {
        private static DataServiceHost _sharedFilesHost = null;

        public static void LaunchDataService(string baseAddress)
        {
            _sharedFilesHost = new DataServiceHost(typeof(SharedFilesDataService), new Uri[] {});
            _sharedFilesHost.AddServiceEndpoint(new ServiceEndpoint(ContractDescription.GetContract(typeof(SharedFilesDataService)))
            {
                Name = "default",
                Address = new EndpointAddress(baseAddress + "sharedfiles/"),
                Contract = ContractDescription.GetContract(typeof(IRequestHandler)),
                Binding = new WebHttpBinding()
                {
                    MaxReceivedMessageSize = 2147483647
                }
            });
            _sharedFilesHost.Open();

            Jobs.StartJobs();
        }

        public static void CloseDataService()
        {
            if (_sharedFilesHost != null)
                _sharedFilesHost.Close();
            _sharedFilesHost = null;

            Jobs.EndJobs();
        }
    }
}
