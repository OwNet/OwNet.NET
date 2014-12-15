
namespace WPFProxy
{
    class AppSettings : Helpers.IAppSettings
    {
        public long MaximumCacheSize()
        {
            return Properties.Settings.Default.MaximumCacheSize;
        }

        public string CentralServerUsername()
        {
            return "";
        }

        public string CentralServerPassword()
        {
            return "";
        }

        public string AppDataFolder()
        {
            return Controller.AppDataFolder;
        }

        public string SearchDatabaseVersion()
        {
            return SearchDatabase.DatabaseVersionString;
        }

        public string ServerName()
        {
            return "";
        }

        public string ServerIP()
        {
            return Properties.Settings.Default.ServerIP;
        }

        public string ClientName()
        {
            return Settings.ClientName;
        }

        public bool DownloadEverythingThroughServer()
        {
            return Settings.DownloadEverythingThroughServer;
        }
    }
}
