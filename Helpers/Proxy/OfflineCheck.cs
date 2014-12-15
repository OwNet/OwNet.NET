using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;

namespace Helpers.Proxy
{
    public class OfflineCheck
    {
        private static bool _isOffline = false;
        private static DateTime _lastChecked = DateTime.Now.AddHours(-1);

        public static bool IsOffline
        {
            get
            {
                if ((DateTime.Now - _lastChecked).TotalSeconds > 10.0)
                {
                    _isOffline = !PingInternet();
                    _lastChecked = DateTime.Now;
                }
                else if (_isOffline)
                {
                    _lastChecked = DateTime.Now;
                }
                return _isOffline;
            }
            set
            {
                _isOffline = value;
                _lastChecked = DateTime.Now;
            }
        }

        private static bool PingInternet()
        {
            try
            {
                PingReply reply = new Ping().Send("www.google.com");

                return reply.Status == IPStatus.Success;
            }
            catch (Exception ex)
            {
                Messages.WriteException("Ping", ex.Message);
            }
            return false;
        }
    }
}
