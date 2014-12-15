using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace SharedProxy.Streams.Input
{
    public abstract class InMemoryStreamItem : StreamItem
    {
        protected Dictionary<int, StreamPart> _parts = new Dictionary<int, StreamPart>();
        protected bool _complete = false;
        protected Queue<Semaphore> _nextPartWaiting = new Queue<Semaphore>();

        protected int _partsAvailable = 0;
        protected object _partsAvailableLock = new object();

        private int _firstPartInMemoryIndex = 0;
        private int _clearPartUntil = 0;
        private object _firstPartInMemoryIndexLock = new object();
        private System.Timers.Timer _clearPartsInMemoryTimer = new System.Timers.Timer(1000); // every 1 sec

        public static int MaxPartSize { get { return Helpers.Proxy.ProxyServer.BUFFER_SIZE * 8; } } // 65536 bytes

        public InMemoryStreamItem()
        {
            _clearPartsInMemoryTimer.Elapsed += new System.Timers.ElapsedEventHandler(ClearPartsTimerTick);
        }

        protected override bool PreparePart(int myPart)
        {
            Semaphore sem = null;
            lock (_partsAvailableLock)
            {
                if (_partsAvailable <= myPart && _complete)
                    return false;

                if (_partsAvailable <= myPart)
                {
                    sem = new Semaphore(0, 1);
                    _nextPartWaiting.Enqueue(sem);
                }
            }
            if (sem != null)
                sem.WaitOne();

            return _partsAvailable > myPart;
        }

        protected override Stream GetStreamPart(int myPart, out int nextPart)
        {
            StreamPart part = _parts[myPart];
            nextPart = part.Next;
            if (part.IsFile)
                return File.Open(Controller.GetCacheFilePath(CacheEntry.ID, part.FilePartNumber),
                    FileMode.Open, FileAccess.Read, FileShare.Read);

            return new MemoryStream(part.Bytes, 0, part.Size);
        }

        public Tuple<int, byte[]> GetBytes(int readerId)
        {
            byte[] bytes = null;
            int size = 0;

            if (_clientIndices.ContainsKey(readerId))
            {
                int myPart = _clientIndices[readerId];
                try
                {
                    if (PreparePart(myPart))
                    {
                        lock (_clientIndices)
                        {
                            StreamPart part = _parts[myPart];
                            if (!part.IsFile)
                            {
                                bytes = part.Bytes;
                                size = part.Size;
                            }
                            _clientIndices[readerId] = part.Next;
                        }
                    }
                    else
                        _clientIndices[readerId] = -1;
                }
                catch (Exception ex)
                {
                    ClientAndServerShared.LogsController.WriteException("GetStream", ex.Message);
                    bytes = null;
                }
            }
            return new Tuple<int,byte[]>(size, bytes);
        }

        protected void InsertNewPart(int size, byte[] bytes)
        {
            lock (_partsAvailableLock)
            {
                _parts[_partsAvailable] = new StreamPart()
                {
                    Bytes = bytes,
                    Size = size,
                    Next = _partsAvailable + 1
                };
                _partsAvailable++;
                while (_nextPartWaiting.Any())
                    _nextPartWaiting.Dequeue().Release();
            }
        }

        public void NewFilePartAvailable(int partNumber, int memoryPartNumber, int numMemoryParts)
        {
            int nextPartIndex = memoryPartNumber + numMemoryParts;
            lock (_partsAvailableLock)
            {
                _parts[memoryPartNumber] =
                    new StreamPart()
                    {
                        IsFile = true,
                        Next = nextPartIndex,
                        FilePartNumber = partNumber
                    };
            }
            ClearPartsUntil(nextPartIndex);
        }

        private void ClearPartsUntil(int partUntil)
        {
            lock (_firstPartInMemoryIndexLock)
            {
                if (_clearPartUntil < partUntil)
                {
                    _clearPartUntil = partUntil;
                    if (!_clearPartsInMemoryTimer.Enabled)
                        _clearPartsInMemoryTimer.Enabled = true;
                }
            }
        }

        private void ClearPartsTimerTick(object sender, EventArgs e)
        {
            _clearPartsInMemoryTimer.Enabled = false;

            lock (_firstPartInMemoryIndexLock)
            {
                List<int> indices = ClientIndicesList();
                for (; _firstPartInMemoryIndex < _clearPartUntil; ++_firstPartInMemoryIndex)
                {
                    if (_parts.ContainsKey(_firstPartInMemoryIndex))
                    {
                        if (!_parts[_firstPartInMemoryIndex].IsFile)
                        {
                            if (indices.Contains(_firstPartInMemoryIndex))
                            {
                                _clearPartsInMemoryTimer.Enabled = true;
                                break;
                            }
                            _parts.Remove(_firstPartInMemoryIndex);
                        }
                    }
                }
            }
        }

        protected void EndOfAllParts()
        {
            lock (_partsAvailableLock)
            {
                _complete = true;

                while (_nextPartWaiting.Any())
                    _nextPartWaiting.Dequeue().Release();

                _nextPartWaiting.Clear();
            }
        }

        public override void DisposeItem()
        {
        }

        protected override bool IsLastPart(int part)
        {
            return _complete && (part >= _partsAvailable - 1);
        }

        protected class StreamPart
        {
            public byte[] Bytes = null;
            public int Next = 0;
            public int Size = 0;
            public bool IsFile = false;
            public int FilePartNumber = -1;
        }
    }
}
