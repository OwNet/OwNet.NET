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
using Microsoft.Win32;
using System.IO;
using System.Data.Services.Client;
using ServiceEntities;

namespace WPFProxy
{
    /// <summary>
    /// Interaction logic for UploadDocsWindow.xaml
    /// </summary>
    public partial class UploadDocsWindow : Window
    {
        private string _fileName = "";
        private SharedFolder _folder = null;
        private SharedFile _file = null;

        public SharedFile File
        {
            get { return _file; }
        }

        public SharedFolder Folder
        {
            get { return _folder; }
            set { _folder = value; }
        }

        public UploadDocsWindow()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Any file (*.*)|*.*";

            if (dialog.ShowDialog() == true)
            {
                _fileName = dialog.FileName;
                labelFilePath.Content = _fileName;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            if ((_file = SharedDocumentsUploader.UploadFile(_fileName, txtTitle.Text, txtDescription.Text, Folder)) != null)
            {
                AppHelpers.ShowInformation("The file was uploaded succesfully.");
                DialogResult = true;
                Close();
            }
            else
            {
                AppHelpers.ShowError("Failed to upload the file.");
            }
        }
    }
}
