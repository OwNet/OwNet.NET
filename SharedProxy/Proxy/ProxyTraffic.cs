using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedProxy.Proxy
{
    public class ProxyTraffic
    {
        private static int _trafficArraySize = 60;
        private static int _trafficArrayIndex = 0;
        private static int _currentTraffic = 0;
        private static int[] _traffic = new int[_trafficArraySize];
        private static object _currentTrafficLock = new object();

        static ProxyTraffic()
        {
            for (int i = 0; i < _trafficArraySize; ++i)
                _traffic[i] = 0;
        }

        public static void IncreaseCurrentTraffic()
        {
            lock (_currentTrafficLock)
            {
                _currentTraffic++;
            }
        }

        internal static void TakeCurrentTrafficSnapshot()
        {
            lock (_currentTrafficLock)
            {
                _traffic[_trafficArrayIndex] = _currentTraffic;
                _trafficArrayIndex++;
                _trafficArrayIndex %= _trafficArraySize;
                _currentTraffic = 0;
            }
        }

        public static int LastTraffic()
        {
            int trafficSum = 0;

            for (int i = 0; i < _trafficArraySize; ++i)
                trafficSum += _traffic[i];

            return trafficSum;
        }
    }
}
