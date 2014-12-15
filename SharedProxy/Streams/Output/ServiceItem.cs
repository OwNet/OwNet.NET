using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Services.Common;
using SharedProxy.Streams.Input;
using SharedProxy.Services.Host;
using System.IO;
using SharedProxy.Proxy;

namespace SharedProxy.Streams.Output
{
    [DataServiceKeyAttribute("NoCacheItemId")]
    [HasStreamAttribute]
    public partial class ServiceItem
    {
        public int NoCacheItemId { get; set; }
        public string AbsoluteUri { get; set; }
        public string ResponseHeaders { get; set; }
        public DateTime? Expires { get; set; }
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public string ContentType { get; set; }
        public string CharacterSet { get; set; }
        public bool Used = false;
        public int DownloadMethod { get; set; }
        public DateTime DateModified { get; set; }

        private StreamItem _streamItem = null;
        private int _readerId = -1;
        private ServiceItemTimer _timeout = null;

        public ServiceItem()
        {
            _timeout = new ServiceItemTimer(60000);
            _timeout.ServiceItem = this;
            _timeout.Elapsed += new System.Timers.ElapsedEventHandler(DisposeTimerElapsed);
        }

        public bool CreateStreamItem(ProxyEntry entry, bool downloadIfNotFound)
        {
            int downloadMethod;
            _streamItem = ProxyStreamManager.CreateStreamItem(entry, out downloadMethod, out _readerId, true, !downloadIfNotFound);

            if (_streamItem == null)
                return false;

            if (!_streamItem.IsSuccessful)
            {
                DisposeStreamItem();
                return false;
            }

            _streamItem.CacheEntry.UpdateProxyServiceItem(this);
            DownloadMethod = downloadMethod;
            _timeout.Start();

            return true;
        }

        public void SetResponseHeaders(List<Tuple<string, string>> headers)
        {
            if (headers == null)
                return;

            StringBuilder builder = new StringBuilder();
            foreach (Tuple<String, String> pair in headers)
            {
                builder.Append(pair.Item1).Append("|||").Append(pair.Item2).Append("<->");
            }
            ResponseHeaders = builder.ToString();
            ResponseHeaders = System.Text.RegularExpressions.Regex.Replace(ResponseHeaders, "<->$", "");
        }

        public void DisposeStreamItem()
        {
            _timeout.Stop();
            if (_streamItem != null)
                _streamItem.DisposeReader(_readerId);

            _streamItem = null;
            _readerId = -1;
        }

        public Stream GetNextPart()
        {
            if (_streamItem == null)
                return null;

            _timeout.Stop();

            Stream stream = _streamItem.GetStream(_readerId);
            if (stream == null)
                ProxyServiceContextEntities.DisposeItem(this);
            else
                _timeout.Start();

            return stream;
        }

        private static void DisposeTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ServiceItemTimer timer = (ServiceItemTimer)sender;
            ProxyServiceContextEntities.DisposeItem(timer.ServiceItem);
        }
    }
}
