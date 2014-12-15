using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.ServiceModel;

namespace SharedProxy.Services.Host
{
    public class Service
    {
        private bool IsOpen = false;
        private WebServiceHost WSHost { get; set; }
        private ServiceEndpoint SEndpoint { get; set; }
        private ServiceDebugBehavior SDBehavior { get; set; }

        public static Service CreateService(Type service, Type iservice, string baseUri, string relativeUri)
        {
            Service serv = new Service();
            serv.WSHost = new WebServiceHost(service, new Uri(baseUri + relativeUri));
            serv.SEndpoint = serv.WSHost.AddServiceEndpoint(iservice, new WebHttpBinding(), "");
            serv.SDBehavior = serv.WSHost.Description.Behaviors.Find<ServiceDebugBehavior>();
            serv.SDBehavior.HttpsHelpPageEnabled = false;
            return serv;
        }

        public void Start()
        {
            if (IsOpen == false)
            {
                WSHost.Open();
            }
            IsOpen = true;
        }

        public void Stop()
        {
            if (IsOpen == true)
            {
                WSHost.Close();
            }
            IsOpen = false;
        }
    }
}
