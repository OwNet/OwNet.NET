using System;
using System.Collections.Generic;
using System.Threading;
using CentralServerShared;
using ServiceEntities;
using ServiceEntities.CentralService.v2;
using System.Linq;

namespace CentralService
{
    public static class Controller
    {
        private static Dictionary<Tuple<string, string>, ServerController> _serverControllers = new Dictionary<Tuple<string, string>, ServerController>();
        private static object _serverControllersLock = new object();

        static Controller()
        {
            MessageWriter writer = new MessageWriter();
            CentralServerShared.Controller.MessageWriter = writer;
        }

        public static ServerController GetServerController(ServerInfo info)
        {
            Tuple<string, string> dictKey = new Tuple<string, string>(info.Username, info.Password);

            lock (_serverControllersLock)
            {
                if (!_serverControllers.ContainsKey(dictKey))
                {
                    int serverId = ServerAuthentication.Authenticate(info);
                    _serverControllers[dictKey] = new ServerController(serverId);
                }
            }

            return _serverControllers[dictKey];
        }

        public static void WriteException(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public static void WriteException(string header, string message)
        {
            System.Diagnostics.Debug.WriteLine(header + ": " + message);
        }

        public static void ReceivedAccessLogsFromServer(List<AccessLogReport> reports, ServerController serverController)
        {
            serverController.LastReports = reports;

            ThreadPool.QueueUserWorkItem(new WaitCallback(ReceivedAccessLogsFromServer), serverController);
        }

        private static void ReceivedAccessLogsFromServer(Object obj)
        {
            ServerController serverController = (ServerController)obj;

            serverController.ReceivedAccessLogs();
        }

        public static List<ServiceEntities.CentralService.v2.ServerInfo> GetAllServerInfos()
        {
            var serverInfos = new List<ServiceEntities.CentralService.v2.ServerInfo>();

            try
            {
                using (var con = new DataModelContainer())
                {
                    var servers = con.Servers.OrderBy(s => s.ServerName);
                    foreach (var server in servers)
                    {
                        serverInfos.Add(new ServerInfo()
                        {
                            Username = server.Username,
                            ServerName = server.ServerName
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Controller.WriteException("GetAllServerInfos", ex.Message);
            }

            return serverInfos;
        }
    }
}