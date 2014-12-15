using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace PhoneApp.Helpers
{
    public class NotifierObject
    {
        private Helpers.INotifiableObject _notifiableObject = null;
        private bool _asyncInitOver = false;
        private object _asyncInitLock = new object();
        private bool _success = true;

        public Helpers.INotifiableObject NotifiableObject
        {
            set
            {
                lock (_asyncInitLock)
                {
                    _notifiableObject = value;
                    if (_asyncInitOver)
                        NotifyNotifiableObject();
                }
            }
        }

        protected void NotifySuccess()
        {
            lock (_asyncInitLock)
            {
                _success = true;
                _asyncInitOver = true;
                NotifyNotifiableObject();
            }
        }

        protected void NotifyFailed()
        {
            lock (_asyncInitLock)
            {
                _success = false;
                _asyncInitOver = true;
                NotifyNotifiableObject();
            }
        }

        void NotifyNotifiableObject()
        {
            if (_notifiableObject != null)
            {
                if (_success)
                    _notifiableObject.NotifyFinished();
                else
                    _notifiableObject.NotifyFailed();
            }
        }
    }
}
