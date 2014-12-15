using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceEntities;
using System.IO;
using System.Data.Services;
using SharedProxy.Proxy;

namespace SharedProxy.Streams.Input
{
    public class CacheStreamItem : StreamItem
    {
        private bool _isSuccessful = false;

        public override ProxyEntry.DownloadMethods DownloadMethod
        {
            get { return ProxyEntry.DownloadMethods.FromCacheOverServer; }
        }

        public override bool IsSuccessful
        {
            get { return _isSuccessful; }
        }

        public override SourceTypes Source { get { return SourceTypes.Cache; } }

        public override bool Init(ProxyEntry entry)
        {
            CacheEntry = entry;

            _isSuccessful = CacheEntry.UpdateFromDatabase();

            return _isSuccessful;
        }

        public override void Download()
        {
        }

        protected override bool PreparePart(int myPart)
        {
            return true;
        }

        protected override Stream GetStreamPart(int myPart, out int nextPart)
        {
            string filePath = Controller.GetCacheFilePath(CacheEntry.ID, myPart);
            if (myPart == 0 && CacheEntry.NumFileParts < 0)
            {
                filePath = Controller.GetCacheFilePath(CacheEntry.ID, -1);
            }
            else if (myPart >= CacheEntry.NumFileParts)
            {
                nextPart = -1;
                return null;
            }
            nextPart = myPart + 1;

            if (!File.Exists(filePath))
                throw new DataServiceException(500, "The file could not be found.");
            
            Stream stream = null;
            try
            {
                stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception)
            {
                throw new DataServiceException(500, "The file could not be found.");
            }

            return stream;
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
