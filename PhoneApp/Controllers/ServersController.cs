using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PhoneApp.Controllers
{
    public class ServersController : Helpers.NotifierObject
    {
        private ObservableCollection<PhoneAppCentralService.ServerInfo> _serverInfos = null;
        private static PhoneAppCentralService.ServerInfo _selectedServer = null;

        private static Dictionary<string, string> _serverAddresses = new Dictionary<string, string>();
        internal static string CurrentServerAddress
        {
            get
            {
                if (UsersController.AuthenticatedUser != null)
                    if (_serverAddresses.ContainsKey(UsersController.AuthenticatedUser.ServerUsername))
                        return _serverAddresses[UsersController.AuthenticatedUser.ServerUsername];
                return "";
            }
        }

        internal ObservableCollection<PhoneAppCentralService.ServerInfo> ServerInfos { get { return _serverInfos; } }
        internal static PhoneAppCentralService.ServerInfo SelectedServer
        {
            get { return _selectedServer; }
            set { _selectedServer = value; }
        }

        public static void Init()
        {
            try
            {
                using (var context = new Database.DatabaseContext(Database.DatabaseContext.DBConnectionString))
                    foreach (var server in context.Servers)
                        _serverAddresses.Add(server.ServerUsername, server.Address);
            }
            catch (Exception ex)
            {
                LogsController.WriteException("InitServers", ex.Message);
            }
        }

        public void GetAllServers()
        {
            try
            {
                var client = new PhoneAppCentralService.PhoneAppServiceClient();
                client.GetAllServerInfosAsync();
                client.GetAllServerInfosCompleted += new EventHandler<PhoneAppCentralService.GetAllServerInfosCompletedEventArgs>(GetAllServersCompleted);
            }
            catch (Exception ex)
            {
                Controllers.LogsController.WriteException("LoadGroups", ex.Message);
            }
        }

        void GetAllServersCompleted(object sender, PhoneAppCentralService.GetAllServerInfosCompletedEventArgs e)
        {
            try
            {
                _serverInfos = e.Result;

                if (_serverInfos.Count > 0)
                    NotifySuccess();
            }
            catch (Exception ex)
            {
                LogsController.WriteException("GetAllServersCompleted", ex.Message);
            }
        }

        internal static void SetServerAddress(string serverName, string serverAddress)
        {
            _serverAddresses[serverName] = serverAddress;
            try
            {
                using (var context = new Database.DatabaseContext(Database.DatabaseContext.DBConnectionString))
                {
                    var server = context.Servers.FirstOrDefault(s => s.ServerUsername == serverName);
                    if (server != null)
                    {
                        server.Address = serverAddress;
                        server.DateModified = DateTime.Now;
                    }
                    else
                    {
                        context.Servers.InsertOnSubmit(new Database.Server()
                        {
                            ServerUsername = serverName,
                            Address = serverAddress,
                            DateCreated = DateTime.Now,
                            DateModified = DateTime.Now
                        });
                    }
                    context.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException("SetServerAddress", ex.Message);
            }
        }

        internal static string GetServiceAddress(string relativeAddress)
        {
            return "http://" + CurrentServerAddress + ":57116/" + relativeAddress;
        }

        internal static string GetServiceAddress(string relativeAddress, string address)
        {
            return "http://" + address + ":57116/" + relativeAddress;
        }

        internal static void ServerAddressInvalid()
        {
            if (UsersController.AuthenticatedUser != null)
            {
                Multicasts.ServerInfoMulticastReceiver.Start();
            }
        }

        public static bool IsServerKnown
        {
            get
            {
                bool isKnown = CurrentServerAddress != "";
                if (!isKnown)
                    Multicasts.ServerInfoMulticastReceiver.Start();
                return isKnown;
            }
        }
    }
}
