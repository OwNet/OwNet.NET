using System.Timers;
using WPFProxy.Cache;

namespace WPFProxy
{
    class Jobs
    {
        private static Timer _timerDeleteOutdatedCache = null;
        private static Timer _timerCleanCache = null;
        private static Timer _timerReportToServer = null;
        private static Timer _timerPrefetchLinks = null;

        public static void StartJobs()
        {
            _timerDeleteOutdatedCache = new Timer(60 * 1000); // Every minute
            _timerDeleteOutdatedCache.Elapsed += new ElapsedEventHandler(DeleteOutdatedCache);
            _timerDeleteOutdatedCache.Enabled = true;

            _timerCleanCache = new System.Timers.Timer(5 * 60 * 1000); // Every 5 mins
            _timerCleanCache.Elapsed += new System.Timers.ElapsedEventHandler(CleanCache);
            _timerCleanCache.Enabled = true;

            _timerReportToServer = new Timer(20 * 1000); // Every 20 secs
            _timerReportToServer.Elapsed += new ElapsedEventHandler(ReportToServer);
            _timerReportToServer.Enabled = true;

            if (Helpers.AudioMixerHelper.SetVolume(0))
                StartPrefetching();

            SharedProxy.Jobs.StartJobs();
        }

        private static void StartPrefetching()
        {
            _timerPrefetchLinks = new Timer(1 * 60 * 1000); // Every 1 minute
            _timerPrefetchLinks.Elapsed += new ElapsedEventHandler(PrefetchLinks);
            _timerPrefetchLinks.Enabled = true;
        }

        public static void EndJobs()
        {
            if (_timerDeleteOutdatedCache != null)
                _timerDeleteOutdatedCache.Enabled = false;

            if (_timerCleanCache != null)
                _timerCleanCache.Enabled = false;

            if (_timerReportToServer != null)
                _timerReportToServer.Enabled = false;
        }

        private static void DeleteOutdatedCache(object sender, ElapsedEventArgs e)
        {
            _timerDeleteOutdatedCache.Stop();
            CacheMaintainer.DeleteOutdatedCache();
            _timerDeleteOutdatedCache.Start();
        }

        private static void CleanCache(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timerCleanCache.Stop();
            CacheMaintainer.CleanCache();
            _timerCleanCache.Start();
        }

        private static void ReportToServer(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timerReportToServer.Stop();
            CacheReporter.ReportToServer();
            _timerReportToServer.Start();
        }

        private static void PrefetchLinks(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timerPrefetchLinks.Stop();
            bool work = Prefetcher.Start();
            if (work == false)
            {
                if (_timerPrefetchLinks.Interval <= 5 * 60 * 1000)
                {
                    _timerPrefetchLinks.Interval += 30 * 1000;
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
            _timerPrefetchLinks.Interval = 1 * 15 * 1000; //1 * 60 * 1000;
            if (_timerPrefetchLinks.Enabled == false)
            {
                _timerPrefetchLinks.Start();
            }
        }
    }
}
