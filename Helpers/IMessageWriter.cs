
namespace Helpers
{
    public interface IMessageWriter
    {
        void WriteException(string message);
        void WriteException(string location, string message);
    }
}
