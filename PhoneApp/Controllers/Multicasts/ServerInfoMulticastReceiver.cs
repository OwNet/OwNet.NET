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
using System.Threading;
using System.Net.Sockets;
using System.Text;

namespace PhoneApp.Controllers.Multicasts
{
    public class ServerInfoMulticastReceiver
    {
        // The address of the multicast group to join.
        // Must be in the range from 224.0.0.0 to 239.255.255.255
        static string GROUP_ADDRESS = "224.0.1.1";

        // The port over which to communicate to the multicast group
        static int GROUP_PORT = 52274;

        // A client receiver for multicast traffic from any source
        static UdpAnySourceMulticastClient _client = null;

        // Buffer for incoming data
        private static byte[] _receiveBuffer;

        // Maximum size of a message in this communication
        static int MAX_MESSAGE_SIZE = 512;

        static bool _running = false;
        static object _runningLock = new object();

        public static bool Running
        {
            get
            {
                if (UsersController.AuthenticatedUser == null)
                    return false;
                
                if (ServersController.IsServerKnown)
                    return false;

                lock (_runningLock)
                    return _running;
            }
        }

        static Thread _receiverThread;

        public static void Start()
        {
            lock (_runningLock)
            {
                if (_running)
                    return;
                _running = true;
            }

            if (_receiverThread == null)
            {
                _receiverThread = new Thread(new ThreadStart(Join));
                _receiverThread.Start();
            }
        }

        public static void Stop()
        {
            lock (_runningLock)
            {
                _running = false;
                _receiverThread = null;
            }
        }

        static void Join()
        {
            try
            {
                // Initialize the receive buffer
                _receiveBuffer = new byte[MAX_MESSAGE_SIZE];

                // Create the UdpAnySourceMulticastClient instance using the defined 
                // GROUP_ADDRESS and GROUP_PORT constants. UdpAnySourceMulticastClient is a 
                // client receiver for multicast traffic from any source, also known as Any Source Multicast (ASM)
                _client = new UdpAnySourceMulticastClient(IPAddress.Parse(GROUP_ADDRESS), GROUP_PORT);

                // Make a request to join the group.
                _client.BeginJoinGroup(
                    result =>
                    {
                        try
                        {
                            // Complete the join
                            _client.EndJoinGroup(result);

                            // The MulticastLoopback property controls whether you receive multicast 
                            // packets that you send to the multicast group. Default value is true, 
                            // meaning that you also receive the packets you send to the multicast group. 
                            // To stop receiving these packets, you can set the property following to false
                            _client.MulticastLoopback = true;
                        }
                        catch (Exception ex)
                        {
                            LogsController.WriteException("JoinMulticastResult", ex.Message);
                        }

                        // Wait for data from the group. This is an asynchronous operation 
                        // and will not block the UI thread.
                        Receive();
                    }, null);
            }
            catch (Exception ex)
            {
                LogsController.WriteException("JoinMulticast", ex.Message);
            }
        }

        static void Receive()
        {
            try
            {
                while (Running)
                {
                    Array.Clear(_receiveBuffer, 0, _receiveBuffer.Length);
                    _client.BeginReceiveFromGroup(_receiveBuffer, 0, _receiveBuffer.Length,
                        result =>
                        {
                            IPEndPoint source;
                            try
                            {
                                // Complete the asynchronous operation. The source field will 
                                // contain the IP address of the device that sent the message
                                _client.EndReceiveFromGroup(result, out source);

                                // Get the received data from the buffer.
                                string[] dataReceived = GetString(_receiveBuffer).Split('$');
                                if (dataReceived.Length > 0)
                                {
                                    string serverName = dataReceived[0];
                                    if (UsersController.AuthenticatedUser != null &&
                                        serverName == UsersController.AuthenticatedUser.ServerUsername)
                                    {
                                        ServersController.SetServerAddress(serverName, source.Address.ToString());
                                        return;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogsController.WriteException("ReceiveMulticastResult", ex.Message);
                            }
                            finally
                            {
                                lock (_runningLock)
                                    _running = false;
                            }
                        }, null);
                }
            }
            catch (SocketException ex)
            {
                LogsController.WriteException("ReceiveServerInfo", ex.Message);
            }
            catch (Exception ex)
            {
                LogsController.WriteException("ReceiveServerInfo", ex.Message);
            }
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}
