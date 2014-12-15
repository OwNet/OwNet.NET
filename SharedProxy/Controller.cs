using System;
using ClientAndServerShared;
using Helpers;
using SharedProxy.Proxy;

namespace SharedProxy
{
    public class Controller
    {
        private static IAppSettings _appSettings = null;
        private static IProxyInstance _proxyInstance = null;
        //        private static string _cacheFilePath = HostingEnvironment.MapPath("~/App_Data");
        private static string _appDataPath { get { return AppSettings.AppDataFolder(); } }
        private static string _cacheFilePath { get { return _appDataPath + @"\Caches"; } }
        private static string _sharedFilePath { get { return _appDataPath + @"\Documents"; } }
        private static string _reportFilePath { get { return _appDataPath + @"\Reports"; } }

        public static string CacheFolderPath { get { return _cacheFilePath; } }

        public static bool IsOnServer = false;

        public static IAppSettings AppSettings
        {
            get { return _appSettings; }
            set { _appSettings = value; }
        }

        public static IProxyInstance ProxyInstance
        {
            get { return _proxyInstance; }
            set { _proxyInstance = value; }
        }

        public static string ServiceBaseUrl(string ip = "")
        {
            return "http://" + (ip == "" ? AppSettings.ServerIP() : ip) + ":57116/";
        }

        public static string ServiceUrl(string relativeUrl, string ip = "")
        {
            return Controller.ServiceBaseUrl(ip) + relativeUrl;
        }

        public static void WriteException(string message)
        {
            LogsController.WriteException(message);
        }

        public static void WriteException(string location, string message)
        {
            LogsController.WriteException(location, message);
        }

        public static string GetSharedFilePath(int fileId)
        {
            return _sharedFilePath + "\\" + Convert.ToString(fileId) + ".ownshare";
        }

        public static string GetCacheFilePath(string url, int part)
        {
            return GetCacheFilePath(Helpers.Proxy.ProxyCache.GetUriHash(url), part);
        }

        public static string GetCacheFilePath(int hash, int part)
        {
            if (part < 0)
                return string.Format("{0}\\{1}.cache", _cacheFilePath, hash);

            return string.Format("{0}\\{1}-{2}.cache", _cacheFilePath, hash, part);
        }

        public static string GetReportTempFilePath(string filename)
        {
            return _reportFilePath + "\\" + filename + ".csv";
        }

        public static string GetReportZipFilePath(string filename)
        {
            return _reportFilePath + "\\" + filename + ".zip";
        }

        public static void Init()
        {
            if (!System.IO.Directory.Exists(_appDataPath))
                System.IO.Directory.CreateDirectory(_appDataPath);
            if (!System.IO.Directory.Exists(_cacheFilePath))
                System.IO.Directory.CreateDirectory(_cacheFilePath);
            if (!System.IO.Directory.Exists(_sharedFilePath))
                System.IO.Directory.CreateDirectory(_sharedFilePath);
            if (!System.IO.Directory.Exists(_reportFilePath))
                System.IO.Directory.CreateDirectory(_reportFilePath);

            SearchDatabase.Init(AppSettings.SearchDatabaseVersion());
            SharedProxy.Cache.GDSFClock.Init();
            Helpers.Messages.MessageWriter = new MessageWriter();
        }

        //public static StringBuilder DumpLogs()
        //{
        //    StringBuilder ret = null;

        //    try
        //    {
        //        ProxyDataContextEntities context = new ProxyDataContextEntities();
        //        var logs = from p in context.AccessLogSet select p;
        //        ret = new StringBuilder();
        //        foreach (var log in logs)
        //        {
        //            ret.AppendLine(String.Format("{0};{1};{2};{3};{4};{5};",log.Id,log.AccessedAt,log.CacheId, log.DownloadedFrom, log.FetchDuration, log.AbsoluteUri));
        //        }
        //        context.ExecuteStoreCommand("DELETE FROM AccessLogSet");
        //        context.Dispose();
        //        return ret;
        //    }
        //    catch (Exception e)
        //    {
        //        Controller.WriteException(e.Message);
        //    }

        //    return ret;
        //}

        public static void UpdateCache(string url, DateTime ifOlderThan)
        {
            try
            {
                ProxyEntry entry = ProxyInstance.CreateCacheEntry(url);
                entry.Refresh = false;
                entry.RefreshIfOlderThan = ifOlderThan;
                entry.CanCache = (int)(ProxyEntry.CanCacheOptions.CanCacheOnClient | SharedProxy.Proxy.ProxyEntry.CanCacheOptions.CanCacheOnServer);
                int downloadMethod = 0;
                int readerId = -1;
                var item = Streams.ProxyStreamManager.CreateStreamItem(entry, out downloadMethod, out readerId, false);
                item.DisposeReader(readerId);
            }
            catch (Exception ex)
            {
                Controller.WriteException("Update cache", ex.Message);
            }
        }
    }
}