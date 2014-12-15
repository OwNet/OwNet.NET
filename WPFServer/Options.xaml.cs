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
using System.Windows.Forms;

namespace WPFServer
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void txtMaximumCacheSize_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Helpers.Common.AreAllValidNumericChars(e.Text);
            base.OnPreviewTextInput(e);
        }

        private void btnBrowseAppDataFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog browseDialog = new FolderBrowserDialog();

            if (browseDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtAppDataFolder.Text = browseDialog.SelectedPath;
            }
        }
    }
}
