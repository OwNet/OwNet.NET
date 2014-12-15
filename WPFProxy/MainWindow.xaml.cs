using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPFProxy.Proxy;
using WPFProxy.Cache;
using ClientAndServerShared;

namespace WPFProxy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ILoginNotified
    {
        private System.Windows.Forms.NotifyIcon trayIcon;
        public MainWindow()
        {
            //CreateDatabaseEntry();
            Controller.Init();

            InitializeComponent();

            Settings.LoginNotified = this;
            labelVersion.Content = "v" + Settings.VersionString;

            trayIcon = new System.Windows.Forms.NotifyIcon();
            trayIcon.BalloonTipText = "Click to open.";
            trayIcon.BalloonTipTitle = "OwNet Client";
            trayIcon.Text = "OwNet Client";

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


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = e.OriginalSource as MenuItem;
            if (null != item)
            {
                switch (item.Name)
                {
                    case "menuItemExit":
                        Close();
                        break;

                    case "menuItemShowLogs":
                        LogsController.Show();
                        break;

                    case "menuItemOptions":
                        new Options().ShowDialog();
                        break;

                    case "menuItemPingServer":
                        ServiceCommunicator.Ping();
                        break;

                    case "menuItemCleanCache":
                        DeleteCache();
                        break;
                }
            }
        }

        private void DeleteCache()
        {
            if (AppHelpers.IsLoggedInMessage() && AppHelpers.ShowQuestion("Are you sure you want to delete the whole cache?", false))
            {
                if (CancelEnabledActions.StartAction(CacheMaintainer.DeleteCache))
                    AppHelpers.ShowInformation("Cache deleted succesfully.");
                else
                    AppHelpers.ShowError("Failed to delete cache");
            }
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Controller.Closing();
            trayIcon.Dispose();
            trayIcon = null;
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            if (AppHelpers.UsesServerMessage())
                new SharedFilesWindow().Show();
        }

        private void buttonChat_Click(object sender, RoutedEventArgs e)
        {
            new WPFPeerToPeerCommunicator.ChatWindow().Show();
        }

        private void buttonMyOwNet_Click(object sender, RoutedEventArgs e)
        {
            AppHelpers.OpenProxyUrl(HttpLocalResponder.BaseUrl);
        }

        public void LoggedIn()
        {
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                labelUsername.Content = Settings.UserName;
                btnLogin.IsEnabled = false;
                btnLogout.IsEnabled = true;
            }));
        }

        public void LoggedOut()
        {
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                labelUsername.Content = "not logged in";
                btnLogin.IsEnabled = true;
                btnLogout.IsEnabled = false;
            }));
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            AppHelpers.IsLoggedInMessage();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            Settings.UserHasLoggedOut();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }
    }

    public interface ILoginNotified
    {
        void LoggedIn();
        void LoggedOut();
    }
}
