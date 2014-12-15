using System.Timers;

namespace WPFCentralServer
{
    public class Jobs
    {
        private static Timer _timerDownloadPlannedUpdates = null;
        private static Timer _timerPrefetchForUpdates = null;

        public static void StartJobs()
        {
            _timerDownloadPlannedUpdates = new System.Timers.Timer(10 * 60 * 1000); // Every 10 mins
            _timerDownloadPlannedUpdates.Elapsed += new System.Timers.ElapsedEventHandler(DownloadPlannedUpdates);
            _timerDownloadPlannedUpdates.Enabled = true;

            _timerPrefetchForUpdates = new System.Timers.Timer(13 * 60 * 1000); // Start in 13 mins
            _timerPrefetchForUpdates.Elapsed += new System.Timers.ElapsedEventHandler(PrefetchForUpdates);
            _timerPrefetchForUpdates.Enabled = true;
        }

        public static void EndJobs()
        {
            if (_timerDownloadPlannedUpdates != null)
                _timerDownloadPlannedUpdates.Enabled = false;
        }

        private static void DownloadPlannedUpdates(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timerDownloadPlannedUpdates.Stop();
            CacheUpdater.DownloadPlannedUpdates();
            _timerDownloadPlannedUpdates.Start();
        }
        private static void PrefetchForUpdates(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timerPrefetchForUpdates.Stop();
            _timerPrefetchForUpdates = new System.Timers.Timer(600000); // Continue every 10 mins
            _timerPrefetchForUpdates.Enabled = false;
            _timerPrefetchForUpdates.Elapsed += new System.Timers.ElapsedEventHandler(PrefetchForUpdates);
            Prefetcher.PrefetchForPlannedUpdates();
            _timerPrefetchForUpdates.Start();
        }
    }
}