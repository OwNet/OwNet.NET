using System;
using System.Data.Services.Client;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ClientAndServerShared;
using ServiceEntities;
using WPFProxy.Proxy;
using SharedProxy.MainServerService;

namespace WPFProxy
{
    class SharedDocumentsUploader
    {
        static string _filePath;
        static string _title;
        static string _description;
        static SharedFolder _folder;
        static SharedFile _file;
        static object _uploadLock = new object();

        public static SharedFile UploadFile(string filePath, string title, string description, SharedFolder folder)
        {
            if (!Controller.UseServer || !Settings.IsLoggedIn() || folder == null)
                return null;

            lock (_uploadLock)
            {
                _filePath = filePath;
                _title = title;
                _description = description;
                _folder = folder;

                if (CancelEnabledActions.StartAction(UploadFileAction, false))
                    return _file;

                return null;
            }
        }

        public static bool UploadFileAction(CancelObject cancelObject)
        {
            FileStream fs = null;
            try
            {
                fs = File.OpenRead(_filePath);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
            if (fs == null) return false;

            bool success = true;
            SharedFile file = null;

            SharedFilesService.MyDBContext context = new SharedFilesService.MyDBContext(new Uri(Controller.ServiceUrl("sharedfiles")));
            context.MergeOption = MergeOption.PreserveChanges;

            SharedFilesService.SharedFileObject sharedFileObject = new SharedFilesService.SharedFileObject();
            sharedFileObject.FileName = _filePath;
            sharedFileObject.ContentType = ProxyServer.GetContentType(_filePath);
            sharedFileObject.DateCreated = DateTime.Now;
            sharedFileObject.Id = Helpers.SharedFiles.CreateSharedFileId(sharedFileObject.FileName);

            context.AddToSharedFileObjects(sharedFileObject);

            try
            {
                context.SetSaveStream(sharedFileObject, fs, true, sharedFileObject.ContentType, _filePath + "|" + Convert.ToString(sharedFileObject.Id));

                DataServiceResponse response = context.SaveChanges();

                foreach (ChangeOperationResponse change in response)
                {
                    EntityDescriptor descriptor = change.Descriptor as EntityDescriptor;

                    if (descriptor != null)
                    {
                        sharedFileObject = descriptor.Entity as SharedFilesService.SharedFileObject;

                        if (sharedFileObject != null)
                        {
                            file = new SharedFile()
                            {
                                Title = _title,
                                Description = _description,
                                FileObjectId = sharedFileObject.Id,
                                SharedFolderId = _folder.Id,
                                FileName = new FileInfo(_filePath).Name
                            };
                            file.User.Id = Settings.UserID;

                            success = ServiceCommunicator.SendSharedFile(file);

                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogsController.WriteException("Upload file", e.Message);
                return false;
            }
            _file = file;
            return true;
        }

        public static SharedFolder CreateFolder(SharedFolder parent)
        {
            SharedFolder folder = null;
            UploadDocsCreateFolderWindow createWindow = new UploadDocsCreateFolderWindow();
            if (createWindow.ShowDialog() ?? false)
            {
                string folderName = createWindow.ResponseText;
                if (folderName == "")
                {
                    AppHelpers.ShowError("The entered folder name is empty.");
                    return null;
                }
                folder = ServiceCommunicator.CreateFolder(parent.Id, folderName);
                
            }
            return folder;
        }

        public static bool RenameFolder(SharedFolder folder)
        {
            bool success = false;
            UploadDocsCreateFolderWindow createWindow = new UploadDocsCreateFolderWindow();
            createWindow.ResponseText = folder.Name;

            if (createWindow.ShowDialog() ?? false)
            {
                string folderName = createWindow.ResponseText;
                if (folderName == "")
                {
                    AppHelpers.ShowError("The entered folder name is empty.");
                    return false;
                }
                else
                    folder.Name = folderName;
                success = ServiceCommunicator.RenameFolder(folder.Id, folderName);
            }
            return success;
        }

        public static bool RemoveFolder(SharedFolder folder)
        {
            return ServiceCommunicator.DeleteFolder(folder.Id);
        }

        public static bool RemoveFile(SharedFile file)
        {
            return ServiceCommunicator.DeleteFile(file.FileObjectId);
        }
    }
}
