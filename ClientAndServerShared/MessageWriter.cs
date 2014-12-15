
namespace ClientAndServerShared
{
    public class MessageWriter : Helpers.IMessageWriter
    {
        public void WriteException(string message)
        {
            LogsController.WriteException(message);
        }

        public void WriteException(string location, string message)
        {
            LogsController.WriteException(location, message);
        }
    }
}
