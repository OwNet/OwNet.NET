using Helpers;

namespace ClientAndServerShared
{
    public class ClientAndServerController
    {
        private static IAppSettings _appSettings = null;
        public static IAppSettings AppSettings
        {
            get { return _appSettings; }
            set { _appSettings = value; }
        }
    }
}
