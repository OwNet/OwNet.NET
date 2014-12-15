using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralServiceCore.Data;

namespace CentralServiceCore.WebTrackerCommunication
{
    public class WebTrackerReporter
    {
        public static int NumAccessesNeededToTrack = 2;
        public static int AccessLogsTimeFrameInHours = 336;

        static object _lock = new object();

        public static void ReportWebsitesToTrack()
        {
            ClientRegistration.RegisterClient();

            lock (_lock)
            {
                var container = DataController.Container;
                var webObjects = ListUrlsToTrack(container);
                if (webObjects.Count() == 0)
                    return;

                List<string> urls = new List<string>();
                foreach (var webObject in webObjects)
                    urls.Add(webObject.AbsoluteUri);

                try
                {
                    var client = new WebTrackerTrackingService.TrackingServiceClient();
                    client.TrackWebsites(urls.ToArray(), new WebTrackerTrackingService.ClientType()
                    {
                        ClientId = Properties.Settings.Default.WebTrackerClientId,
                        Code = Properties.Settings.Default.WebTrackerClientPassword
                    });

                    foreach (var webObject in webObjects)
                        webObject.IsTracked = true;
                    container.SaveChanges();
                }
                catch (Exception e)
                { }
            }
        }

        public static void ReportWebsitesToUntrack()
        {
            lock (_lock)
            {
                var container = DataController.Container;
                var webObjects = ListUrlsToUntrack(container);
                if (webObjects.Count() == 0)
                    return;

                List<string> urls = new List<string>();
                foreach (var webObject in webObjects)
                    urls.Add(webObject.AbsoluteUri);

                try
                {
                    var client = new WebTrackerTrackingService.TrackingServiceClient();
                    client.UntrackWebsites(urls.ToArray(), new WebTrackerTrackingService.ClientType()
                    {
                        ClientId = Properties.Settings.Default.WebTrackerClientId,
                        Code = Properties.Settings.Default.WebTrackerClientPassword
                    });

                    foreach (var webObject in webObjects)
                        webObject.IsTracked = false;
                    container.SaveChanges();
                }
                catch (Exception e)
                { }
            }
        }

        public static IEnumerable<WebObject> ListUrlsToTrack(ICentralDataModelContainer container)
        {
            var accessLogsSince = DateTime.Now.AddHours(0 - AccessLogsTimeFrameInHours);

            return from o in container.WebObjects
                    where !o.IsTracked &&
                        o.AccessLogs.Where(a => a.AccessedAt > accessLogsSince).Count() >= NumAccessesNeededToTrack
                    select o;
        }

        public static IEnumerable<WebObject> ListUrlsToUntrack(ICentralDataModelContainer container)
        {
            var accessLogsSince = DateTime.Now.AddHours(0 - AccessLogsTimeFrameInHours);

            return from o in container.WebObjects
                   where o.IsTracked &&
                    o.AccessLogs.Where(a => a.AccessedAt > accessLogsSince).Count() < NumAccessesNeededToTrack
                   select o;
        }
    }
}
