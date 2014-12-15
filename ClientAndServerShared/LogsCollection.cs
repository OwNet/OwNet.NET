using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace ClientAndServerShared
{
    public class LogsCollection<T> : ObservableCollection<T>
    {
        public int MaximumSize = 100;
        private Queue<T> _notAddedItems = new Queue<T>();
        private DispatcherTimer _addItemsTimer = null;
        private object _lockObject = new object();

        public LogsCollection()
            : base()
        {
            _addItemsTimer = new DispatcherTimer(DispatcherPriority.Normal);
            _addItemsTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _addItemsTimer.Tick += new EventHandler(AddNotSavedItems);
            _addItemsTimer.Start();
        }

        public new void Add(T value)
        {
            _notAddedItems.Enqueue(value);
        }

        private void AddNotSavedItems(Object sender, EventArgs args)
        {
            try
            {
                lock (_lockObject)
                {
                    while (_notAddedItems.Count > 0)
                        base.Add(_notAddedItems.Dequeue());

                    while (Count > MaximumSize)
                        RemoveAt(0);
                }
            }
            catch (Exception ex)
            {
                LogsController.WriteException("AddNotSavedItems", ex.Message);
            }
        }
    }
}
