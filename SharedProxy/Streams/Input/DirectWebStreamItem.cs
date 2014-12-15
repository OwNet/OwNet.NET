using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.Services;
using System.Threading;
using SharedProxy.Proxy;

namespace SharedProxy.Streams.Input
{
    public class DirectWebStreamItem : StreamItem
    {
        private bool _initFinished = false;
        private Semaphore _initFinishedSem = new Semaphore(0, 1000);
        private Stream _stream = null;

        public override ProxyEntry.DownloadMethods DownloadMethod
        {
            get { return ProxyEntry.DownloadMethods.FromInternetOverLocalProxy; }
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
                return _stream != null;
            }
        }

        public override SourceTypes Source { get { return SourceTypes.Cache; } }

        public override bool Init(ProxyEntry entry)
        {
            CacheEntry = entry;
            bool success = GetWebStream();
            _initFinished = true;
            _initFinishedSem.Release();

            return success;
        }

        private bool GetWebStream()
        {
            try
            {
                _stream = Helpers.Proxy.HttpGetRetriever.RetrieveFromWeb(CacheEntry);
            }
            catch (Exception ex)
            {
                Controller.WriteException("GetWebStream()", ex.Message);
            }

            return _stream != null;
        }

        public override void Download()
        {
        }

        protected override bool PreparePart(int myPart)
        {
            return myPart == 0 && IsSuccessful;
        }

        protected override Stream GetStreamPart(int myPart, out int nextPart)
        {
            nextPart = -1;
            
            return _stream;
        }

        public override void DisposeItem()
        {
        }

        protected override bool IsLastPart(int part)
        {
            return part >= 0;
        }
    }
}
