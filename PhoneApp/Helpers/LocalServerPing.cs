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
using System.Text.RegularExpressions;

namespace PhoneApp.Helpers
{
    public class LocalServerPing : NotifierObject
    {
        public void Ping(string address)
        {
            try
            {
                WebClient serviceRequest = new WebClient();
                serviceRequest.DownloadStringAsync(new Uri(Controllers.ServersController.GetServiceAddress("cache/server_name", address)));
                serviceRequest.DownloadStringCompleted += new DownloadStringCompletedEventHandler(serviceRequest_DownloadStringCompleted);
            }
            catch (Exception ex)
            {
                Controllers.LogsController.WriteException("Ping", ex.Message);
                NotifyFailed();
            }
        }

        void serviceRequest_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                string res = e.Result;
                string serverName = Regex.Replace(res, @"<[^>]*>", String.Empty);
                if (serverName == Controllers.UsersController.AuthenticatedUser.ServerUsername)
                {
                    NotifySuccess();
                    return;
                }
            }
            catch (Exception ex)
            {
                Controllers.LogsController.WriteException("PingResult", ex.Message);
            }
            NotifyFailed();
        }
    }
}
