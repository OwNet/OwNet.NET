using System;
using System.Linq;
using SharedProxy.MainServerService;
using System.IO;
using System.Data.Services.Client;
using System.Threading;
using ServiceEntities.Cache;
using SharedProxy.Proxy;

namespace SharedProxy.Streams.Input
{
    public class DataServiceStreamItem : InMemoryStreamItem
    {
        private bool _initFinished = false;
        private Semaphore _initFinishedSem = new Semaphore(0, 1000);

        ServiceItem _serviceItem = null;
        ProxyServiceContextEntities _context = null;
        private bool _success = false;

        public override ProxyEntry.DownloadMethods DownloadMethod
        {
            get { return ProxyEntry.DownloadMethods.FromCacheOverServer; }
        }

        public override bool IsSuccessful
        {
            get
            {
                if (!_initFinished)
                {
                    _initFinishedSem.WaitOne();
                    _initFinishedSem.Release();
                }
                return _success;
            }
        }

        public override SourceTypes Source { get { return SourceTypes.LocalNetwork; } }

        public DataServiceStreamItem() : base() { }

        public override bool Init(ProxyEntry entry)
        {
            CacheEntry = entry;
            _success = InitFromService();
            _initFinished = true;
            _initFinishedSem.Release();

            return _success;
        }

        private bool InitFromService()
        {
            var tuple = GetServiceItem(CacheEntry);
            if (tuple == null)
                return false;

            _context = tuple.Item1;
            _serviceItem = tuple.Item2;

            CacheEntry.Update(_serviceItem);
            return true;
        }

        internal static Tuple<ProxyServiceContextEntities, ServiceItem> GetServiceItem(ProxyEntry request)
        {
            CacheCreateResponse cacheResponse = null;
            ProxyServiceContextEntities context = null;
            ServiceItem serviceItem = null;

            try
            {
                cacheResponse = Services.Client.CacheServiceRequester.GetResponse(request);

                if (cacheResponse == null || !cacheResponse.Found || cacheResponse.CacheID < 0)
                    return null;

                string proxyServiceName = cacheResponse.ClientIP == "" ? "proxy" : "clientproxy";

                if (cacheResponse.ClientIP == "::1")
                    cacheResponse.ClientIP = "";

                context = new ProxyServiceContextEntities(new Uri(Controller.ServiceUrl(proxyServiceName, cacheResponse.ClientIP)));

                serviceItem = (from c in context.ProxyServiceItems
                               where c.NoCacheItemId == cacheResponse.CacheID
                               select c).FirstOrDefault();
            }
            catch (Exception e)
            {
                ClientAndServerShared.LogsController.WriteException("GetCache()", e.Message);
                return null;
            }

            if (serviceItem == null || serviceItem.AbsoluteUri == null)
                return null;

            return new Tuple<ProxyServiceContextEntities,ServiceItem>(context, serviceItem);
        }

        public override void Download()
        {
            if (!IsSuccessful)
            {
                EndOfAllParts();
                return;
            }

            try
            {
                DataServiceStreamResponse response;
                byte[] buffer = new byte[WebStreamItem.MaxPartSize];
                int read;
                bool empty;

                while ((response = _context.GetReadStream(_serviceItem)) != null)
                {
                    Stream readStream = response.Stream;
                    if (readStream == null)
                        break;

                    empty = true;
                    while ((read = readStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        InsertNewPart(read, buffer);
                        buffer = new byte[WebStreamItem.MaxPartSize];
                        empty = false;
                    }

                    readStream.Close();
                    response.Dispose();

                    if (empty)
                        break;
                }
            }
            catch (Exception ex)
            {
                Controller.WriteException("Read response", ex.Message);
            }
            finally
            {
                EndOfAllParts();
            }
        }
    }
}
