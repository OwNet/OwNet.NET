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

namespace WPFProxy
{
    /// <summary>
    /// Interaction logic for UploadDocsCreateFolderWindow.xaml
    /// </summary>
    public partial class UploadDocsCreateFolderWindow : Window
    {
        public UploadDocsCreateFolderWindow()
        {
            InitializeComponent();
        }

        public string ResponseText
        {
            get { return txtFolderName.Text; }
            set { txtFolderName.Text = value; }
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
