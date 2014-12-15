using System;
using System.Data.Services;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace SharedProxy.Services.Host
{
    public class ServiceHost
    {
        private static DataServiceHost _proxyHost = null;
        private static Service _cacheService = null;

        public static void LaunchDataService(string baseAddress)
        {
            string proxyServiceName = Controller.IsOnServer ? "proxy/" : "clientproxy/";
            _proxyHost = new DataServiceHost(typeof(ProxyService), new Uri[1] { new Uri(baseAddress + proxyServiceName) });
            _proxyHost.Open();

            if (!Controller.IsOnServer)
            {
                _cacheService = Service.CreateService(typeof(ClientCacheService), typeof(IClientCacheService), baseAddress, "clientcache/");
                _cacheService.Start();
            }

            Jobs.StartJobs();
        }

        public static void CloseDataService()
        {
            if (_proxyHost != null)
                _proxyHost.Close();
            _proxyHost = null;

            if (_cacheService != null)
                _cacheService.Stop();
            _cacheService = null;

            Jobs.EndJobs();
        }
    }
}
