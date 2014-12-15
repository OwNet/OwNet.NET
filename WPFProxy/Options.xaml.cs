using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace WPFProxy
{
    /// <summary>
    /// Interaction logic for Preferences.xaml
    /// </summary>
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
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

        private void buttonTestConnection_Click(object sender, RoutedEventArgs e)
        {
            ServiceCommunicator.Ping();
        }
    }
}
