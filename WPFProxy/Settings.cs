using System;
using System.Collections.Generic;
using ClientAndServerShared;

namespace WPFProxy
{
    public static class Settings
    {
        public static int ID = -1;
        public static Dictionary<string, string> UserInfo = new Dictionary<string, string>();

        private static string _userName = "";

        private static string _userFirstName = "";
        private static string _userSurname = "";
        private static string _userEmail = "";
        private static bool _userTeacher = false;
        private static bool _userMale = false;
        private static int _userID = -1;

        private static Version _version = new Version(0, 1, 10);
        private static Version _databaseVersion = new Version(0, 2, 7);

        public static Version Version { get { return _version; } }
        public static Version DatabaseVersion { get { return _databaseVersion; } }

        public static string VersionString { get { return _version.ToString(3); } }
        public static string DatabaseVersionString { get { return _databaseVersion.ToString(3); } }

        public static ILoginNotified LoginNotified = null;

        public static bool RealTimePrefetchingEnabled { get { return Properties.Settings.Default.RealTimePrefetching; } }

        public static void Init()
        {
            ID = Math.Abs(Guid.NewGuid().GetHashCode());
        }

        public static string UserFirstname
        {
            get { return _userFirstName; }
            set { _userFirstName = value; }
        }

        public static string UserSurname
        {
            get { return _userSurname; }
            set { _userSurname = value; }
        }

        public static string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        public static string UserEmail
        {
            get { return _userEmail; }
            set { _userEmail = value; }
        }

        public static int UserID
        {
            get { return _userID; }
            set { _userID = value; }
        }

        public static bool UserTeacher
        {
            get { return _userTeacher; }
            set { _userTeacher = value; }
        }

        public static bool UserMale
        {
            get { return _userMale; }
            set { _userMale = value; }
        }

        public static string ClientName
        {
            get { return Properties.Settings.Default.ClientName; }
        }

        public static void UserHasLoggedIn(string name, string firstname, string surname, string email, int id, bool teacher, bool ismale)
        {
            UserFirstname = firstname;
            UserSurname = surname;
            UserName = name;
            UserID = id;
            UserTeacher = teacher;
            UserEmail = email;
            UserMale = ismale;

            LogsController.WriteMessage(UserName + " logged in.");

            if (LoginNotified != null)
                LoginNotified.LoggedIn();
        }

        public static void UserHasLoggedOut()
        {
            if (UserName != "")
                LogsController.WriteMessage(UserName + " logged out.");

            UserName = "";
            UserSurname = "";
            UserName = "";
            UserFirstname = "";
            UserEmail = "";
            UserTeacher = false;
            UserID = -1;
            UserMale = true;

            if (LoginNotified != null)
                LoginNotified.LoggedOut();
        }

        public static bool IsLoggedIn()
        {
            return _userID >= 0;
        }

        public static bool IsTeacher()
        {
            return IsLoggedIn() && UserTeacher;
        }

        public static bool LogIn(string username, string password)
        {
            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
                return false;

            try
            {
                if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password))
                {
                    Settings.UserHasLoggedOut();

                    ServiceEntities.UserLogsIn pobj = new ServiceEntities.UserLogsIn();
                    pobj.Username = username;
                    pobj.Password = password;
                    ServiceEntities.UserLoggedIn ret = null;

                    if ((ret = ServiceCommunicator.UserLogIn(pobj)).Success == true)
                    {
                        Settings.UserHasLoggedIn(ret.User.Username, ret.User.Firstname, ret.User.Surname, ret.User.Email, ret.User.Id, ret.User.IsTeacher, ret.User.IsMale);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                LogsController.WriteException("LoginUser()", e.Message);
            }
            return false;
        }

        public static bool DownloadEverythingThroughServer
        {
            get { return Properties.Settings.Default.DownloadEverythingThroughServer; }
            set
            {
                if (value != Properties.Settings.Default.DownloadEverythingThroughServer)
                {
                    Properties.Settings.Default.DownloadEverythingThroughServer = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        internal static void Update(ServiceEntities.ServerSettings settings)
        {
            if (settings == null) return;

            DownloadEverythingThroughServer = settings.DownloadEverythingThroughServer;
            Cache.CacheUpdater.ProcessLinksToUpdate(settings.Updates);
        }
    }
}
