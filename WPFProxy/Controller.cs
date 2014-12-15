using System;
using System.Data.EntityClient;
using System.Threading;
using ClientAndServerShared;
using SharedProxy.Services.Host;
using WPFProxy.Cache;
using WPFProxy.Database;
using WPFProxy.Proxy;
using System.Data.SqlServerCe;

namespace WPFProxy
{
    public sealed class Controller
    {
        private static string _appExecutableFolder = System.AppDomain.CurrentDomain.BaseDirectory;
        private static bool _proxyIsRunning = false;

        public static string AppDataFolder
        {
            get
            {
                if (Properties.Settings.Default.AppDataFolder == "")
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    path += @"\FIIT\OwNet\Client";
                    Properties.Settings.Default.AppDataFolder = path;
                    Properties.Settings.Default.Save();
                }
                return Properties.Settings.Default.AppDataFolder;
            }
        }
        public static string AppExecutableFolder
        {
            get { return _appExecutableFolder; }
        }
        public static string CacheFolder
        {
            get { return String.Format("{0}\\Caches", Controller.AppDataFolder); }
        }
        public static bool UseDataService
        {
            get { return UseServer && Properties.Settings.Default.CacheOnServer; }
        }
        public static bool UseServer
        {
            get { return Properties.Settings.Default.UseServer; }
        }
        public static string ServiceBaseUrl
        {
            get { return "http://" + Properties.Settings.Default.ServerIP + ":57116/"; }
        }
        public static bool DoNotCache
        {
            get { return Properties.Settings.Default.DoNotCache; }
            set
            {
                Properties.Settings.Default.DoNotCache = value;
                Properties.Settings.Default.Save();
            }
        }
        public static string DatabasePath
        {
            get { return GetDatabasePath(Settings.DatabaseVersionString); }
        }
        public static bool ProxyIsRunning
        {
            get { return _proxyIsRunning; }
        }
        public static bool DoNotCacheHtml
        {
            get { return Properties.Settings.Default.DoNotCacheHtml; }
        }

        public static void Init()
        {
            LogsController.Init();

            // Copy or update database to AppData folder
            if (!System.IO.File.Exists(DatabasePath))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(AppDataFolder);

                    SqlCeEngine en = new SqlCeEngine(Controller.GetDatabaseConnectionString());
                    en.CreateDatabase();
                    en.Dispose();

                    using (var con = new SqlCeConnection(Controller.GetDatabaseConnectionString()))
                    {
                        con.Open();
                        foreach (string query in Database.DatabaseInitializer.InitialSchema())
                        {
                            using (SqlCeCommand cmd = con.CreateCommand())
                            {
                                cmd.CommandText = query;
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogsController.WriteException("Init", ex.Message);
                }
            }

            // ProxyInstance
            SharedProxy.Controller.ProxyInstance = new ProxyInstance();

            // AppSettings
            AppSettings settings = new AppSettings();
            SharedProxy.Controller.AppSettings = settings;
            ClientAndServerShared.ClientAndServerController.AppSettings = settings;

            // SearchDatabase
            SearchDatabase.VirtualSearchDatabaseObject = new SearchDatabase.VirtualSearchDatabase();

            // CacheEntrySaver
            SharedProxy.Proxy.ProxyEntry.Saver = new CacheEntrySaver();

            // Init
            SharedProxy.Controller.Init();
            Settings.Init();
            Cache.CachingExceptions.Init();

            // Update settings
            Properties.Settings.Default.AppVersion = Settings.VersionString;
            Properties.Settings.Default.DatabaseVersion = Settings.DatabaseVersionString;
            Properties.Settings.Default.SearchDatabaseVersion = SearchDatabase.DatabaseVersionString;
            if (Properties.Settings.Default.ClientName == "")
                Properties.Settings.Default.ClientName = Helpers.Common.RandomString(12, false);
            Properties.Settings.Default.Save();

            CancelEnabledActions.StartAction(LaunchDataService, false, "Starting services...");
            
            StartProxy();
        }

        private static bool LaunchDataService(CancelObject cancelObject)
        {
            ServiceHost.LaunchDataService("http://localhost:57116/");
            return true;
        }

        private static void StartProxy()
        {
            new Thread(new ThreadStart(StartProxyThread)).Start();
            Jobs.StartJobs();
            _proxyIsRunning = true;
            LogsController.WriteMessage("Proxy is running.");
        }

        private static string GetDatabasePath(string version)
        {
            return String.Format("{0}\\Database_v{1}.sdf", AppDataFolder, version);
        }

        internal static string GetDatabaseConnectionString()
        {
            return string.Format("Data Source={0};Persist Security Info=False;", DatabasePath);
        }

        internal static DatabaseEntities GetDatabase()
        {
            var builder = new EntityConnectionStringBuilder();
            builder.Metadata = "res://*/Database.Database.csdl|res://*/Database.Database.ssdl|res://*/Database.Database.msl";
            builder.Provider = "System.Data.SqlServerCe.4.0";
            builder.ProviderConnectionString = string.Format("Data Source={0}", DatabasePath);
            return new DatabaseEntities(builder.ToString());
        }

        public static bool SubmitDatabaseChanges(DatabaseEntities database)
        {
            try
            {
                database.SaveChanges();
                
            }
            catch (Exception e)
            {
                LogsController.WriteException("SubmitDatabaseChanges()", "Query failed:" + e.ToString());
                return false;
            }
            return true;
        }

        private static void StartProxyThread()
        {
            try
            {
                ProxyServer.Server.Start();
            }
            catch (Exception)
            {
            }
        }

        public static string GetCacheFilePath(string url, int part)
        {
            return SharedProxy.Controller.GetCacheFilePath(ProxyCache.GetUriHash(url), part);
        }

        public static string ServiceUrl(string relativeUrl)
        {
            return Controller.ServiceBaseUrl + relativeUrl;
        }

        public static void Closing()
        {
        }

        public static string GetAppResourcePath(string relativePath)
        {
            return System.IO.Path.Combine(AppExecutableFolder, relativePath);
        }
    }
}
