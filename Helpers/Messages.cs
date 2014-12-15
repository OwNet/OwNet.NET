
namespace Helpers
{
    public class Messages
    {
        private static IMessageWriter _messageWriter = null;
        public static IMessageWriter MessageWriter
        {
            set { _messageWriter = value; }
        }

        public static void WriteException(string header, string body)
        {
            _messageWriter.WriteException(header, body);
        }

        public static void WriteException(string body)
        {
            _messageWriter.WriteException(body);
        }
    }
}
