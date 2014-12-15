using ClientAndServerShared;

namespace WPFCentralServer
{
    public class Controller
    {
        public static void Init()
        {
            LogsController.Init();
        }

        public static void WriteException(string location, string message)
        {
            LogsController.WriteException(location, message);
        }

        public static void Start()
        {
            Jobs.StartJobs();
            LogsController.WriteMessage("Started");
        }

        public static void Stop()
        {
            Jobs.EndJobs();
            LogsController.WriteMessage("Stopped");
        }
    }
}
