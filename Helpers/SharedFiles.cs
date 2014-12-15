using System;

namespace Helpers
{
    public class SharedFiles
    {
        public static int CreateSharedFileId(string fileName)
        {
            return (fileName + DateTime.Now.ToString("yyyyMMddHHmmssffff")).GetHashCode();
        }
    }
}
