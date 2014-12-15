using System;

namespace ClientAndServerShared
{
    public static class LogsController
    {
        public static LogsCollection<string> ExceptionsList
        {
            get;
            private set;
        }

        public static LogsCollection<string> PageAccessList
        {
            get;
            private set;
        }

        public static LogsCollection<string> MessagesList
        {
            get;
            private set;
        }

        public static void Init()
        {
            ExceptionsList = new LogsCollection<string>();
            PageAccessList = new LogsCollection<string>();
            MessagesList = new LogsCollection<string>();
            Helpers.Messages.MessageWriter = new MessageWriter();
        }

        public static void Show()
        {
            new LogsWindow().Show();
        }

        public static void WriteException(string message, bool important = false)
        {
            ExceptionsList.Add(String.Format("{0}: {1}", DateTime.Now.ToString(), message));
            if (important)
                WriteMessage(message);
        }

        public static void WriteException(string occurence, string message, bool important = false)
        {
            WriteException(String.Format("{0} - {1}", occurence, message), important);
        }

        public static void WriteMessage(string message)
        {
            MessagesList.Add(String.Format("{0}: {1}", DateTime.Now.ToString(), message));
        }

        public static void LogPageAccess(string url)
        {
            PageAccessList.Add(String.Format("{0}: {1}", DateTime.Now.ToString(), url));
        }
    }
}
