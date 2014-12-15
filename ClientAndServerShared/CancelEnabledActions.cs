using System;
using System.Threading;

namespace ClientAndServerShared
{
    public class CancelEnabledActions
    {
        public delegate bool CancelAction(CancelObject cancelObject);

        public static bool StartAction(CancelAction callback, bool cancelEnabled = true, string customMessage = "")
        {
            CancelObject cancelObject = new CancelObject();
            cancelObject.Callback = callback;
            ThreadPool.QueueUserWorkItem(new WaitCallback(StartAction), cancelObject);
            new CancelWindow(cancelObject, cancelEnabled, customMessage).ShowDialog();
            return cancelObject.IsSuccessful;
        }

        private static void StartAction(Object obj)
        {
            CancelObject cancelObject = (CancelObject)obj;

            cancelObject.StartAction();
        }
    }
}
