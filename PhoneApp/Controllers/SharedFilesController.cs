using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using System.IO;
using PhoneApp.ServiceEntities;
using System.Xml.Linq;
using System.Linq;
using Microsoft.Phone.Tasks;

namespace PhoneApp.Controllers
{
    public class SharedFilesController : Helpers.NotifierObject
    {
        private static SharedFolder _rootFolder = null;

        public SharedFolder RootFolder { get { return _rootFolder; } }

        public void Refresh()
        {
            _rootFolder = null;
            GetSharedFiles();
        }

        public void GetSharedFiles()
        {
            if (_rootFolder != null)
                NotifySuccess();
            else if (ServersController.IsServerKnown)
            {
                try
                {
                    WebClient serviceRequest = new WebClient();
                    serviceRequest.DownloadStringAsync(new Uri(ServersController.GetServiceAddress("activity/share/folders/")));
                    serviceRequest.DownloadStringCompleted += new DownloadStringCompletedEventHandler(serviceRequest_DownloadStringCompleted);
                }
                catch (Exception ex)
                {
                    LogsController.WriteException("GetSharedFilesCall", ex.Message);
                }
            }
        }

        void serviceRequest_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                string res = e.Result;
                XDocument document = XDocument.Parse(res);

                if (document.Root != null)
                {
                    var sharedFolder = ReadSharedFolder(document.Root);
                    _rootFolder = sharedFolder;
                    NotifySuccess();
                }
            }
            catch (Exception ex)
            {
                Controllers.LogsController.WriteException("GetSharedFiles", ex.Message);
                ServersController.ServerAddressInvalid();
            }
        }

        SharedFolder ReadSharedFolder(XElement folderElement)
        {
            SharedFolder folder = new SharedFolder();
            XNode node = folderElement.FirstNode;

            while (node != null)
            {
                var element = node as XElement;
                XNode childNode;
                switch (element.Name.LocalName)
                {
                    case "ChildFolders":
                        childNode = element.FirstNode;
                        while (childNode != null)
                        {
                            var childElement = childNode as XElement;
                            folder.ChildFolders.Add(ReadSharedFolder(childElement));
                            childNode = childNode.NextNode;
                        }
                        break;

                    case "Id":
                        folder.Id = Convert.ToInt32(element.Value);
                        break;

                    case "Name":
                        folder.Name = element.Value;
                        break;

                    case "Files":
                        childNode = element.FirstNode;
                        while (childNode != null)
                        {
                            var childElement = childNode as XElement;
                            folder.Files.Add(ReadSharedFile(childElement));
                            childNode = childNode.NextNode;
                        }
                        break;
                }
                node = node.NextNode;
            }

            return folder;
        }

        SharedFile ReadSharedFile(XElement fileElement)
        {
            SharedFile file = new SharedFile();
            XNode node = fileElement.FirstNode;

            while (node != null)
            {
                var element = node as XElement;

                switch (element.Name.LocalName)
                {
                    case "FileObjectId":
                        file.FileObjectId = Convert.ToInt32(element.Value);
                        break;

                    case "Title":
                        file.Name = element.Value;
                        break;

                    case "Description":
                        file.Description = element.Value;
                        break;

                    case "Username":
                        file.Username = element.Value;
                        break;
                }
                node = node.NextNode;
            }

            return file;
        }

        internal static void OpenFile(SharedFile sharedFile)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.Uri = new Uri(ServersController.GetServiceAddress(string.Format("sharedfiles/SharedFileObjects({0})/$value", sharedFile.FileObjectId)));
            task.Show();
        }

        public static SharedFolder CurrentFolder { get; set; }
    }
}
