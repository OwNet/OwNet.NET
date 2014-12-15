using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ServiceEntities;
using WPFProxy.Proxy;

namespace WPFProxy
{
    /// <summary>
    /// Interaction logic for SharedFilesWindow.xaml
    /// </summary>
    public partial class SharedFilesWindow : Window
    {
        private Dictionary<SharedFolder, SharedFolder> _folderStruture
            = new Dictionary<SharedFolder, SharedFolder>();

        public SharedFilesWindow()
        {
            InitializeComponent();

            LoadFolders();
        }

        private void LoadFolders()
        {
            SharedFolder rootFolder = ServiceCommunicator.GetSharedFolderStructure();
            if (rootFolder == null)
                return;

            CustomTreeViewItem rootItem = new CustomTreeViewItem();
            AddFolderToTree(rootFolder, rootItem);
            treeFolders.Items.Add(rootItem);
            rootItem.IsSelected = true;
            rootItem.IsExpanded = true;
        }

        private void AddFolderToTree(SharedFolder folder, CustomTreeViewItem item)
        {
            item.Folder = folder;

            if (folder.ChildFolders != null)
            {
                foreach (SharedFolder subFolder in folder.ChildFolders)
                {
                    CustomTreeViewItem subItem = new CustomTreeViewItem();
                    _folderStruture.Add(subFolder, folder);
                    AddFolderToTree(subFolder, subItem);
                    item.Items.Add(subItem);
                }
            }

            if (folder.Files != null)
            {
                foreach (SharedFile file in folder.Files)
                {
                    AddFileToTree(file, item);
                }
            }
        }

        private void AddFileToTree(SharedFile file, CustomTreeViewItem item)
        {
            CustomTreeViewItem subItem = new CustomTreeViewItem();
            subItem.File = file;
            item.Items.Add(subItem);
        }

        private CustomTreeViewItem GetSelectedItem()
        {
            return treeFolders.SelectedItem as CustomTreeViewItem;
        }

