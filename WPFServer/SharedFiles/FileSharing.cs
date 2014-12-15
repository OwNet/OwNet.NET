using System;
using System.Linq;
using WPFServer.DatabaseContext;
using ClientAndServerShared;

namespace WPFServer.SharedFiles
{
    class FileSharing
    {
        private static string _sharedFilePath { get { return Server.Settings.AppDataFolder() + @"\Documents"; } }

        public static string GetSharedFilePath(int fileId)
        {
            return _sharedFilePath + "\\" + Convert.ToString(fileId) + ".ownshare";
        }

        public static ServiceEntities.SharedFolder GetFolderStructure(MyDBContext con)
        {
            ServiceEntities.SharedFolder rootItem = new ServiceEntities.SharedFolder();

            SharedFolder root = GetRootFolderItem(con);
            AssignChildFoldersToItem(root, rootItem);

            return rootItem;
        }

        private static void AssignChildFoldersToItem(SharedFolder dbItem, ServiceEntities.SharedFolder sharedFolder)
        {
            sharedFolder.Name = dbItem.Name;
            sharedFolder.Id = dbItem.Id;
            sharedFolder.ParentFolderId = dbItem.ParentFolderId ?? -1;

            if (dbItem.ChildFolders != null)
            {
                if (sharedFolder.ChildFolders == null)
                    sharedFolder.ChildFolders = new ServiceEntities.SharedFolderList();

                foreach (SharedFolder childDbItem in dbItem.ChildFolders.OrderBy(f => f.Name))
                {
                    ServiceEntities.SharedFolder child = new ServiceEntities.SharedFolder();
                    sharedFolder.ChildFolders.Add(child);
                    AssignChildFoldersToItem(childDbItem, child);
                }
            }

            if (dbItem.SharedFiles != null)
            {
                foreach (SharedFile fileDbItem in dbItem.SharedFiles.OrderBy(f => f.Title))
                {
                    if (sharedFolder.Files == null)
                        sharedFolder.Files = new ServiceEntities.SharedFileList();

                    sharedFolder.Files.Add(new ServiceEntities.SharedFile()
                    {
                        SharedFolderId = dbItem.Id,
                        FileObjectId = fileDbItem.SharedFileObject.Id,
                        Title = (String.IsNullOrWhiteSpace(fileDbItem.Title)) ? fileDbItem.FileName : fileDbItem.Title,
                        Description = fileDbItem.Description,
                        UserId = fileDbItem.User.Id,
                        Created = fileDbItem.DateCreated,
                        FileName = fileDbItem.FileName
                    });
                }
            }
        }

        public static SharedFolder GetRootFolderItem(MyDBContext con)
        {
            SharedFolder root = null;
            var parentFolders = con.Fetch<SharedFolder>(i => i.ParentFolderId == null);
            if (parentFolders.Any())
            {
                root = parentFolders.First();
            }
            else
            {
                root = new SharedFolder()
                {
                    Name = "OwNet",
                    DateCreated = DateTime.Now
                };
                con.FetchSet<SharedFolder>().Add(root);
                con.SaveChanges();
            }

            return root;
        }

        public static void DeleteFolder(SharedFolder folder, MyDBContext con)
        {
            foreach (SharedFolder childFolder in folder.ChildFolders.ToArray())
            {
                DeleteFolder(childFolder, con);
            }

            foreach (SharedFile file in folder.SharedFiles.ToArray())
            {
                DeleteFile(file, con);
            }

            con.Remove<SharedFolder>(folder);
        }

        public static void DeleteFile(SharedFile sharedFile, MyDBContext con)
        {
            SharedFileObject file = sharedFile.SharedFileObject;
            if (file != null)
            {
                foreach (Activity activity in sharedFile.Activities.ToArray())
                {
                    con.FetchSet<Activity>().Remove(activity);
                }
            }
            RemoveFileFromDisk(file.Id);
            con.Remove<SharedFile>(sharedFile);
        }

        public static ServiceEntities.SharedFolder GetFolder(int id, MyDBContext con)
        {
            ServiceEntities.SharedFolder folder = null;
            try
            {
                SharedFolder folderItem = null;
                if (id == 0)
                    folderItem = GetRootFolderItem(con);
                else
                {
                    var folders = con.Fetch<SharedFolder>(i => i.Id == id);
                    if (folders.Any())
                    {
                        folderItem = folders.First();
                    }
                }

                if (folderItem != null)
                {
                    folder = new ServiceEntities.SharedFolder()
                    {
                        Name = folderItem.Name,
                        Id = folderItem.Id
                    };
                    folder.Files = new ServiceEntities.SharedFileList();
                    if (folderItem.SharedFiles != null)
                    {
                        foreach (SharedFile fileItem in folderItem.SharedFiles)
                        {
                            folder.Files.Add(new ServiceEntities.SharedFile()
                            {
                                Description = fileItem.Description,
                                Title = (String.IsNullOrWhiteSpace(fileItem.Title)) ? fileItem.FileName : fileItem.Title,
                                FileObjectId = fileItem.SharedFileObject.Id,
                                UserId = fileItem.User.Id,
                                FileName = fileItem.FileName,
                                Username = fileItem.User == null ? "" : fileItem.User.Username,
                                IsTeacher = fileItem.User == null ? false : fileItem.User.IsTeacher,
                                Created = fileItem.DateCreated
                            });
                        }
                    }
                    folder.ChildFolders = new ServiceEntities.SharedFolderList();
                    if (folderItem.ChildFolders != null)
                    {
                        foreach (SharedFolder childFolderItem in folderItem.ChildFolders)
                        {
                            folder.ChildFolders.Add(new ServiceEntities.SharedFolder()
                            {
                                Name = childFolderItem.Name,
                                Id = childFolderItem.Id,
                                ParentFolderId = folderItem.Id
                            });
                        }
                    }
                    folder.ParentFolderId = folderItem.ParentFolderId ?? 0;
                    folder.FullPath = GetFolderPath(folderItem);
                }
            }
            catch (Exception ex)
            {
                Server.WriteException("GetFolder", ex.Message);
            }

            return folder;
        }

        public static string GetFolderPath(SharedFolder item)
        {
            if (item == null)
                return "";

            string path = "";
            if (item.ParentFolderId != null)
                path += GetFolderPath(item.ParentFolder);
            else
                return item.Name;

            path += "\\" + item.Name;
            return path;
        }

        private static void RemoveFileFromDisk(int fileId)
        {
            try
            {
                System.IO.FileInfo file = new System.IO.FileInfo(SharedProxy.Controller.GetSharedFilePath(fileId));
                if (file.Exists)
                    file.Delete();
            }
            catch (Exception e)
            {
                LogsController.WriteException("Delete cache", e.Message);
            }
        }
    }
}
