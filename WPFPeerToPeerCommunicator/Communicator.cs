using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows;

namespace WPFPeerToPeerCommunicator
{
    public class Communicator
    {
        private int _broadcastPort = 9050;
        private int _replyPort = 9051;
        private int _broadcastChatPort = 9052;
        private bool _running = false;
        private object _runningLock = new object();

        Thread _receiverThread;
        Thread _chatReceiverThread;
        Thread _senderThread;

        public ConnectedUsers Users = new ConnectedUsers();

        public void Start()
        {
            lock (_runningLock)
                _running = true;

            if (_receiverThread == null)
            {
                _receiverThread = new Thread(new ThreadStart(ReceiveBroadcasts));
                _receiverThread.Start();
            }
            if (_chatReceiverThread == null)
            {
                _chatReceiverThread = new Thread(new ThreadStart(ReceiveBroadcastChatMessages));
                _chatReceiverThread.Start();
            }
            if (_senderThread == null)
            {
                _senderThread = new Thread(new ThreadStart(StartSending));
                _senderThread.Start();
            }
        }

        public void Stop()
        {
            lock (_runningLock)
                _running = false;
        }

        private void StartSending()
        {
            try
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(10000);

                    lock (_runningLock)
                    {
                        if (_running == false)
                        {
                            _senderThread = null;
                            return;
                        }
                    }
                    SendBroadcast();
                }
            }
            catch (Exception ex)
            {
                Controller.WriteException("StartSending()", ex.Message);
            }
        }

        private void SendBroadcast()
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                ProtocolType.Udp);
            IPEndPoint iep1 = new IPEndPoint(IPAddress.Broadcast, _broadcastPort);

            BroadcastMessage message = new BroadcastMessage();
            message.Hostname = Dns.GetHostName();
            message.Username = Settings.Username;

            MemoryStream stream = new MemoryStream();
            BinaryFormatter bformatter = new BinaryFormatter();

            try
            {
                bformatter.Serialize(stream, message);
                byte[] data = stream.ToArray();

                sock.SetSocketOption(SocketOptionLevel.Socket,
                           SocketOptionName.Broadcast, 1);
                sock.SendTo(data, iep1);
            }
            catch (Exception ex)
            {
                Controller.WriteException("SendBroadcast():", ex.Message);
            }
            finally
            {
                stream.Close();
                sock.Close();
            }
        }

        private void ReceiveBroadcasts()
        {
            Socket sock = new Socket(AddressFamily.InterNetwork,
                               SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, _broadcastPort);
            
            try
            {
                sock.Bind(iep);
                EndPoint ep = (EndPoint)iep;
                
                while (true)
                {
                    //Console.WriteLine("Ready to receive…");
                    byte[] data = new byte[1024];

                    MemoryStream stream = new MemoryStream(data);
                    BinaryFormatter bformatter = new BinaryFormatter();

                    try
                    {
                        int recv = sock.ReceiveFrom(data, ref ep);

                        BroadcastMessage message = (BroadcastMessage)bformatter.Deserialize(stream);

                        if (message.UserID != Settings.ID)
                            ReceivedBroadcastMessage(message, ep);
                    }
                    catch (Exception ex)
                    {
                        Controller.WriteException("ReceiveBroadcasts() [inner]", ex.Message);
//                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        stream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Controller.WriteException("ReceiveBroadcasts()", ex.Message);
               // Console.WriteLine(ex.Message);
            }
            finally
            {
                sock.Close();
            }
        }

        private void ReceivedBroadcastMessage(BroadcastMessage message, EndPoint ep)
        {
            if (message.FirstCompatibleVersion <= Settings.MessagesVersion)
            {
                ConnectedUser user;
                if (Users.ContainsID(message.UserID))
                {
                    user = Users.UserWithID(message.UserID);
                    user.Username = message.Username;
                    user.Update();
                }
                else
                {
                    IPEndPoint iep = ep as IPEndPoint;

                    user = new ConnectedUser();
                    user.UserID = message.UserID;
                    user.Hostname = message.Hostname;
                    user.Username = message.Username;
                    user.IPAddress = iep.Address;

                    Application.Current.Dispatcher.BeginInvoke((Action)delegate()
                    {
                        Users.Add(user);
                    });

                    Console.WriteLine("Received broadcast: {0} {1} from: {2}",
                                           message.Hostname,
                                           Convert.ToString(message.UserID),
                                           ep.ToString());
                }
            }
        }

        private void ReceiveReplies()
        {
            Socket sock = new Socket(AddressFamily.InterNetwork,
                               SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, _replyPort);
            try
            {
                sock.Bind(iep);
                EndPoint ep = (EndPoint)iep;

                while (true)
                {
                    //Console.WriteLine("Ready to receive…");
                    byte[] data = new byte[1024];

                    MemoryStream stream = new MemoryStream(data);
                    BinaryFormatter bformatter = new BinaryFormatter();

                    try
                    {
                        int recv = sock.ReceiveFrom(data, ref ep);

                        ReplyMessage message = (ReplyMessage)bformatter.Deserialize(stream);

                        ReceivedReplyMessage(message);
                    }
                    catch (Exception ex)
                    {
                        Controller.WriteException("ReceiveReplies() [inner]", ex.Message);
//                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        stream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Controller.WriteException("ReceiveReplies()", ex.Message);
             //   Console.WriteLine(ex.Message);
            }
            finally
            {
                sock.Close();
            }
        }

        public void SendBroadcastChatMessage(string body)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                ProtocolType.Udp);
            IPEndPoint iep1 = new IPEndPoint(IPAddress.Broadcast, _broadcastChatPort);

            ChatMessage message = new ChatMessage();
            message.Username = Settings.Username;
            message.MessageBody = body;

            MemoryStream stream = new MemoryStream();
            BinaryFormatter bformatter = new BinaryFormatter();

            try
            {
                bformatter.Serialize(stream, message);
                byte[] data = stream.ToArray();

                sock.SetSocketOption(SocketOptionLevel.Socket,
                           SocketOptionName.Broadcast, 1);
                sock.SendTo(data, iep1);
            }
            catch (Exception ex)
            {
                Controller.WriteException("SendBroadcastChatMessage():", ex.Message);
            }
            finally
            {
                stream.Close();
                sock.Close();
            }
        }

        private void ReceiveBroadcastChatMessages()
        {
            Socket sock = new Socket(AddressFamily.InterNetwork,
                               SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, _broadcastChatPort);

            try
            {
                sock.Bind(iep);
                EndPoint ep = (EndPoint)iep;

                while (true)
                {
                    //Console.WriteLine("Ready to receive…");
                    byte[] data = new byte[1024];

                    MemoryStream stream = new MemoryStream(data);
                    BinaryFormatter bformatter = new BinaryFormatter();

                    try
                    {
                        int recv = sock.ReceiveFrom(data, ref ep);

                        ChatMessage message = (ChatMessage)bformatter.Deserialize(stream);
                        if (message.FirstCompatibleVersion <= Settings.MessagesVersion)
                            Controller.ReceivedBroadcastChatMessage(message);
                    }
                    catch (Exception ex)
                    {
                        Controller.WriteException("ReceiveBroadcasts() [inner]", ex.Message);
                        //                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        stream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Controller.WriteException("ReceiveBroadcasts()", ex.Message);
                // Console.WriteLine(ex.Message);
            }
            finally
            {
                sock.Close();
            }
        }

        public void ReplyToUser(ConnectedUser user)
        {
            if (user.IPAddress == null)
                return;

            Socket sock = new Socket(AddressFamily.InterNetwork,
                                   SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint iep = new IPEndPoint(user.IPAddress, _replyPort);

            MemoryStream stream = new MemoryStream();
            BinaryFormatter bformatter = new BinaryFormatter();

            ReplyMessage replyMessage = new ReplyMessage();
            replyMessage.RecommendedLinks = new string[] { "http://apple.com", "http://sme.sk" };
            replyMessage.Hostname = Dns.GetHostName();

            try
            {
                bformatter.Serialize(stream, replyMessage);
                byte[] data = stream.ToArray();

                sock.SetSocketOption(SocketOptionLevel.Socket,
                           SocketOptionName.Broadcast, 1);
                sock.SendTo(data, iep);
            }
            catch (Exception ex)
            {
                Controller.WriteException("ReplyToUser()", ex.Message);
            }
            finally
            {
                stream.Close();
                sock.Close();
            }
        }

        private void ReceivedReplyMessage(ReplyMessage message)
        {
            Console.WriteLine("Received reply: {0} {1}",
                                       message.Hostname,
                                       Convert.ToString(message.UserID));

            string infoMessage;

            ConnectedUser user = Users.UserWithID(message.UserID);
            if (user != null)
                infoMessage = String.Format("Received reply from {0} containing {1} links.",
                    user.Username, message.RecommendedLinks.Count());
            else
                infoMessage = String.Format("Received reply from {0} containing {1} links.",
                    user.Username, message.RecommendedLinks.Count());

            MessageBox.Show(infoMessage);

            foreach (string link in message.RecommendedLinks)
            {
                Console.WriteLine("Received link: " + link);
            }
        }
    }
}
