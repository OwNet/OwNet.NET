using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CentralServiceClient
{
    public class Jobs
    {
        private static Timer _timerReportToWebTracker = null;

        public static void StartJobs()
        {
            _timerReportToWebTracker = new System.Timers.Timer(30 * 1000); // Every 0.5 min
            _timerReportToWebTracker.Elapsed += new System.Timers.ElapsedEventHandler(DownloadPlannedUpdates);
            _timerReportToWebTracker.Enabled = true;
        }

        public static void EndJobs()
        {
            if (_timerReportToWebTracker != null)
                _timerReportToWebTracker.Enabled = false;
        }

        private static void DownloadPlannedUpdates(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timerReportToWebTracker.Stop();
            CentralServiceCore.WebTrackerCommunication.WebTrackerReporter.ReportWebsitesToTrack();
            CentralServiceCore.WebTrackerCommunication.WebTrackerReporter.ReportWebsitesToUntrack();
            CentralServiceCore.WebTrackerCommunication.WebTrackerUpdater.Update();
            _timerReportToWebTracker.Start();
        }
    }
}
