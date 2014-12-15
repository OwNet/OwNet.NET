using System;
using System.Collections.Generic;
using WPFServer.DatabaseContext;

namespace WPFServer.Clients
{
    class ClientsController
    {
        static Dictionary<string, Client> _onlineClients = new Dictionary<string, Client>();

        public static bool IsOnline(string clientName)
        {
            if (clientName == AppSettings.ServerClientName ||
                (_onlineClients.ContainsKey(clientName) &&
                    (DateTime.Now - _onlineClients[clientName].LastSeen).TotalSeconds < 60))
                return true;

            return false;
        }

        public static void ClientIsOnline(string clientName, string ip)
        {
            if (clientName == AppSettings.ServerClientName)
                return;

            Client client = null;

            if (_onlineClients.ContainsKey(clientName))
                client = _onlineClients[clientName];
            else
            {
                client = new Client();
                _onlineClients[clientName] = client;
            }
            
            client.LastSeen = DateTime.Now;
            client.IP = ip;
        }

        public static string GetClientIP(string clientName)
        {
            if (_onlineClients.ContainsKey(clientName))
                return _onlineClients[clientName].IP;
            return "";
        }

        class Client
        {
            public DateTime LastSeen { get; set; }
            public string IP { get; set; }
        }

        internal static void ClientIsOffline(string clientName)
        {
            if (_onlineClients.ContainsKey(clientName))
                _onlineClients.Remove(clientName);
        }

        internal static void CheckAvailableClients()
        {
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    var clients = con.FetchSet<DatabaseContext.Client>();

                    foreach (var client in clients)
                    {
                        if (IsOnline(client.ClientName))
                            continue;

                        bool ret = false;
                        try
                        {
                            ret = SharedProxy.ServerRequestManager.Get<bool>(new Uri("clientcache/ping", UriKind.Relative),
                                null, client.LastIP);
                        }
                        catch (Exception)
                        {
                        }
                        if (ret)
                            ClientIsOnline(client.ClientName, client.LastIP);
                    }
                }
            }
            catch (Exception e)
            {
                Server.WriteException("CheckAvailableClients", e.Message);
            }
        }

        internal static List<string> GetAvailableClients()
        {
            List<string> clients = new List<string>();
            clients.Add(AppSettings.ServerClientName);

            foreach (var pair in _onlineClients)
            {
                if (IsOnline(pair.Key))
                    clients.Add(pair.Key);
            }

            return clients;
        }
    }
}
