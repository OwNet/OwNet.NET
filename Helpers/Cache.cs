using System.Security.Cryptography;
using System.Text;

namespace Helpers
{
    public static class Cache
    {
        private static int _maximumCacheReserve = 100;
        private static int _minimumCacheSize = 10;
        public static long MaximumCacheReserve { get { return (long)_maximumCacheReserve * Helpers.Common.BytesInMB; } }
        public static long MinimumCacheSize { get { return (long)_minimumCacheSize * Helpers.Common.BytesInMB; } }

        public static string GetMD5HashFromFile(byte[] bytes)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(bytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
