using System;
using System.Data.Services;
using System.Data.Services.Providers;
using System.IO;
using System.Linq;
using WPFServer.DatabaseContext;

namespace WPFServer.SharedFiles
{
    public class SharedFilesServiceStreamProvider : IDataServiceStreamProvider, IDisposable
    {
        private MyDBContext _context;
        private SharedFileObject _sharedFileObject;
        private string _tempFile;

        public SharedFilesServiceStreamProvider(MyDBContext context)
        {
            this._context = context;
            this._tempFile = Path.GetTempFileName();
        }

        private string GetEntityPath(object entity)
        {
            SharedFileObject sharedFile = entity as SharedFileObject;
            if (sharedFile == null)
                throw new DataServiceException(500, "Internal Server Error.");
            return SharedProxy.Controller.GetSharedFilePath(sharedFile.Id);
        }

        #region IDataServiceStreamProvider Members

        public void DeleteStream(object entity, DataServiceOperationContext operationContext)
        {
            string filePath = GetEntityPath(entity);

            try
            {
                File.Delete(filePath);
            }
            catch (IOException ex)
            {
                throw new DataServiceException("The file could not be found.", ex);
            }
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

            string filePath = GetEntityPath(entity);

            if (!File.Exists(filePath))
                throw new DataServiceException(500, "The file could not be found.");

            FileStream fs;
            try
            {
                fs = new FileStream(filePath, FileMode.Open);
            }
            catch (Exception)
            {
                throw new DataServiceException(500, "The file could not be found.");
            }
            return fs;
        }

        public Uri GetReadStreamUri(object entity, DataServiceOperationContext operationContext)
        {
            // Allow the runtime set the URI of the Media Resource.
            return null;
        }

        public string GetStreamContentType(object entity, DataServiceOperationContext operationContext)
        {
            SharedFileObject file = entity as SharedFileObject;
            if (file == null)
            {
                throw new DataServiceException(500, "Internal Server Error.");
            }

            if (file.ContentType == null || file.ContentType == "")
                return "application/x-unknown";
            return file.ContentType;
        }

        public string GetStreamETag(object entity, DataServiceOperationContext operationContext)
        {
            return null;
        }

        public Stream GetWriteStream(object entity, string etag, bool?
            checkETagForEquality, DataServiceOperationContext operationContext)
        {
            if (checkETagForEquality != null)
            {
                // This stream provider implementation does not support ETags associated with BLOBs. 
                // This means that we do not track concurrency for a media resource  
                // and last-in wins on updates. 
                throw new DataServiceException(400,
                    "This demo does not support ETags associated with BLOBs");
            }

            SharedFileObject file = entity as SharedFileObject;

            if (file == null)
            {
                throw new DataServiceException(500, "Internal Server Error: "
                + "the Media Link Entry could not be determined.");
            }

            // Handle the POST request. 
            if (operationContext.RequestMethod == "POST")
            {
                // Set the file name from the Slug header; if we don't have a  
                // Slug header, just set a temporary name which is overwritten  
                // by the subsequent MERGE request from the client.  
                string[] slugSplit = (operationContext.RequestHeaders["Slug"] ?? "newFile<>0").Split('|');
                file.FileName = slugSplit.FirstOrDefault();
                if (slugSplit.Length > 1)
                    file.Id = Convert.ToInt32(slugSplit[1]);
                else
                    file.Id = Helpers.SharedFiles.CreateSharedFileId(file.FileName);

                file.Id = file.Id;
                file.DateCreated = DateTime.Now;

                // Set the content type, which cannot be null. 
                file.ContentType = operationContext.RequestHeaders["Content-Type"];

                // Cache the current entity to enable us to both create a key based storage file name  
                // and to maintain transactional integrity in the disposer; we do this only for a POST request. 
                _sharedFileObject = file;

                return new FileStream(_tempFile, FileMode.Open);
            }
            // Handle the PUT request 
            else
            {
                // Return a stream to write to an existing file.
                return new FileStream(SharedProxy.Controller.GetSharedFilePath(file.Id),
                    FileMode.Open, FileAccess.Write);
            }
        }

        public string ResolveType(string entitySetName, DataServiceOperationContext operationContext)
        {
            if (entitySetName == "SharedFile")
            {
                return "WcfProxy.SharedFile";
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
        {
            if (_sharedFileObject != null)
            {
                string filePath = SharedProxy.Controller.GetSharedFilePath(_sharedFileObject.Id);
                FileInfo fi = new FileInfo(filePath);
                fi.Directory.Create();
                File.Move(_tempFile, filePath);
                File.Delete(_tempFile);
            }
        }
    }
}