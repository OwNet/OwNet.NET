using System.Timers;
using SharedProxy.Cache;
using SharedProxy.Proxy;

namespace SharedProxy
{
    public class Jobs
    {
        private static Timer _timerSaveAccessCount = null;
        private static Timer _timerTrafficSnapshots = null;
        private static Timer _timerRequestCacheService = null;
        private static Timer _timerSaveCacheEntries = null;

        private static bool _started = false;
        private static object _startedLock = new object();

        public static void StartJobs()
        {
            lock (_startedLock)
            {
                if (_started)
                    return;
                _started = true;
            }

            _timerSaveAccessCount = new System.Timers.Timer(5000); // Every 5 secs
            _timerSaveAccessCount.Elapsed += new System.Timers.ElapsedEventHandler(SaveAccessCount);
            _timerSaveAccessCount.Enabled = true;

            _timerTrafficSnapshots = new System.Timers.Timer(5000); // Every 5 secs
            _timerTrafficSnapshots.Elapsed += new System.Timers.ElapsedEventHandler(TakeTrafficSnapshot);
            _timerTrafficSnapshots.Enabled = true;

            _timerRequestCacheService = new System.Timers.Timer(100); // Every 0.1 secs
            _timerRequestCacheService.Elapsed += new System.Timers.ElapsedEventHandler(RequestCacheService);
            _timerRequestCacheService.Enabled = true;

            _timerSaveCacheEntries = new Timer(3000); // Every 3 secs
            _timerSaveCacheEntries.Elapsed += new ElapsedEventHandler(SaveCacheEntries);
            _timerSaveCacheEntries.Enabled = true;
        }

        public static void EndJobs()
        {
            lock (_startedLock)
            {
                if (!_started)
                    return;
                _started = false;
            }

            if (_timerSaveAccessCount != null)
                _timerSaveAccessCount.Enabled = false;

            if (_timerTrafficSnapshots != null)
                _timerTrafficSnapshots.Enabled = false;

            if (_timerSaveCacheEntries != null)
                _timerSaveCacheEntries.Enabled = false;
        }

        private static void SaveAccessCount(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timerSaveAccessCount.Stop();
            GDSFClock.SaveLastClock();
            _timerSaveAccessCount.Start();
        }

        private static void TakeTrafficSnapshot(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timerTrafficSnapshots.Stop();
            ProxyTraffic.TakeCurrentTrafficSnapshot();
            _timerTrafficSnapshots.Start();
        }

        private static void RequestCacheService(object sender, System.Timers.ElapsedEventArgs e)
        {
            Services.Client.CacheServiceRequester.RequestService();
        }

        private static void SaveCacheEntries(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timerSaveCacheEntries.Stop();
            ProxyEntry.Saver.Save();
            _timerSaveCacheEntries.Start();
        }
    }
}
