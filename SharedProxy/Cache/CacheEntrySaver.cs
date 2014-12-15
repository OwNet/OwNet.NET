using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientAndServerShared;
using System.Threading;
using SharedProxy.Proxy;
using SharedProxy.Streams.Output;

namespace SharedProxy.Cache
{
    public abstract class CacheEntrySaver
    {
        protected Dictionary<int, SaveItem> _items = new Dictionary<int, SaveItem>();
        protected ManualResetEvent _saveSem = null;

        public void IncreaseAccessCount(int hash, long Size)
        {
            ManualResetEvent nextSaveSem = null;
            lock (_items)
            {
                if (_items.ContainsKey(hash))
                {
                    if (_saveSem != null)
                        nextSaveSem = _saveSem;
                    else
                        _items[hash].IncreaseAccessCount++;

                }
                else
                {
                    _items[hash] = new SaveItem()
                    {
                        IncreaseAccessCount = 1,
                        Size = Size
                    };
                }
            }
            if (nextSaveSem != null)
            {
                nextSaveSem.WaitOne();
                IncreaseAccessCount(hash, Size);
            }
        }

        public void PlanSave(ProxyEntry entry, CacheWriter writer, bool increaseAccessCount)
        {
            ManualResetEvent nextSaveSem = null;
            Dictionary<int, SaveItem> copyToSave = null;

            lock (_items)
            {
                if (_items.ContainsKey(entry.ID))
                {
                    if (_saveSem != null)
                    {
                        nextSaveSem = _saveSem;
                    }
                    else
                    {
                        SaveItem item = _items[entry.ID];
                        if (item.Entry != null)
                        {
                            nextSaveSem = _saveSem = new ManualResetEvent(false);
                            copyToSave = new Dictionary<int, SaveItem>(_items);
                        }
                        else
                        {
                            if (increaseAccessCount)
                                item.IncreaseAccessCount++;
                            item.Entry = entry;
                            item.Writer = writer;
                        }
                    }
                }
                else
                {
                    _items[entry.ID] = new SaveItem()
                    {
                        IncreaseAccessCount = increaseAccessCount ? 1 : 0,
                        Entry = entry,
                        Writer = writer
                    };
                }
            }
            if (copyToSave != null)
            {
                SaveNow(copyToSave);
                PlanSave(entry, writer, increaseAccessCount);
            }
            else if (nextSaveSem != null)
            {
                nextSaveSem.WaitOne();
                PlanSave(entry, writer, increaseAccessCount);
            }
        }

        internal void Save()
        {
            Dictionary<int, SaveItem> copyToSave = null;
            ManualResetEvent nextSaveSem = null;

            lock (_items)
            {
                if (_saveSem != null)
                    nextSaveSem = _saveSem;
                else
                {
                    _saveSem = new ManualResetEvent(false);
                    copyToSave = new Dictionary<int, SaveItem>(_items);
                }
            }

            if (nextSaveSem == null)
                SaveNow(copyToSave);
            else
                nextSaveSem.WaitOne();
        }

        private void SaveNow(Dictionary<int, SaveItem> copyToSave)
        {
            try
            {
                ExecuteSave(copyToSave);
            }
            catch (Exception ex)
            {
                LogsController.WriteException("SaveNow", ex.Message);
            }
            finally
            {
                lock (_items)
                {
                    foreach (var pair in copyToSave)
                    {
                        if (pair.Value.Writer != null)
                            pair.Value.Writer.Dispose();
                        _items.Remove(pair.Key);
                    }

                    _saveSem.Set();
                    _saveSem = null;
                }
            }
        }

        protected abstract void ExecuteSave(Dictionary<int, SaveItem> copyToSave);

        protected class SaveItem
        {
            public int IncreaseAccessCount = 0;
            public ProxyEntry Entry = null;
            public CacheWriter Writer = null;
            public long Size = 0;
        }
    }
}
