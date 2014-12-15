using System.Threading;

namespace ClientAndServerShared
{
    public class CancelObject
    {
        private bool _canceled = false;
        private bool _successful = false;
        private object _lock = new object();
        private CancelEnabledActions.CancelAction _callback = null;
        private Semaphore _finishedSem = new Semaphore(0, 1);
        private INotifyFinished _notifyFinished = null;

        public INotifyFinished NotifyFinished
        {
            set { _notifyFinished = value; }
        }

        public bool IsSuccessful { get { return _successful; } }
        
        public bool IsCanceled
        {
            get
            {
                lock (_lock)
                    return _canceled;
            }
            set
            {
                lock (_lock)
                    _canceled = value;
            }
        }

        public CancelEnabledActions.CancelAction Callback
        {
            set { _callback = value; }
        }

        public void StartAction()
        {
            if (_callback != null)
                _successful = _callback(this);
            _finishedSem.Release();
            if (_notifyFinished != null)
                _notifyFinished.Finished();
        }

        public void WaitUntilFinished()
        {
            _finishedSem.WaitOne();
            _finishedSem.Release();
        }
    }

    public interface INotifyFinished
    {
        void Finished();
    }
}
