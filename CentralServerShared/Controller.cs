
namespace CentralServerShared
{
    public class Controller
    {
        private static Helpers.IMessageWriter _messageWriter = null;

        public static Helpers.IMessageWriter MessageWriter
        {
            set
            {
                _messageWriter = value;
                Helpers.Messages.MessageWriter = value;
            }
        }

        public static void WriteException(string location, string message)
        {
            if (_messageWriter != null)
                _messageWriter.WriteException(location, message);
        }
    }
}
