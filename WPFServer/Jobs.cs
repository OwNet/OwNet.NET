using System.Timers;
using WPFServer.CentralService;

namespace WPFServer
{
    class Jobs
    {
        private static Timer _timerReportToCentralServer = null;
        private static Timer _timerCheckAvailableClients = null;
        private static Timer _timerPrepareCacheServiceResonses = null;
        private static Timer _timerGetLinksToUpdate = null;
        private static Timer _timerCleanCache = null;
        private static Timer _timerUpdateCache = null;
        private static Timer _timerPrefetchLinks = null;
        private static Timer _timerDeleteOutdatedCache = null;

        public static void StartJobs()
        {
            /* posielanie logov */
            _timerReportToCentralServer = new System.Timers.Timer(2 * 60 * 1000); // Every 2 mins
            _timerReportToCentralServer.Elapsed += new System.Timers.ElapsedEventHandler(ReportToCentralServer);
            _timerReportToCentralServer.Enabled = true;

            _timerCheckAvailableClients = new Timer(30 * 1000); // 30 secs
            _timerCheckAvailableClients.Elapsed += new System.Timers.ElapsedEventHandler(CheckAvailableClients);
            _timerCheckAvailableClients.Enabled = true;

            _timerPrepareCacheServiceResonses = new Timer(100); // Every 0.1 secs
            _timerPrepareCacheServiceResonses.Elapsed += new System.Timers.ElapsedEventHandler(PrepareCacheServiceResponses);
            _timerPrepareCacheServiceResonses.Enabled = true;

            /* ziskavanie updatov z central service */
            _timerGetLinksToUpdate = new System.Timers.Timer(5 * 60 * 1000); // Every 5 mins
            _timerGetLinksToUpdate.Elapsed += new System.Timers.ElapsedEventHandler(GetLinksToUpdate);
            _timerGetLinksToUpdate.Enabled = true;

            _timerCleanCache = new System.Timers.Timer(3 * 60 * 1000); // Every 3 mins
            _timerCleanCache.Elapsed += new System.Timers.ElapsedEventHandler(CleanCache);
            _timerCleanCache.Enabled = true;

            _timerUpdateCache = new System.Timers.Timer(3 * 60 * 1000); // Every 3 mins
            _timerUpdateCache.Elapsed += new System.Timers.ElapsedEventHandler(UpdateCache);
            _timerUpdateCache.Enabled = true;

            _timerDeleteOutdatedCache = new Timer(1 * 60 * 1000); // Every minute
            _timerDeleteOutdatedCache.Elapsed += new ElapsedEventHandler(DeleteOutdatedCache);
            _timerDeleteOutdatedCache.Enabled = true;

            if (Helpers.AudioMixerHelper.SetVolume(0))
                StartPrefetching();

            SharedProxy.Jobs.StartJobs();

            Multicasts.ServerInfoMulticastSender.Start();
        }

        private static void StartPrefetching()
        {
            _timerPrefetchLinks = new System.Timers.Timer(2 * 60 * 1000); // Every 2 mins
            _timerPrefetchLinks.Elapsed += new System.Timers.ElapsedEventHandler(PrefetchLinks);
            _timerPrefetchLinks.Enabled = true;
        }

        public static void EndJobs()
        {
            if (_timerReportToCentralServer != null)
                _timerReportToCentralServer.Enabled = false;

            if (_timerCheckAvailableClients != null)
                _timerCheckAvailableClients.Enabled = false;

            if (_timerGetLinksToUpdate != null)
                _timerGetLinksToUpdate.Enabled = false;

            if (_timerCleanCache != null)
                _timerCleanCache.Enabled = false;

            if (_timerUpdateCache != null)
                _timerUpdateCache.Enabled = false;

            if (_timerPrefetchLinks != null)
                _timerPrefetchLinks.Enabled = false;

            if (_timerDeleteOutdatedCache != null)
                _timerDeleteOutdatedCache.Enabled = false;

            SharedProxy.Jobs.EndJobs();

            Multicasts.ServerInfoMulticastSender.Stop();
        }

        public static void ReportToCentralServer()
        {
            ReportToCentralServer(null, null);
        }

        private static void ReportToCentralServer(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_timerReportToCentralServer != null)
                _timerReportToCentralServer.Stop();

            CentralServiceCommunicator.ReportToCentralServer();

            if (_timerReportToCentralServer != null)
                _timerReportToCentralServer.Start();
        }

        private static void CheckAvailableClients(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timerCheckAvailableClients.Stop();
            WPFServer.Clients.ClientsController.CheckAvailableClients();
            _timerCheckAvailableClients.Start();
        }

        private static void PrepareCacheServiceResponses(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timerPrepareCacheServiceResonses.Stop();
            Proxy.RequestProcessing.PrepareResponses();
            _timerPrepareCacheServiceResonses.Start();
        }

        public static void GetLinksToUpdate()
        {
            GetLinksToUpdate(null, null);
        }

        private static void GetLinksToUpdate(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_timerGetLinksToUpdate != null)
                _timerGetLinksToUpdate.Stop();

            CentralService.CacheUpdates.GetLinksToUpdate();

            if (_timerGetLinksToUpdate != null)
                _timerGetLinksToUpdate.Start();
        }

        private static void CleanCache(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timerCleanCache.Stop();
            WPFServer.Cache.CacheMaintainer.CleanCache();
            _timerCleanCache.Start();
        }

        private static void UpdateCache(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timerUpdateCache.Stop();
            WPFServer.Cache.CacheMaintainer.UpdateCache();
            _timerUpdateCache.Start();
        }

        private static void PrefetchLinks(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timerPrefetchLinks.Stop();
            bool work = WPFServer.Prefetching.Prefetcher.Start();
            if (work == false)
            {
                if (_timerPrefetchLinks.Interval <= 32 * 60 * 1000)
                {
                    _timerPrefetchLinks.Interval *= 2;
                    _timerPrefetchLinks.Start();
                }
                else
                {
                    ResetTimerPrefetchLinks();
                }
            }
        }

        public static void ResetTimerPrefetchLinks()
        {
            _timerPrefetchLinks.Interval = 2 * 60 * 1000;
            if (_timerPrefetchLinks.Enabled == false)
            {
                _timerPrefetchLinks.Start();
            }
        }

        private static void DeleteOutdatedCache(object sender, ElapsedEventArgs e)
        {
            _timerDeleteOutdatedCache.Stop();
            WPFServer.Cache.CacheMaintainer.DeleteOutdatedCache();
            _timerDeleteOutdatedCache.Start();
        }
    }
}
