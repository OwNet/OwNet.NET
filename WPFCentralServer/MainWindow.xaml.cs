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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ClientAndServerShared;

namespace WPFCentralServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, Helpers.IMessageWriter
    {
        public MainWindow()
        {
            Controller.Init();

            InitializeComponent();

            CentralServerShared.Controller.MessageWriter = this;
            listLog.ItemsSource = LogsController.MessagesList;
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            Controller.Start();
            buttonStart.IsEnabled = false;
            buttonStop.IsEnabled = true;
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            Controller.Stop();
            buttonStart.IsEnabled = true;
            buttonStop.IsEnabled = false;
        }

        public void WriteException(string message)
        {
            LogsController.WriteException(message);
        }

        public void WriteException(string location, string message)
        {
            LogsController.WriteException(location, message);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Controller.Stop();
        }

        private void buttonLogs_Click(object sender, RoutedEventArgs e)
        {
            LogsController.Show();
        }

        private void buttonServers_Click(object sender, RoutedEventArgs e)
        {
            new ServersWindow().Show();
        }
    }
}
