using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ServiceEntities;
using SharedProxy.Proxy;

namespace SharedProxy.Streams.Input
{
    public abstract class StreamItem
    {
        public enum SourceTypes
        {
            Internet,
            LocalNetwork,
            Cache
        }

        private int _numReaders = 0;
        private object _numReadersLock = new object();

        protected Dictionary<int, int> _clientIndices = new Dictionary<int, int>();

        private ProxyEntry _cacheEntry = null;

        protected bool _isSavedToCache = false;

        protected bool _isReferenced = true;

        public ProxyEntry CacheEntry
        {
            get { return _cacheEntry; }
            set { _cacheEntry = value; _cacheEntry.DownloadedFrom = DownloadMethod; }
        }

        public bool IsReferenced { get { return _isReferenced; } set { _isReferenced = value; } }

        public bool IsSavedToCache
        {
            get { return _isSavedToCache || Source == SourceTypes.Cache; }
            set { _isSavedToCache = value; }
        }

        public abstract SourceTypes Source { get; }

        public bool IsErrorPage = false;

        public string AbsoluteUri { get { return _cacheEntry.AbsoluteUri; } }

        public int UriHash { get { return Helpers.Proxy.ProxyCache.GetUriHash(AbsoluteUri); } }

        public abstract bool IsSuccessful { get; }

        public abstract ProxyEntry.DownloadMethods DownloadMethod { get; }

        public bool IsUsed { get { return _clientIndices.Count > 0; } }

        public abstract bool Init(ProxyEntry request);

        public Stream GetStream(int readerId)
        {
            Stream stream = null;
            int nextPart = -1;

            if (_clientIndices.ContainsKey(readerId))
            {
                int myPart;
                myPart = _clientIndices[readerId];
                try
                {
                    if (PreparePart(myPart))
                    {
                        lock (_clientIndices)
                        {
                            stream = GetStreamPart(myPart, out nextPart);
                            _clientIndices[readerId] = nextPart;
                        }
                    }
                    else
                        _clientIndices[readerId] = nextPart;
                }
                catch (Exception ex)
                {
                    ClientAndServerShared.LogsController.WriteException("GetStream", ex.Message);
                }
                finally
                {
                    if (stream == null)
                        DisposeReader(readerId);
                }
            }
            return stream;
        }

        protected abstract bool PreparePart(int myPart);

        protected abstract Stream GetStreamPart(int myPart, out int nextPart);

        public abstract void Download();

        public abstract void DisposeItem();

        public bool IsLast(int readerId)
        {
            bool isLast = true;
            if (_clientIndices.ContainsKey(readerId))
                isLast = IsLastPart(_clientIndices[readerId]);
            return isLast;
        }

        protected abstract bool IsLastPart(int part);

        public int RegisterReader()
        {
            int readerId = 0;
            lock (_numReadersLock)
                readerId = _numReaders++;
            lock (_clientIndices)
                _clientIndices[readerId] = 0;

            return readerId;
        }

        public void DisposeReader(int id)
        {
            lock (_clientIndices)
            {
                _clientIndices.Remove(id);

                if (_clientIndices.Count <= 0)
                    ProxyStreamManager.DisposeStreamItem(this);
            }
        }

        public void DisposeAllReaders()
        {
            lock (_clientIndices)
                _clientIndices.Clear();
            ProxyStreamManager.DisposeStreamItem(this);
        }

        protected List<int> ClientIndicesList()
        {
            lock (_clientIndices)
                return _clientIndices.Values.ToList();
        }
    }
}
