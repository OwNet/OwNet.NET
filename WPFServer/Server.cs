using System;
using System.Collections.Generic;
using System.Data.Entity;
using ClientAndServerShared;
using SharedProxy.Services.Host;

namespace WPFServer
{
    public interface IServerView
    {
        void RequireTeacher();
    }

    public static class Server
    {
        public static IServerView ServerView = null;
        public static AppSettings Settings { get; set; }

        static Server()
        {
            SharedProxy.Controller.IsOnServer = true;

            // AppSettings
            AppSettings settings = new AppSettings();
            Settings = settings;
            ClientAndServerShared.ClientAndServerController.AppSettings = settings;
            SharedProxy.Controller.AppSettings = settings;

            // ProxyInstance
            SharedProxy.Controller.ProxyInstance = new WPFServer.Proxy.ProxyInstance();

            // CacheEntrySaver
            SharedProxy.Proxy.ProxyEntry.Saver = new Cache.CacheEntrySaver();

            // SearchDatabase
            SearchDatabase.VirtualSearchDatabaseObject = new Cache.SearchDatabase.VirtualSearchDatabase();
        }

        public static void WriteException(string message)
        {
            LogsController.WriteException(message);
        }

        public static void WriteException(string location, string message)
        {
            LogsController.WriteException(location, message);
        }

        private static string Address = "http://localhost";
        private static string Port = "57116";

        private static List<Service> Services = new List<Service>();

        private static void RegisterService(Type service, Type iservice, string baseUri, string relativeUri)
        {
            Services.Add(Service.CreateService(service, iservice, baseUri, relativeUri));
        }

        private static void StartServices()
        {
            CancelEnabledActions.StartAction(StartServices, false);

            if (new UserService().AnyTeacher() == false)
            {
                ServerView.RequireTeacher();
            }
        }

        private static bool StartServices(CancelObject cancelObject)
        {
            string baseUri = Address + ":" + Port + "/";
            RegisterService(typeof(CacheService), typeof(ICacheService), baseUri, "cache/");
            RegisterService(typeof(ActivityService), typeof(IActivityService), baseUri, "activity/");
            RegisterService(typeof(SearchService), typeof(ISearchService), baseUri, "search/");
            RegisterService(typeof(HistoryService), typeof(IHistoryService), baseUri, "history/");
            RegisterService(typeof(UserService), typeof(IUserService), baseUri, "user/");
            RegisterService(typeof(GroupsService), typeof(IGroupsService), baseUri, "groups/");
            foreach (Service service in Services)
            {
                service.Start();
            }
            SharedProxy.Services.Host.ServiceHost.LaunchDataService(baseUri);
            SharedFiles.ProxyDataServiceHost.LaunchDataService(baseUri);
            Jobs.StartJobs();

            return true;
        }

        private static void StopServices()
        {
            foreach (Service service in Services)
            {
                service.Stop();
            }
            SharedProxy.Services.Host.ServiceHost.CloseDataService();
            SharedFiles.ProxyDataServiceHost.CloseDataService();
            Services.Clear();
            Jobs.EndJobs();
        }

        public static void Init()
        {
            LogsController.Init();
            try
            {
                if (Properties.Settings.Default.AppDataFolder == "")
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    path += @"\FIIT\OwNet\Server";
                    Properties.Settings.Default.AppDataFolder = path;
                    Properties.Settings.Default.Save();
                }
                Database.SetInitializer(
                   new DatabaseContext.MyDBContext.DropCreateInitializer()
                    //new DropCreateDatabaseAlways<MyDBContext>()
                    //new DropCreateDatabaseIfModelChanges<MyDBContext>()
                   );
                SharedProxy.Controller.Init();
            }
            catch (Exception e)
            {
                LogsController.WriteException("Server Init", e.Message, true);
            }
        }

        public static void Start()
        {
            StartServices();
        }

        public static void Stop()
        {
            StopServices();
        }

        public static bool ResetCache()
        {
            StopServices();
            return CancelEnabledActions.StartAction(Cache.CacheMaintainer.DeleteCache, false);
        }
        public static bool ResetContent()
        {
            try
            {
                StopServices();
                DatabaseContext.MyDBContext con = new DatabaseContext.MyDBContext();
                con.Reset();
                con.Dispose();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static string GetAddress()
        {
            System.Net.IPHostEntry host;
            string localIP = "?";
            host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (System.Net.IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }

        public static string GetPort()
        {
            return Port;
        }
    }
}
