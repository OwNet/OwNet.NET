using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SharedProxy.MainServerService;
using System.IO;
using System.Data.Services.Client;
using SharedProxy.Proxy;

namespace SharedProxy.Streams.Input
{
    class DirectDataServiceStreamItem : StreamItem
    {
        private bool _initFinished = false;
        private Semaphore _initFinishedSem = new Semaphore(0, 1000);

        DataServiceStreamResponse _response = null;

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
            var tuple = DataServiceStreamItem.GetServiceItem(CacheEntry);
            if (tuple == null)
                return false;

            _context = tuple.Item1;
            _serviceItem = tuple.Item2;

            CacheEntry.Update(_serviceItem);
            return true;
        }

        public override void Download()
        { }

        protected override bool PreparePart(int myPart)
        {
            return myPart >= 0 && IsSuccessful;
        }

        protected override Stream GetStreamPart(int myPart, out int nextPart)
        {
            Stream stream = null;
            nextPart = myPart + 1;

            try
            {
                if (_response != null)
                    _response.Dispose();

                if ((_response = _context.GetReadStream(_serviceItem)) == null)
                    nextPart = -1;
                else
                    stream = _response.Stream;
            }
            catch (Exception ex)
            {
                Controller.WriteException("Get stream", ex.Message);
                nextPart = -1;
            }
            return stream;
        }

        public override void DisposeItem()
        {
            if (_response != null)
                _response.Dispose();
            _response = null;
        }

        protected override bool IsLastPart(int part)
        {
            return false;
        }
    }
}
