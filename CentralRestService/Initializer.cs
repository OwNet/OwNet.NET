using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;

namespace CentralRestService
{
    public class Initializer
    {
        internal static void InitRestServices()
        {
            RouteTable.Routes.Add(new ServiceRoute("Ping", new WebServiceHostFactory(), typeof(PingService)));
            RouteTable.Routes.Add(new ServiceRoute("Sync", new WebServiceHostFactory(), typeof(SyncService)));
            RouteTable.Routes.Add(new ServiceRoute("Feedback", new WebServiceHostFactory(), typeof(FeedbackService)));
        }
    }
}