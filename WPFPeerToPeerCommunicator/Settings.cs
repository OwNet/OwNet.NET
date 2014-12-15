using System;

namespace WPFPeerToPeerCommunicator
{
    public class Settings
    {
        public static int ID = -1;
        private static string _username = "";

        private static Version _messagesVersion = new Version(0, 1, 2);
        private static Version _messagesFirstCompatibleVersion = new Version(0, 1, 2);

        public static Version MessagesVersion { get { return _messagesVersion; } }
        public static Version MessagesFirstCompatibleVersion { get { return _messagesFirstCompatibleVersion; } }
        public static string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        static Settings()
        {
            ID = Math.Abs(Guid.NewGuid().GetHashCode());
        }
    }
}
