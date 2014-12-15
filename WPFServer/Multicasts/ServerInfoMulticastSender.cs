using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace WPFServer.Multicasts
{
    class ServerInfoMulticastSender
    {
        // The address of the multicast group to join.
        // Must be in the range from 224.0.0.0 to 239.255.255.255
        static string GROUP_ADDRESS = "224.0.1.1";

        // The port over which to communicate to the multicast group
        static int GROUP_PORT = 52274;

        static bool _running = true;
        static object _runningLock = new object();

        public static bool Running { get { lock (_runningLock) return _running; } }

        static Thread _senderThread;

        public static void Start()
        {
            lock (_runningLock)
                _running = true;

            if (_senderThread == null)
            {
                _senderThread = new Thread(new ThreadStart(StartSending));
                _senderThread.Start();
            }
        }

        public static void Stop()
        {
            lock (_runningLock)
                _running = false;
        }

        static void StartSending() 
        {
            IPAddress ip;
            try
            {
                ip = IPAddress.Parse(GROUP_ADDRESS);

                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip));
                s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 1);

                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(GROUP_ADDRESS), GROUP_PORT);
                s.Connect(ipep);

                while (Running)
                {
                    byte[] bytes = GetBytes(Server.Settings.ClientName() + "$");
                    s.Send(bytes, bytes.Length, SocketFlags.None);

                    System.Threading.Thread.Sleep(1000);
                }

                s.Close();
            }
            catch (System.Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