        private CustomTreeViewItem GetSelectedFolderItem()
        {
            CustomTreeViewItem item = GetSelectedItem();
            if (item != null)
            {
                if (item.Folder == null)
                {
                    item = item.Parent as CustomTreeViewItem;
                    if (item.Folder == null)
                    {
                        AppHelpers.ShowError("No folder selected.");
                        return null;
                    }
                }
            }
            return item;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnAddFolder_Click(object sender, RoutedEventArgs e)
        {

            if (!AppHelpers.IsLoggedInMessage())
                return;

            CustomTreeViewItem item = GetSelectedFolderItem();
            if (item == null)
                return;

            SharedFolder folder = SharedDocumentsUploader.CreateFolder(item.Folder);
            if (folder != null)
            {
                CustomTreeViewItem newItem = new CustomTreeViewItem();
                AddFolderToTree(folder, newItem);
                item.Items.Add(newItem);
                item.IsExpanded = true;
            }
            else
            {
                AppHelpers.ShowError("The folder could not be created.");
            }
        }

        private void treeFolders_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            CustomTreeViewItem item = treeFolders.SelectedItem as CustomTreeViewItem;
            if (item == null)
                return;

            btnRenameFolder.IsEnabled = item.Folder != null;
            btnDeleteFolder.IsEnabled = item.Folder == null || item.Folder.ParentFolderId != -1;
            btnOpen.IsEnabled = item.File != null;
            SharedFolder folder = null;

            if (item.Folder != null)
            {
                folder = item.Folder;
                txtFileDescription.Text = "";
                txtTitle.Content = folder.Name;
            }
            else if (item.File != null)
            {
                CustomTreeViewItem folderItem = GetSelectedFolderItem();
                if (folderItem != null)
                    folder = folderItem.Folder;

                txtFileDescription.Text = item.File.Description;
                txtTitle.Content = item.File.Title;
            }

            if (folder != null)
            {
                List<string> path = new List<string>();
                AddParentToPath(folder, path);
                labelFolderPath.Content = string.Join(@"\", path);
            }
        }

        private void AddParentToPath(SharedFolder folder, List<string> path)
        {
            if (_folderStruture.ContainsKey(folder))
            {
                AddParentToPath(_folderStruture[folder], path);
            }
            path.Add(folder.Name);
        }

        private void btnRenameFolder_Click(object sender, RoutedEventArgs e)
        {
            if (!AppHelpers.IsLoggedInMessage())
                return;

            CustomTreeViewItem item = GetSelectedFolderItem();
            if (item == null)
                return;

            if (SharedDocumentsUploader.RenameFolder(item.Folder))
                item.UpdateHeader(item.Folder.Name);
            else
                AppHelpers.ShowError("Failed to rename the folder.");
        }

        private void btnDeleteFolder_Click(object sender, RoutedEventArgs e)
        {
            if (!AppHelpers.IsLoggedInMessage())
                return;

            CustomTreeViewItem item = GetSelectedItem();
            if (item == null)
                return;

            if ((item.IsFolder && !item.Folder.IsEmpty() && !AppHelpers.IsAuthorizedMessage()) ||
                (!item.IsFolder && !AppHelpers.IsAuthorizedMessage(item.File.UserId)))
                return;

            if (AppHelpers.ShowQuestion(String.Format("Do you really want to delete the {0} named {1}?",
                item.IsFolder ? "folder" : "file",
                item.ItemName), false))
            {
                if (item.IsFolder)
                {
                    if (SharedDocumentsUploader.RemoveFolder(item.Folder))
                    {
                        CustomTreeViewItem parentItem = item.Parent as CustomTreeViewItem;
                        parentItem.Items.Remove(item);
                    }
                    else
                        AppHelpers.ShowError("Failed to remove the folder.");
                }
                else
                {
                    if (SharedDocumentsUploader.RemoveFile(item.File))
                    {
                        CustomTreeViewItem parentItem = item.Parent as CustomTreeViewItem;
                        parentItem.Items.Remove(item);
                    }
                    else
                        AppHelpers.ShowError("Failed to remove the file.");
                }
            }
        }

        private void btnUploadFile_Click(object sender, RoutedEventArgs e)
        {
            CustomTreeViewItem item = GetSelectedFolderItem();
            if (item == null)
                return;

            if (AppHelpers.UsesServerMessage() && AppHelpers.IsLoggedInMessage())
            {
                UploadDocsWindow window = new UploadDocsWindow();
                window.Folder = item.Folder;
                if (window.ShowDialog() == true)
                {
                    if (window.File != null)
                    {
                        item.Folder.AddFile(window.File);
                        AddFileToTree(window.File, item);
                    }
                }
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            CustomTreeViewItem item = GetSelectedItem();
            if (item == null || item.File == null)
                return;

            AppHelpers.OpenProxyUrl(HttpLocalResponder.GetSharedFilePath(item.File));
        }
    }

    class CustomTreeViewItem : TreeViewItem
    {
        SharedFolder _folder = null;
        SharedFile _file = null;

        private string _iconPath;
        private string _name;

        public string ItemName { get { return _name; } }

        public SharedFolder Folder
        {
            get { return _folder; }
            set {
                _folder = value;
                if (_folder != null)
                {
                    SetHeader(_folder.Name, Controller.GetAppResourcePath("Html\\graphics\\files\\folder.png"));
                }
            }
        }

        public SharedFile File
        {
            get { return _file; }
            set {
                _file = value;
                if (_file != null)
                {
                    SetHeader((String.IsNullOrWhiteSpace(_file.Title)) ? _file.FileName : _file.Title,
                        Controller.GetAppResourcePath(String.Format("Html\\graphics\\files\\{0}", AppHelpers.GetFileIconName(_file.FileName))));
                }
            }
        }

        public bool IsFolder { get { return _folder != null; } }

        private void SetHeader(string name, string iconPath)
        {
            _iconPath = iconPath;
            UpdateHeader(name);
        }

        public void UpdateHeader(string name)
        {
            _name = name;
            StackPanel pan = new StackPanel();
            pan.Orientation = Orientation.Horizontal;
            BitmapImage bmi = new BitmapImage(new Uri(_iconPath, UriKind.Absolute));
            Image image = new Image();
            image.Height = 16;
            image.Source = bmi;
            pan.Children.Add(image);
            pan.Children.Add(new TextBlock(new Run("  " + name)));
            Header = pan;
        }
    }
}
