using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;

namespace SharedProxy.Cache
{
    public class GDSFClock
    {
        protected static double _lastClock = 0;
        private static double _lastSavedClock = 0;
        private static object _lastClockLock = new object();

        public static double LastClock
        {
            get
            {
                lock (_lastClockLock)
                    return _lastClock;
            }
            set
            {
                lock (_lastClockLock)
                    _lastClock = value;
            }
        }

        public static void Init()
        {
            try
            {
                string fileName = GetCostFilePath();
                if (File.Exists(fileName))
                    using (FileStream fs = File.Open(fileName, FileMode.Open,
                        FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (BinaryReader reader = new BinaryReader(fs))
                        {
                            _lastSavedClock = LastClock = reader.ReadInt64();
                        }
                        fs.Close();
                    }
            }
            catch (Exception)
            {
                _lastSavedClock = LastClock = 0;
            }
        }

        public static void SaveLastClock()
        {
            double clock = 0;
            lock (_lastClockLock)
            {
                if (_lastSavedClock == _lastClock)
                    return;
                clock = _lastClock;
            }

            try
            {
                using (FileStream fs = File.Open(GetCostFilePath(), FileMode.OpenOrCreate,
                    FileAccess.Write, FileShare.ReadWrite))
                {
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        writer.Write(clock);
                        writer.Flush();
                    }
                    fs.Close();
                }

            }
            catch (Exception ex)
            {
                Controller.WriteException("Save access count", ex.Message);
            }
        }

        public static string GetCostFilePath()
        {
            return Path.Combine(Controller.AppSettings.AppDataFolder(), "GDSFClock.txt");
        }
    }
}
