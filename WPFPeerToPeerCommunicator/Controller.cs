
namespace WPFPeerToPeerCommunicator
{
    public class Controller
    {
        private static Communicator _communicator = new Communicator();
        private static ChatWindow _chatWindow;

        public static Communicator Communicator { get { return _communicator; } }
        public static ChatWindow ChatWindow { set { _chatWindow = value; } }

        public static void WriteException(string location, string message)
        {
        }

        public static void StartChat()
        {
            _communicator.Start();
        }

        public static void StopChat()
        {
            _communicator.Stop();
        }

        public static void ReceivedBroadcastChatMessage(ChatMessage message)
        {
            if (_chatWindow != null)
                _chatWindow.ReceivedMessage(message.Username, message.MessageBody);
        }

        public static void SendBroadcastChatMessage(string body)
        {
            if (_chatWindow != null)
                _communicator.SendBroadcastChatMessage(body);
        }
    }
}
