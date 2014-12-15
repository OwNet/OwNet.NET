using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace PhoneApp.Views
{
    public partial class SharedFolderPage : PhoneApplicationPage
    {
        public SharedFolderPage()
        {
            InitializeComponent();

            FolderTitle.Text = Controllers.SharedFilesController.CurrentFolder.Name;
            SharedFilesList.ItemsSource = Controllers.SharedFilesController.CurrentFolder.AllItems;
        }

        private void SharedFilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ServiceEntities.SharedItem item = (ServiceEntities.SharedItem)e.AddedItems[0];

                if (item.IsFolder)
                {
                    Controllers.SharedFilesController.CurrentFolder = (ServiceEntities.SharedFolder)item;
                    SharedFilesList.ItemsSource = Controllers.SharedFilesController.CurrentFolder.AllItems;
                }
                else
                {
                    Controllers.SharedFilesController.OpenFile((ServiceEntities.SharedFile)item);
                }
            }
        }
    }
}