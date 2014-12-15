using System;
using WPFServer.CentralService;

namespace WPFServer
{
    public class AppSettings : Helpers.IAppSettings
    {
        private static Version _version = new Version(0, 1, 9);
        private static Version _myDBContextVersion = new Version(0, 1, 1);
        private static Version _proxyDataVersion = new Version(0, 1, 1);

        public static Version Version { get { return _version; } }
        public static string VersionString { get { return _version.ToString(3); } }
        public static string MyDBContextVersionString { get { return _myDBContextVersion.ToString(3); } }
        public static string ProxyDataVersionString { get { return _proxyDataVersion.ToString(3); } }
        public static string SearchDatabaseVersionString { get { return ClientAndServerShared.SearchDatabase.DatabaseVersionString; } }
        public static string ServerClientName { get { return Properties.Settings.Default.CentralServerUsername; } }

        public AppSettings()
        {
            Properties.Settings.Default.AppVersion = VersionString;
            Properties.Settings.Default.MyDBContextVersion = MyDBContextVersionString;
            Properties.Settings.Default.ProxyDataVersion = ProxyDataVersionString;
            Properties.Settings.Default.SearchDatabaseVersion = SearchDatabaseVersionString;
            Properties.Settings.Default.Save();
        }

        public long MaximumCacheSize()
        {
            return (long)Properties.Settings.Default.MaximumCacheSize * Helpers.Common.BytesInMB;
        }

        public string CentralServerUsername()
        {
            if (Properties.Settings.Default.CentralServerUsername == "")
                CentralServiceCommunicator.Register();

            return Properties.Settings.Default.CentralServerUsername;
        }

        public string CentralServerPassword()
        {
            return Properties.Settings.Default.CentralServerPassword;
        }

        public string AppDataFolder()
        {
            return Properties.Settings.Default.AppDataFolder;
        }

        public string SearchDatabaseVersion()
        {
            return Properties.Settings.Default.SearchDatabaseVersion;
        }

        public string ServerName()
        {
            return Properties.Settings.Default.ServerName;
        }

        public string ServerIP()
        {
            return "localhost";
        }

        public string ClientName()
        {
            return Properties.Settings.Default.CentralServerUsername;
        }

        public bool DownloadEverythingThroughServer()
        {
            return false;
        }
    }
}
