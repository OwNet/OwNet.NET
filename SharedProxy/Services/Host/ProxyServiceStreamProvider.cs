using System;
using System.Data.Services;
using System.Data.Services.Providers;
using System.IO;
using SharedProxy.Streams.Output;

namespace SharedProxy.Services.Host
{
    public class ProxyServiceStreamProvider : IDataServiceStreamProvider, IDisposable
    {
        private ProxyServiceContextEntities _context;
        private ServiceItem _serviceItem = null;

        public ProxyServiceStreamProvider(ProxyServiceContextEntities context)
        {
            this._context = context;
        }

        #region IDataServiceStreamProvider Members

        public void DeleteStream(object entity, DataServiceOperationContext operationContext)
        {
            _serviceItem = entity as ServiceItem;
            Dispose();
        }

        public Stream GetReadStream(object entity, string etag, bool?
            checkETagForEquality, DataServiceOperationContext operationContext)
        {
            if (checkETagForEquality != null)
            {
                // This stream provider implementation does not support
                // ETag headers for media resources. This means that we do not track
                // concurrency for a media resource and last-in wins on updates.
                throw new DataServiceException(400,
                    "This sample service does not support the ETag header for a media resource.");
            }

            ServiceItem item = entity as ServiceItem;
            _serviceItem = item;
            if (item == null)
                throw new DataServiceException(500, "Internal Server Error.");

            Stream stream = item.GetNextPart();
            if (stream == null)
            {
                return new MemoryStream();
            }

            return stream;
        }

        public Uri GetReadStreamUri(object entity, DataServiceOperationContext operationContext)
        {
            // Allow the runtime set the URI of the Media Resource.
            return null;
        }

        public string GetStreamContentType(object entity, DataServiceOperationContext operationContext)
        {
            ServiceItem item = entity as ServiceItem;
            if (item == null)
            {
                throw new DataServiceException(500, "Internal Server Error.");
            }

            if (item.ContentType == null || item.ContentType == "")
                return "application/x-unknown";
            return item.ContentType;
        }

        public string GetStreamETag(object entity, DataServiceOperationContext operationContext)
        {
            return null;
        }

        public Stream GetWriteStream(object entity, string etag, bool?
            checkETagForEquality, DataServiceOperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public string ResolveType(string entitySetName, DataServiceOperationContext operationContext)
        {
            if (entitySetName == "ServiceItem")
            {
                return "SharedProxy.Streams.Output.ServiceItem";
            }
            else
            {
                // This will raise an DataServiceException.
                return null;
            }
        }

        public int StreamBufferSize
        {
            // Use a buffer size of 64K bytes. 
            get { return 64000; }
        }

        #endregion

        public void Dispose()
        { }
    }
}
