using System;
using System.Windows;

namespace WPFProxy
{
    class AppHelpers
    {
        public static void ShowError(string message)
        {
            ShowMessage(message, MessageBoxImage.Error);
        }

        public static void ShowInformation(string message)
        {
            ShowMessage(message, MessageBoxImage.Information);
        }

        public static void ShowWarning(string message)
        {
            ShowMessage(message, MessageBoxImage.Warning);
        }

        public static void ShowMessage(string message, MessageBoxImage icon)
        {
            MessageBox.Show(message, Helpers.Common.AppName, MessageBoxButton.OK, icon);
        }

        public static bool ShowQuestion(string question, bool defaultYes = true)
        {
            return MessageBox.Show(question, Helpers.Common.AppName,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                defaultYes ? MessageBoxResult.Yes : MessageBoxResult.No)
                    == MessageBoxResult.Yes;
        }

        public static bool IsConnectedToInternet()
        {
            return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
        }

        public static bool UsesDataServiceMessage()
        {
            if (!Controller.UseDataService)
                AppHelpers.ShowError("Server is not set up.");

            return Controller.UseDataService;
        }

        public static bool UsesServerMessage()
        {
            if (!Controller.UseServer)
                AppHelpers.ShowError("Server is not set up.");

            return Controller.UseServer;
        }

        public static bool IsLoggedInMessage()
        {
            if (Settings.IsLoggedIn())
                return true;
            if (!UsesServerMessage())
                return false;

            if (new LoginWindow().ShowDialog() ?? false)
                return true;

            ShowError("Failed to login.");
            return false;
        }

        public static bool IsAuthorizedMessage(int creator = -1)
        {
            if (Settings.IsTeacher() || (creator >= 0 && creator == Settings.UserID))
                return true;

            ShowError("You don't have the permission to perform this action.");
            return false;
        }

        public static bool OpenProxyUrl(string url)
        {
            if (!Controller.ProxyIsRunning)
            {
                ShowError("You have to start the proxy server by clicking the Start button on the main window first.");
                return false;
            }

            System.Diagnostics.Process.Start(url);
            return true;
        }

        public static string GetFileIconRelativePath(string fileName)
        {
            return String.Format("graphics/files/{0}", GetFileIconName(fileName));
        }

        public static string GetFileIconName(string fileName)
        {
            if (fileName != null)
            {
                string[] parts = fileName.Split(new char[] { '.' });
                string extension = "";
                if (parts.Length > 0)
                    extension = parts[parts.Length - 1];

                if (extension != "")
                    if (new System.IO.FileInfo(Controller.GetAppResourcePath(String.Format("Html/graphics/files/{0}.png", extension))).Exists)
                        return String.Format("{0}.png", extension);
            }

            return "_blank.png";
        }
    }
}
