using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharedProxy.Streams.Input;
using SharedProxy.Proxy;

namespace SharedProxy.Streams.Output
{
    public class CacheWriter
    {
        private InMemoryStreamItem _streamItem = null;
        private bool _update = false;
        private int _readerId = 0;

        public string AbsoluteUri { get { return _streamItem.AbsoluteUri; } }

        public ProxyEntry Entry { get { return _streamItem.CacheEntry; } }

        public static int NumMemoryPartsInFilePart { get { return 500; } }
        public static int CacheFilePartSize { get { return InMemoryStreamItem.MaxPartSize * NumMemoryPartsInFilePart; } }

        public CacheWriter(InMemoryStreamItem item, bool update, int readerId)
        {
            _streamItem = item;
            _update = update;
            _readerId = readerId;
        }

        public void Save()
        {
            if (!_streamItem.IsSuccessful || _streamItem.IsErrorPage)
            {
                _streamItem.DisposeReader(_readerId);
                return;
            }

            try
            {
                int hash = Helpers.Proxy.ProxyCache.GetUriHash(_streamItem.AbsoluteUri);
                int cacheFilePartSize = CacheFilePartSize;
                int readComplete = 0, filePart = 0, readMemoryPartsComplete = 0;
                bool endOfParts = false;

                for (filePart = 0; !endOfParts; filePart++)
                {
                    string filePath = Controller.GetCacheFilePath(hash, filePart);
                    int readFilePart = 0;

                    if (filePart == 0)
                    {
                        FileInfo fileInfo = new System.IO.FileInfo(filePath);
                        fileInfo.Directory.Create();
                    }

                    int i = 0;
                    using (FileStream fs = File.Open(filePath, FileMode.Create))
                    {
                        for (i = 0; i < NumMemoryPartsInFilePart; ++i)
                        {
                            var tuple = _streamItem.GetBytes(_readerId);
                            if (tuple.Item2 == null)
                            {
                                endOfParts = true;
                                break;
                            }

                            fs.Write(tuple.Item2, 0, tuple.Item1);
                            readFilePart += tuple.Item1;
                        }
                    }
                    readComplete += readFilePart;
                    _streamItem.NewFilePartAvailable(filePart, readMemoryPartsComplete, i);
                    readMemoryPartsComplete += i;
                }
                Entry.Size = readComplete;
                Entry.NumFileParts = filePart;
                Entry.UpdateInDatabase(this);
            }
            catch (Exception ex)
            {
                Controller.WriteException("Cache stream save", ex.Message);
                Dispose();
            }
        }

        public void Dispose()
        {
            _streamItem.DisposeReader(_readerId);
        }
    }
}
