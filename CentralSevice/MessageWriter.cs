
namespace CentralService
{
    public class MessageWriter : Helpers.IMessageWriter
    {
        public void WriteException(string message)
        {
            Controller.WriteException(message);
        }

        public void WriteException(string location, string message)
        {
            Controller.WriteException(location, message);
        }
    }
}