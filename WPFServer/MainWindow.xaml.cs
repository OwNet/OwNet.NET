using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ClientAndServerShared;
using WPFServer.DatabaseContext;

namespace WPFServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IServerView
    {
        private System.Windows.Forms.NotifyIcon trayIcon;

        public MainWindow()
        {
            Server.ServerView = this;

            Server.Init();
            
            InitializeComponent();
            InfoBox.ItemsSource = LogsController.MessagesList;
            txtServerName.Text = Properties.Settings.Default.ServerName;
            labelVersion.Content = "v" + AppSettings.VersionString;

            trayIcon = new System.Windows.Forms.NotifyIcon();
            trayIcon.BalloonTipText = "Click to open.";
            trayIcon.BalloonTipTitle = "OwNet Server";
            trayIcon.Text = "OwNet Server";

            System.Drawing.Icon appIcon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
            trayIcon.Icon = new System.Drawing.Icon(appIcon, appIcon.Size);

            trayIcon.Click += new EventHandler(trayIcon_Click);
        }
        private WindowState storedWindowState = WindowState.Normal;
        void OnStateChanged(object sender, EventArgs args)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                if (trayIcon != null)
                    trayIcon.ShowBalloonTip(2000);
            }
            else
                storedWindowState = WindowState;
        }

        void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            CheckTrayIcon();
        }
        void trayIcon_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = storedWindowState;
        }
        void CheckTrayIcon()
        {
            ShowTrayIcon(!IsVisible);
        }

        void ShowTrayIcon(bool show)
        {
            if (trayIcon != null)
                trayIcon.Visible = show;
        }

        private void Button_Start_clicked(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;

            Server.Start();
            string servaddress = Server.GetAddress();
            string port = Server.GetPort();

            StopButton.IsEnabled = true;

            LogsController.WriteMessage("Server is running on " + servaddress);
        }

        private void Button_Stop_clicked(object sender, RoutedEventArgs e)
        {
            StopServer();
            LogsController.WriteMessage("Server is not running, press Start to run.");
        }

        private void StopServer()
        {
            Server.Stop();
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = e.OriginalSource as MenuItem;
            if (null != item)
            {
                switch (item.Name)
                {
                    //case "menuItemSendReport":
                    //    Server.Stop();
                    //    StartButton.IsEnabled = false;
                    //    StopButton.IsEnabled = false;
                    //    InfoBox.Items.Add(DateTime.Now.ToString("HH:mm:ss") + ": Server attempts to send report.");
                    //    if (Server.SendDump())
                    //    {
                    //        InfoBox.Items.Add(DateTime.Now.ToString("HH:mm:ss") + ": Server report sent successfully.");
                    //    }
                    //    else
                    //    {
                    //        InfoBox.Items.Add(DateTime.Now.ToString("HH:mm:ss") + ": Error.");
                    //    }
                    //    StartButton.IsEnabled = true;
                    //    break;

                    case "menuItemExit":
                        Close();
                        break;

                    case "menuItemShowLogs":
                        LogsController.Show();
                        break;

                    case "menuItemClearCache":
                        // Display message box
                        MessageBoxResult result = MessageBox.Show("Do you really want to clear cache?", "Clear Cache", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        // Process message box results
                        switch (result)
                        {
                            case MessageBoxResult.Yes:        
                                Server.Stop();
                                LogsController.WriteMessage("Cleaning server cache.");
                                StartButton.IsEnabled = false;
                                StopButton.IsEnabled = false;
                                if (Server.ResetCache())
                                {
                                    StartButton.IsEnabled = true;
                                    LogsController.WriteMessage("Server cache cleaned successfully.");
                                }
                                else
                                {
                                    LogsController.WriteMessage("Error occured while server cache clean operation.");
                                }
                                break;
                            case MessageBoxResult.No:
                            case MessageBoxResult.Cancel:
                                return;
                        }        
                        break;
                    case "menuItemClearContent":
                        MessageBoxResult result2 = MessageBox.Show("Do you really want to clear content?", "Clear Content", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        // Process message box results
                        switch (result2)
                        {
                            case MessageBoxResult.Yes:        
                                Server.Stop();
                                LogsController.WriteMessage("Cleaning server content.");
                                StartButton.IsEnabled = false;
                                StopButton.IsEnabled = false;
                                if (Server.ResetContent())
                                {
                                    StartButton.IsEnabled = true;
                                    LogsController.WriteMessage("Server content cleaned successfully.");
                                }
                                else
                                {
                                    LogsController.WriteMessage("Error occured while server content clean operation.");
                                }
                                break;
                            case MessageBoxResult.No:
                            case MessageBoxResult.Cancel:
                                return;
                        }        
                        break;
                    case "menuItemOptions":
                        new Options().ShowDialog();
                        break;

                    case "menuItemUsers":
                        new RegisterTeacherWindow().ShowDialog();
                        break;

                    case "menuItemReportNow":
                        CancelEnabledActions.StartAction(CentralService.CentralServiceCommunicator.ReportNow,
                            false, "Reporting to central service...");
                        break;

                    case "menuItemGetUpdatesNow":
                        CancelEnabledActions.StartAction(CentralService.CentralServiceCommunicator.GetUpdatesNow,
                            false, "Getting updates from central service...");
                        break;
                }
            }
        }

        public void WriteException(string message)
        {
            LogsController.WriteException(message);
        }

        public void WriteException(string location, string message)
        {
            LogsController.WriteException(location, message);
        }

        public void RequireTeacher()
        {
            MessageBox.Show("No teacher is registered in system.\nRegister new teacher.","No Teacher",MessageBoxButton.OK, MessageBoxImage.Warning);

            new RegisterTeacherWindow().ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            StopServer();
            LogsController.WriteMessage("Attempting to close OwNet Server.");
            trayIcon.Dispose();
            trayIcon = null;
        }

        private void txtServerName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.ServerName = txtServerName.Text;
            Properties.Settings.Default.Save();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }


    }
}
