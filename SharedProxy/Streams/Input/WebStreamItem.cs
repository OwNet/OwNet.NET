using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Net;
using ServiceEntities;
using SharedProxy.Proxy;

namespace SharedProxy.Streams.Input
{
    public class WebStreamItem : InMemoryStreamItem
    {
        private Stream _stream = null;
        private bool _initFinished = false;
        private Semaphore _initFinishedSem = new Semaphore(0, 1000);
        
        public override ProxyEntry.DownloadMethods DownloadMethod
        {
            get { return ProxyEntry.DownloadMethods.FromInternetOverServer; }
        }

        public override bool IsSuccessful {
            get {
                if (!_initFinished)
                {
                    _initFinishedSem.WaitOne();
                    _initFinishedSem.Release();
                }
                return _stream != null;
            }
        }

        public override SourceTypes Source { get { return SourceTypes.Internet; } }

        public WebStreamItem() : base() { }

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
            if (_stream == null)
            {
                EndOfAllParts();
                return;
            }

            try
            {
                byte[] buffer = new byte[WebStreamItem.MaxPartSize];
                int read;
                while ((read = _stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    InsertNewPart(read, buffer);
                    buffer = new byte[WebStreamItem.MaxPartSize];
                }
                _stream.Close();
            }
            catch (Exception ex)
            {
                Controller.WriteException("Downloading response", ex.Message);
            }
            finally
            {
                EndOfAllParts();
            }
        }
    }
}
